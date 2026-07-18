using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Subphase that inserts required blank lines before statements
/// </summary>
internal sealed class BlankLineStatementSpacingRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Shared blank-line query and edit collaborator
    /// </summary>
    private readonly BlankLineEditor _editor;

    /// <summary>
    /// Cancellation token of the current blank-line subphase
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="editor">Shared blank-line query and edit collaborator</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public BlankLineStatementSpacingRewriter(BlankLineEditor editor, CancellationToken cancellationToken)
    {
        _editor = editor;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the specified statement directly follows a closing brace, i.e. the shape RH5030
    /// requires a blank line for
    /// </summary>
    /// <param name="statement">The current statement</param>
    /// <param name="previous">The preceding statement</param>
    /// <param name="inSwitchSection">Whether the statements are inside a switch section</param>
    /// <returns><see langword="true"/> if the statement follows a closing brace and needs a blank line</returns>
    /// <remarks>
    /// This mirrors RH5030, which carries no directive exemption, unlike the statement-kind rules in
    /// <see cref="NeedsBlankLineForStatementKind"/>. Callers must reposition the insertion past a leading
    /// directive rather than skip it (issue #415)
    /// </remarks>
    private static bool IsAfterClosingBrace(StatementSyntax statement, StatementSyntax previous, bool inSwitchSection)
    {
        return previous.GetLastToken().IsKind(SyntaxKind.CloseBraceToken)
               && (statement is BreakStatementSyntax == false || inSwitchSection == false);
    }

    /// <summary>
    /// Determines whether a blank line is required before the specified statement based on its own kind
    /// </summary>
    /// <param name="statement">The current statement</param>
    /// <param name="previous">The preceding statement</param>
    /// <param name="inSwitchSection">Whether the statements are inside a switch section</param>
    /// <returns><see langword="true"/> if a blank line should be inserted before the statement</returns>
    private static bool NeedsBlankLineForStatementKind(StatementSyntax statement, StatementSyntax previous, bool inSwitchSection)
    {
        switch (statement)
        {
            case LocalDeclarationStatementSyntax:
                return previous is LocalDeclarationStatementSyntax == false;

            case TryStatementSyntax:
            case IfStatementSyntax:
            case WhileStatementSyntax:
            case DoStatementSyntax:
            case UsingStatementSyntax:
            case CommonForEachStatementSyntax:
            case ForStatementSyntax:
            case ReturnStatementSyntax:
            case GotoStatementSyntax:
            case ContinueStatementSyntax:
            case ThrowStatementSyntax:
            case SwitchStatementSyntax:
            case CheckedStatementSyntax:
            case FixedStatementSyntax:
            case LockStatementSyntax:
                return true;

            case BreakStatementSyntax:
                return inSwitchSection == false;

            case YieldStatementSyntax:
                return previous is YieldStatementSyntax == false;

            case ExpressionStatementSyntax expressionStatement:
                return previous is LocalDeclarationStatementSyntax
                       && expressionStatement.Expression is AssignmentExpressionSyntax == false;

            default:
                return false;
        }
    }

    /// <summary>
    /// Applies statement-spacing rules to a statement list
    /// </summary>
    /// <param name="statements">Statements to process</param>
    /// <param name="inSwitchSection">Whether this list belongs to a switch section</param>
    /// <returns>Updated statement list and a modified flag</returns>
    private (SyntaxList<StatementSyntax> Statements, bool Modified) ApplyStatementSpacing(SyntaxList<StatementSyntax> statements, bool inSwitchSection)
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
            var currentStatement = newStatements[statementIndex];
            var isAfterClosingBrace = IsAfterClosingBrace(currentStatement, previousStatement, inSwitchSection);

            if (isAfterClosingBrace == false
                && NeedsBlankLineForStatementKind(currentStatement, previousStatement, inSwitchSection) == false)
            {
                continue;
            }

            var updatedStatement = isAfterClosingBrace
                                       ? _editor.EnsureBlankLineAfterClosingBrace(currentStatement)
                                       : _editor.EnsureBlankLineBeforeStatement(currentStatement);

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
        _cancellationToken.ThrowIfCancellationRequested();

        var inSwitchSection = node.Parent is SwitchSectionSyntax;
        node = (BlockSyntax)base.VisitBlock(node);

        if (node == null)
        {
            return null;
        }

        var result = ApplyStatementSpacing(node.Statements, inSwitchSection);

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

        var result = ApplyStatementSpacing(node.Statements, inSwitchSection: true);

        return result.Modified
                   ? node.WithStatements(result.Statements)
                   : node;
    }

    #endregion // CSharpSyntaxVisitor
}