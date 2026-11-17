using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

/// <summary>
/// Splits field declarations that declare multiple variables
/// </summary>
internal sealed class FieldDeclarationSplitTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public FieldDeclarationSplitTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the transform splits the field declaration: it declares more than one variable, is a member of a
    /// class, struct, record, interface, or union, has no disabled text in its leading trivia, and has no documentation
    /// comment (FMT-STRUCT-021, FMT-STRUCT-022, FMT-STRUCT-030). A comment anywhere else in the declaration, and a directive or
    /// disabled text outside its leading trivia, never reach the split, because the precheck refuses every input that holds
    /// one (FMT-UNSUP-004, FMT-UNSUP-010)
    /// </summary>
    /// <param name="fieldDeclaration">The field declaration to inspect</param>
    /// <returns><see langword="true"/> if the transform splits the declaration; otherwise, <see langword="false"/></returns>
    internal static bool Splits(FieldDeclarationSyntax fieldDeclaration)
    {
        return fieldDeclaration.Declaration.Variables.Count > 1
               && fieldDeclaration.Parent is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax or InterfaceDeclarationSyntax or UnionDeclarationSyntax
               && HasDisabledTextInLeadingTrivia(fieldDeclaration) == false
               && IsDocumented(fieldDeclaration) == false;
    }

    /// <summary>
    /// Determines whether a field declaration has a documentation comment in its leading trivia. The split keeps that trivia
    /// on the first generated field only, so every other field would lose the documentation that applied to it. The parser
    /// produces documentation comment trivia only in the documentation modes <c>Parse</c> and <c>Diagnose</c>; in the mode
    /// <c>None</c>, a <c>///</c> or <c>/** */</c> comment is an ordinary comment and does not count (FMT-STRUCT-030)
    /// </summary>
    /// <param name="fieldDeclaration">The field declaration to inspect</param>
    /// <returns><see langword="true"/> if the declaration is documented; otherwise, <see langword="false"/></returns>
    private static bool IsDocumented(FieldDeclarationSyntax fieldDeclaration)
    {
        return fieldDeclaration.GetLeadingTrivia().Any(SyntaxTriviaUtilities.IsDocumentationCommentTrivia);
    }

    /// <summary>
    /// Determines whether the leading trivia of a field declaration's first token holds disabled text. The split keeps that
    /// trivia on the first generated field only, and disabled text there can be an attribute list or a modifier that another
    /// build compiles onto the whole declaration, so the declaration is kept whole whatever the disabled text holds. A
    /// directive there without disabled text stays in front of the first field and does not stop the split (FMT-STRUCT-022)
    /// </summary>
    /// <param name="fieldDeclaration">The field declaration to inspect</param>
    /// <returns><see langword="true"/> if the leading trivia holds disabled text; otherwise, <see langword="false"/></returns>
    private static bool HasDisabledTextInLeadingTrivia(FieldDeclarationSyntax fieldDeclaration)
    {
        return fieldDeclaration.GetLeadingTrivia().Any(static trivia => trivia.IsKind(SyntaxKind.DisabledTextTrivia));
    }

    /// <summary>
    /// Splits one eligible field declaration into single-declarator fields
    /// </summary>
    /// <param name="fieldDeclaration">Field declaration to split</param>
    /// <returns>The generated single-declarator fields</returns>
    private static IEnumerable<FieldDeclarationSyntax> SplitField(FieldDeclarationSyntax fieldDeclaration)
    {
        var variables = fieldDeclaration.Declaration.Variables;

        return variables.Select((_, variableIndex) => CreateSplitField(fieldDeclaration, variableIndex));
    }

    /// <summary>
    /// Creates the generated field for one declarator. The first field keeps the declaration's leading trivia, the last its
    /// trailing trivia; a later declarator's leading trivia and a separator's trailing trivia hold no comment
    /// (FMT-UNSUP-004) and no directive or disabled text (FMT-UNSUP-010) and are dropped, and no field gets a separator in
    /// front of it (FMT-STRUCT-022, FMT-STRUCT-026)
    /// </summary>
    /// <param name="fieldDeclaration">Original field declaration</param>
    /// <param name="variableIndex">Index of the declarator to emit</param>
    /// <returns>The generated single-declarator field</returns>
    private static FieldDeclarationSyntax CreateSplitField(FieldDeclarationSyntax fieldDeclaration, int variableIndex)
    {
        var variables = fieldDeclaration.Declaration.Variables;
        var variable = variables[variableIndex];

        // The first declarator keeps the whitespace in front of its name; a later one stands behind the repeated type
        // instead of behind a separator, so the whitespace that followed the separator is dropped
        var declarator = variableIndex == 0
                             ? variable
                             : variable.WithoutLeadingTrivia();
        var updatedField = fieldDeclaration.WithDeclaration(fieldDeclaration.Declaration.WithVariables(SyntaxFactory.SingletonSeparatedList(declarator)));

        if (variableIndex > 0)
        {
            updatedField = updatedField.WithLeadingTrivia();
        }

        // The separator for every non-final declarator and the declaration semicolon for the final declarator become
        // the generated field's terminator, which keeps the trivia in front of that token
        var isFinalVariable = variableIndex == variables.Count - 1;
        var terminator = isFinalVariable ? fieldDeclaration.SemicolonToken : variables.GetSeparator(variableIndex);

        if (isFinalVariable == false)
        {
            updatedField = updatedField.WithTrailingTrivia();
        }

        return updatedField.WithSemicolonToken(updatedField.SemicolonToken.WithLeadingTrivia(terminator.LeadingTrivia));
    }

    /// <summary>
    /// Splits field declarations in the provided member list
    /// </summary>
    /// <param name="members">The member list</param>
    /// <returns>The updated members</returns>
    private SyntaxList<MemberDeclarationSyntax> SplitFields(SyntaxList<MemberDeclarationSyntax> members)
    {
        if (members.Any(member => member is FieldDeclarationSyntax fieldDeclaration && Splits(fieldDeclaration)) == false)
        {
            return members;
        }

        var updatedMembers = new List<MemberDeclarationSyntax>(members.Count);

        foreach (var member in members)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (member is FieldDeclarationSyntax fieldDeclaration
                && Splits(fieldDeclaration))
            {
                updatedMembers.AddRange(SplitField(fieldDeclaration));
            }
            else
            {
                updatedMembers.Add(member);
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

    /// <inheritdoc/>
    public override SyntaxNode VisitUnionDeclaration(UnionDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (UnionDeclarationSyntax)base.VisitUnionDeclaration(node);

        if (node == null)
        {
            return null;
        }

        return node.WithMembers(SplitFields(node.Members));
    }

    #endregion // CSharpSyntaxVisitor
}