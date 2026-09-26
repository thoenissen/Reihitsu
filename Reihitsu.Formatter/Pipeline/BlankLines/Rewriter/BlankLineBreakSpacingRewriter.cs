using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.BlankLines.Utilities;

namespace Reihitsu.Formatter.Pipeline.BlankLines.Rewriter;

/// <summary>
/// Subphase that inserts blank lines after break statements. A formatting root that is itself a statement or switch section
/// following a <see langword="break"/> is decided against its preceding sibling in the document, which
/// <see cref="FormattingContext.RootListPosition"/> carries
/// </summary>
internal sealed class BlankLineBreakSpacingRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Formatting context of the current blank-line subphase
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// Shared blank-line query and edit collaborator
    /// </summary>
    private readonly BlankLineEditor _editor;

    /// <summary>
    /// Cancellation token of the current blank-line subphase
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// Whether the formatting root has been entered, so that every later <see cref="Visit"/> call visits a descendant
    /// </summary>
    private bool _isRootEntered;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="editor">Shared blank-line query and edit collaborator</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public BlankLineBreakSpacingRewriter(FormattingContext context, BlankLineEditor editor, CancellationToken cancellationToken)
    {
        _context = context;
        _editor = editor;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the specified switch section ends in a <see langword="break"/> statement, which requires a blank
    /// line before the section that follows it
    /// </summary>
    /// <param name="section">The switch section</param>
    /// <returns><see langword="true"/> if the section ends in a <see langword="break"/> statement</returns>
    private static bool EndsInBreak(SwitchSectionSyntax section)
    {
        return section.Statements.LastOrDefault() is BreakStatementSyntax;
    }

    /// <summary>
    /// Ensures a blank line exists before the first token of the specified switch section
    /// </summary>
    /// <param name="section">The switch section</param>
    /// <returns>The section with a blank line inserted before it, or the original if one already exists</returns>
    private SwitchSectionSyntax EnsureBlankLineBeforeSection(SwitchSectionSyntax section)
    {
        var firstToken = section.GetFirstToken();

        if (_editor.HasBlankLineBeforeToken(firstToken))
        {
            return section;
        }

        var endOfLine = SyntaxFactory.EndOfLine(_context.EndOfLine);
        var newLeadingTrivia = firstToken.LeadingTrivia.Insert(0, endOfLine);

        return section.ReplaceToken(firstToken, firstToken.WithLeadingTrivia(newLeadingTrivia));
    }

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
            var updatedStatement = _editor.EnsureBlankLineBeforeStatement(currentStatement);

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
    /// <remarks>
    /// The first call receives the formatting root. After its descendants are visited, a root statement or switch section is
    /// decided against its preceding sibling in the document, because the visitor of the list that contains it lies outside
    /// this run
    /// </remarks>
    public override SyntaxNode Visit(SyntaxNode node)
    {
        if (_isRootEntered)
        {
            return base.Visit(node);
        }

        _isRootEntered = true;

        var visited = base.Visit(node);
        var position = _context.RootListPosition;

        switch (visited)
        {
            case StatementSyntax statement when position.PreviousStatement is BreakStatementSyntax:
                {
                    return _editor.EnsureBlankLineBeforeStatement(statement);
                }

            case SwitchSectionSyntax section when position.PreviousSection != null
                                                  && EndsInBreak(position.PreviousSection):
                {
                    return EnsureBlankLineBeforeSection(section);
                }

            default:
                {
                    return visited;
                }
        }
    }

    /// <inheritdoc />
    public override SyntaxNode VisitBlock(BlockSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

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
        _cancellationToken.ThrowIfCancellationRequested();

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
        _cancellationToken.ThrowIfCancellationRequested();

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
            if (EndsInBreak(newSections[sectionIndex - 1]) == false)
            {
                continue;
            }

            var section = newSections[sectionIndex];
            var updatedSection = EnsureBlankLineBeforeSection(section);

            if (updatedSection == section)
            {
                continue;
            }

            newSections[sectionIndex] = updatedSection;
            modified = true;
        }

        return modified
                   ? node.WithSections(SyntaxFactory.List(newSections))
                   : node;
    }

    #endregion // CSharpSyntaxVisitor
}