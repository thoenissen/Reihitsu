using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Subphase that inserts blank lines after break statements
/// </summary>
internal sealed class BlankLineBreakSpacingRewriter : BlankLineSubphaseRewriter
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public BlankLineBreakSpacingRewriter(FormattingContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Applies break-spacing rules to a statement list
    /// </summary>
    /// <param name="statements">Statements to process</param>
    /// <returns>Updated statement list and a modified flag</returns>
    private (SyntaxList<StatementSyntax> Statements, bool Modified) ApplyBreakSpacing(SyntaxList<StatementSyntax> statements)
    {
        if (statements.Count <= 1)
        {
            return (statements, false);
        }

        var modified = false;
        var newStatements = new StatementSyntax[statements.Count];

        for (var statementIndex = 0; statementIndex < statements.Count; statementIndex++)
        {
            newStatements[statementIndex] = statements[statementIndex];
        }

        for (var statementIndex = 1; statementIndex < statements.Count; statementIndex++)
        {
            var previousStatement = newStatements[statementIndex - 1];

            if (previousStatement is not BreakStatementSyntax)
            {
                continue;
            }

            var currentStatement = newStatements[statementIndex];
            var updatedStatement = EnsureBlankLineBeforeStatement(currentStatement);

            if (updatedStatement == currentStatement)
            {
                continue;
            }

            newStatements[statementIndex] = updatedStatement;
            modified = true;
        }

        return (SyntaxFactory.List(newStatements), modified);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc />
    public override SyntaxNode VisitBlock(BlockSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (BlockSyntax)base.VisitBlock(node);

        if (node == null)
        {
            return null;
        }

        var result = ApplyBreakSpacing(node.Statements);

        return result.Modified
                   ? node.WithStatements(result.Statements)
                   : node;
    }

    /// <inheritdoc />
    public override SyntaxNode VisitSwitchSection(SwitchSectionSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (SwitchSectionSyntax)base.VisitSwitchSection(node);

        if (node == null)
        {
            return null;
        }

        var result = ApplyBreakSpacing(node.Statements);

        return result.Modified
                   ? node.WithStatements(result.Statements)
                   : node;
    }

    /// <inheritdoc />
    public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (SwitchStatementSyntax)base.VisitSwitchStatement(node);

        if (node == null)
        {
            return null;
        }

        var sections = node.Sections;

        if (sections.Count <= 1)
        {
            return node;
        }

        var modified = false;
        var newSections = new SwitchSectionSyntax[sections.Count];

        for (var sectionIndex = 0; sectionIndex < sections.Count; sectionIndex++)
        {
            newSections[sectionIndex] = sections[sectionIndex];
        }

        for (var sectionIndex = 1; sectionIndex < sections.Count; sectionIndex++)
        {
            var previousSection = newSections[sectionIndex - 1];
            var lastStatement = previousSection.Statements.LastOrDefault();

            if (lastStatement is not BreakStatementSyntax)
            {
                continue;
            }

            var section = newSections[sectionIndex];
            var firstToken = section.GetFirstToken();

            if (HasBlankLineBeforeToken(firstToken))
            {
                continue;
            }

            var endOfLine = SyntaxFactory.EndOfLine(Context.EndOfLine);
            var newLeadingTrivia = firstToken.LeadingTrivia.Insert(0, endOfLine);
            var newToken = firstToken.WithLeadingTrivia(newLeadingTrivia);

            newSections[sectionIndex] = section.ReplaceToken(firstToken, newToken);
            modified = true;
        }

        return modified
                   ? node.WithSections(SyntaxFactory.List(newSections))
                   : node;
    }

    #endregion // CSharpSyntaxVisitor
}