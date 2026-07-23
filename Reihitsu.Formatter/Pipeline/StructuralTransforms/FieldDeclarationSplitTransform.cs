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
    /// Determines whether the member list contains a field declaration that has to be split
    /// </summary>
    /// <param name="members">The member list</param>
    /// <returns><c>true</c> when at least one field declares multiple variables; otherwise <c>false</c></returns>
    private static bool HasFieldToSplit(SyntaxList<MemberDeclarationSyntax> members)
    {
        foreach (var member in members)
        {
            if (member is FieldDeclarationSyntax fieldDeclaration
                && fieldDeclaration.Declaration.Variables.Count > 1)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a field declaration carries a preprocessor directive or disabled text
    /// anywhere in its trivia, which the split would otherwise drop
    /// </summary>
    /// <param name="fieldDeclaration">The field declaration to inspect</param>
    /// <returns><see langword="true"/> if the declaration carries a directive or disabled text; otherwise, <see langword="false"/></returns>
    private static bool CarriesDirective(FieldDeclarationSyntax fieldDeclaration)
    {
        return fieldDeclaration.DescendantTrivia(descendIntoTrivia: true)
                               .Any(ReihitsuFormatterHelpers.IsDirectiveOrDisabledTextTrivia);
    }

    /// <summary>
    /// Gets the comment trivia contained in the provided trivia list
    /// </summary>
    /// <param name="trivia">The trivia list</param>
    /// <returns>The comment trivia</returns>
    private static IEnumerable<SyntaxTrivia> GetComments(SyntaxTriviaList trivia)
    {
        return trivia.Where(item => item.IsKind(SyntaxKind.SingleLineCommentTrivia)
                                    || item.IsKind(SyntaxKind.MultiLineCommentTrivia));
    }

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

        // Collect the whitespace that follows the last end-of-line — the member's indentation. When the field is
        // the first member of its type its leading trivia carries no end-of-line (the newline sits on the opening
        // brace), so fall back to the leading whitespace of the trivia list instead of returning no indentation.
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
    /// Builds the trailing trivia for a split field, re-attaching comments that followed the declarator or its separator
    /// </summary>
    /// <param name="comments">The comments to re-attach</param>
    /// <param name="suffixTrivia">The trivia appended after the comments</param>
    /// <returns>The trailing trivia</returns>
    private static SyntaxTriviaList BuildTrailingTrivia(List<SyntaxTrivia> comments, SyntaxTriviaList suffixTrivia)
    {
        if (comments.Count == 0)
        {
            return suffixTrivia;
        }

        var trivia = new List<SyntaxTrivia>((comments.Count * 2) + suffixTrivia.Count);

        foreach (var comment in comments)
        {
            trivia.Add(SyntaxFactory.Space);
            trivia.Add(comment);
        }

        trivia.AddRange(suffixTrivia);

        return SyntaxFactory.TriviaList(trivia);
    }

    /// <summary>
    /// Builds the leading trivia for a split field, re-attaching standalone comments that preceded the declarator
    /// </summary>
    /// <param name="indentationTrivia">The indentation trivia for the generated field</param>
    /// <param name="declaratorLeadingTrivia">The leading trivia of the declarator</param>
    /// <returns>The leading trivia</returns>
    private SyntaxTriviaList BuildLeadingTrivia(SyntaxTriviaList indentationTrivia, SyntaxTriviaList declaratorLeadingTrivia)
    {
        var trivia = new List<SyntaxTrivia>();

        foreach (var comment in GetComments(declaratorLeadingTrivia))
        {
            trivia.AddRange(indentationTrivia);
            trivia.Add(comment);
            trivia.Add(SyntaxFactory.EndOfLine(_context.EndOfLine));
        }

        trivia.AddRange(indentationTrivia);

        return SyntaxFactory.TriviaList(trivia);
    }

    /// <summary>
    /// Splits field declarations in the provided member list
    /// </summary>
    /// <param name="members">The member list</param>
    /// <returns>The updated members</returns>
    private SyntaxList<MemberDeclarationSyntax> SplitFields(SyntaxList<MemberDeclarationSyntax> members)
    {
        if (HasFieldToSplit(members) == false)
        {
            return members;
        }

        var updatedMembers = new List<MemberDeclarationSyntax>(members.Count);
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

            // Splitting rebuilds each generated field's trivia from comments only. A preprocessor
            // directive or disabled text entangled with the declarators or separators would be dropped,
            // so leave a directive-bearing field declaration intact rather than losing the directive.
            if (CarriesDirective(fieldDeclaration))
            {
                updatedMembers.Add(member);

                continue;
            }

            var indentationTrivia = GetMemberIndentationTrivia(fieldDeclaration.GetLeadingTrivia());
            var variables = fieldDeclaration.Declaration.Variables;

            for (var variableIndex = 0; variableIndex < variables.Count; variableIndex++)
            {
                var variable = variables[variableIndex];
                var updatedField = fieldDeclaration.WithDeclaration(fieldDeclaration.Declaration.WithVariables(SyntaxFactory.SingletonSeparatedList(variable.WithoutTrivia())));

                if (variableIndex == 0)
                {
                    updatedField = updatedField.WithLeadingTrivia(fieldDeclaration.GetLeadingTrivia());
                }
                else
                {
                    updatedField = updatedField.WithLeadingTrivia(BuildLeadingTrivia(indentationTrivia, variable.GetLeadingTrivia()));
                }

                if (variableIndex == variables.Count - 1)
                {
                    var trailingComments = GetComments(variable.GetTrailingTrivia()).ToList();

                    updatedField = updatedField.WithTrailingTrivia(BuildTrailingTrivia(trailingComments, fieldDeclaration.GetTrailingTrivia()));
                }
                else
                {
                    var trailingComments = new List<SyntaxTrivia>();

                    trailingComments.AddRange(GetComments(variable.GetTrailingTrivia()));
                    trailingComments.AddRange(GetComments(variables.GetSeparator(variableIndex).TrailingTrivia));

                    updatedField = updatedField.WithTrailingTrivia(BuildTrailingTrivia(trailingComments, lineBreakTrivia));
                }

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

    /// <inheritdoc/>
    public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (InterfaceDeclarationSyntax)base.VisitInterfaceDeclaration(node);

        if (node == null)
        {
            return null;
        }

        return node.WithMembers(SplitFields(node.Members));
    }

    #endregion // CSharpSyntaxVisitor
}