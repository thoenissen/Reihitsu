using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

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
    /// Determines whether the transform splits the field declaration: it declares more than one variable, is a member of a
    /// class, struct, record, or interface, and carries no preprocessor directive or disabled text (FMT-STRUCT-021,
    /// FMT-STRUCT-023)
    /// </summary>
    /// <param name="fieldDeclaration">The field declaration to inspect</param>
    /// <returns><see langword="true"/> if the transform splits the declaration; otherwise, <see langword="false"/></returns>
    internal static bool Splits(FieldDeclarationSyntax fieldDeclaration)
    {
        return fieldDeclaration.Declaration.Variables.Count > 1
               && fieldDeclaration.Parent is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax or InterfaceDeclarationSyntax
               && CarriesDirective(fieldDeclaration) == false;
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
                               .Any(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia);
    }

    /// <summary>
    /// Gets the comment trivia contained in the provided trivia list, including documentation comments
    /// </summary>
    /// <param name="trivia">The trivia list</param>
    /// <returns>The comment trivia</returns>
    private static List<SyntaxTrivia> GetComments(SyntaxTriviaList trivia)
    {
        return trivia.Where(SyntaxTriviaUtilities.IsCommentTrivia).ToList();
    }

    /// <summary>
    /// Builds the trailing trivia for a split field that a separating comma ended: one space in front of each comment that
    /// trailed the comma, and a line break where a separator needs one (FMT-STRUCT-026) — behind a single-line comment, and in
    /// front of a comment that leads the next field
    /// </summary>
    /// <param name="comments">The comments that trailed the comma</param>
    /// <param name="nextFieldHasLeadingComment">Whether a comment leads the next field</param>
    /// <returns>The trailing trivia</returns>
    private SyntaxTriviaList BuildTrailingTrivia(List<SyntaxTrivia> comments, bool nextFieldHasLeadingComment)
    {
        var trivia = new List<SyntaxTrivia>((comments.Count * 2) + 1);

        foreach (var comment in comments)
        {
            trivia.Add(SyntaxFactory.Space);
            trivia.Add(comment);
        }

        var endsWithSingleLineComment = comments.Count > 0
                                        && comments[comments.Count - 1].IsKind(SyntaxKind.SingleLineCommentTrivia);

        if (endsWithSingleLineComment || nextFieldHasLeadingComment)
        {
            trivia.Add(SyntaxFactory.EndOfLine(_context.EndOfLine));
        }

        return SyntaxFactory.TriviaList(trivia);
    }

    /// <summary>
    /// Builds the leading trivia for a later split field: each comment that preceded its declarator, on a line of its own.
    /// No indentation is written, because the indentation phase indents those lines (FMT-STRUCT-026)
    /// </summary>
    /// <param name="comments">The comments that preceded the declarator</param>
    /// <returns>The leading trivia</returns>
    private SyntaxTriviaList BuildLeadingTrivia(List<SyntaxTrivia> comments)
    {
        var trivia = new List<SyntaxTrivia>(comments.Count * 2);

        foreach (var comment in comments)
        {
            trivia.Add(comment);

            // A single-line documentation comment already terminates its line, so another end-of-line would put a blank
            // line between the comment and the field it documents. A delimited documentation comment (/** … */) carries
            // no break and still needs one, which is why the question is asked of the trivia text rather than of its kind
            if (SyntaxTriviaUtilities.EndsLine(comment) == false)
            {
                trivia.Add(SyntaxFactory.EndOfLine(_context.EndOfLine));
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
        if (members.Any(member => member is FieldDeclarationSyntax fieldDeclaration && Splits(fieldDeclaration)) == false)
        {
            return members;
        }

        var updatedMembers = new List<MemberDeclarationSyntax>(members.Count);

        foreach (var member in members)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            // Splitting rebuilds each generated field's trivia from comments only. A preprocessor directive or disabled
            // text entangled with the declarators or separators would be dropped, so Splits leaves directive-bearing
            // declarations intact.
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

    /// <summary>
    /// Splits one eligible field declaration into single-declarator fields
    /// </summary>
    /// <param name="fieldDeclaration">Field declaration to split</param>
    /// <returns>The generated single-declarator fields</returns>
    private IEnumerable<FieldDeclarationSyntax> SplitField(FieldDeclarationSyntax fieldDeclaration)
    {
        var variables = fieldDeclaration.Declaration.Variables;

        return variables.Select((_, variableIndex) => CreateSplitField(fieldDeclaration, variableIndex));
    }

    /// <summary>
    /// Creates the generated field for one declarator while preserving its trivia slots
    /// </summary>
    /// <param name="fieldDeclaration">Original field declaration</param>
    /// <param name="variableIndex">Index of the declarator to emit</param>
    /// <returns>The generated single-declarator field</returns>
    private FieldDeclarationSyntax CreateSplitField(FieldDeclarationSyntax fieldDeclaration, int variableIndex)
    {
        var variables = fieldDeclaration.Declaration.Variables;
        var variable = variables[variableIndex];

        // Only a later declarator's leading trivia is dropped, because BuildLeadingTrivia rebuilds it below. The
        // first declarator is carried over unchanged so comments between the type and declarator remain in that slot.
        // Trailing trivia stays on its declarator so comments are not moved into a leading position.
        var declarator = variableIndex == 0
                             ? variable
                             : variable.WithoutTrivia()
                                       .WithTrailingTrivia(variable.GetTrailingTrivia());
        var updatedField = fieldDeclaration.WithDeclaration(fieldDeclaration.Declaration.WithVariables(SyntaxFactory.SingletonSeparatedList(declarator)));

        updatedField = variableIndex == 0
                           ? updatedField.WithLeadingTrivia(fieldDeclaration.GetLeadingTrivia())
                           : updatedField.WithLeadingTrivia(BuildLeadingTrivia(GetComments(variable.GetLeadingTrivia())));

        // The separator for every non-final declarator and the declaration semicolon for the final declarator become
        // the generated field's terminator. Trivia before and after that token remains on the same side.
        var isFinalVariable = variableIndex == variables.Count - 1;
        var terminator = isFinalVariable ? fieldDeclaration.SemicolonToken : variables.GetSeparator(variableIndex);

        updatedField = isFinalVariable
                           ? updatedField.WithTrailingTrivia(fieldDeclaration.GetTrailingTrivia())
                           : updatedField.WithTrailingTrivia(BuildTrailingTrivia(GetComments(terminator.TrailingTrivia),
                                                                                 variables[variableIndex + 1].GetLeadingTrivia().Any(SyntaxTriviaUtilities.IsCommentTrivia)));

        return updatedField.WithSemicolonToken(updatedField.SemicolonToken.WithLeadingTrivia(terminator.LeadingTrivia));
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