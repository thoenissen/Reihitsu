using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Splits field declarations that declare multiple variables
/// </summary>
internal sealed class FieldDeclarationSplitTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">Formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public FieldDeclarationSplitTransform(FormattingContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the indentation trivia for additional generated fields
    /// </summary>
    /// <param name="leadingTrivia">The original leading trivia</param>
    /// <returns>The indentation trivia</returns>
    private static SyntaxTriviaList GetMemberIndentationTrivia(SyntaxTriviaList leadingTrivia)
    {
        var lastEndOfLineIndex = -1;

        for (var triviaIndex = 0; triviaIndex < leadingTrivia.Count; triviaIndex++)
        {
            if (leadingTrivia[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                lastEndOfLineIndex = triviaIndex;
            }
        }

        if (lastEndOfLineIndex < 0)
        {
            return SyntaxFactory.TriviaList();
        }

        var trivia = new List<SyntaxTrivia>(leadingTrivia.Count - lastEndOfLineIndex - 1);

        for (var triviaIndex = lastEndOfLineIndex + 1; triviaIndex < leadingTrivia.Count; triviaIndex++)
        {
            if (leadingTrivia[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                trivia.Add(leadingTrivia[triviaIndex]);
            }
        }

        return SyntaxFactory.TriviaList(trivia);
    }

    /// <summary>
    /// Splits field declarations in the provided member list
    /// </summary>
    /// <param name="members">The member list</param>
    /// <returns>The updated members</returns>
    private SyntaxList<MemberDeclarationSyntax> SplitFields(SyntaxList<MemberDeclarationSyntax> members)
    {
        var updatedMembers = new List<MemberDeclarationSyntax>(members.Count);
        var indentationTrivia = default(SyntaxTriviaList);
        var lineBreakTrivia = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(_context.EndOfLine));

        foreach (var member in members)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (member is not FieldDeclarationSyntax fieldDeclaration
                || fieldDeclaration.Declaration.Variables.Count <= 1)
            {
                updatedMembers.Add(member);

                continue;
            }

            indentationTrivia = GetMemberIndentationTrivia(fieldDeclaration.GetLeadingTrivia());

            for (var variableIndex = 0; variableIndex < fieldDeclaration.Declaration.Variables.Count; variableIndex++)
            {
                var variable = fieldDeclaration.Declaration.Variables[variableIndex];
                var updatedField = fieldDeclaration.WithDeclaration(fieldDeclaration.Declaration.WithVariables(SyntaxFactory.SingletonSeparatedList(variable)));

                if (variableIndex == 0)
                {
                    updatedField = updatedField.WithLeadingTrivia(fieldDeclaration.GetLeadingTrivia());
                }
                else
                {
                    updatedField = updatedField.WithLeadingTrivia(indentationTrivia);
                }

                updatedField = updatedField.WithTrailingTrivia(variableIndex == fieldDeclaration.Declaration.Variables.Count - 1
                                                                   ? fieldDeclaration.GetTrailingTrivia()
                                                                   : lineBreakTrivia);
                updatedMembers.Add(updatedField);
            }
        }

        return SyntaxFactory.List(updatedMembers);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

        if (node == null)
        {
            return null;
        }

        return node.WithMembers(SplitFields(node.Members));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (StructDeclarationSyntax)base.VisitStructDeclaration(node);

        if (node == null)
        {
            return null;
        }

        return node.WithMembers(SplitFields(node.Members));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (RecordDeclarationSyntax)base.VisitRecordDeclaration(node);

        if (node == null)
        {
            return null;
        }

        return node.WithMembers(SplitFields(node.Members));
    }

    #endregion // CSharpSyntaxVisitor
}