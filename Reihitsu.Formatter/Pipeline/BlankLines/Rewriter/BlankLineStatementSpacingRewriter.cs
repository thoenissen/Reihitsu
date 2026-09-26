using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.BlankLines.Utilities;

namespace Reihitsu.Formatter.Pipeline.BlankLines.Rewriter;

/// <summary>
/// Subphase that inserts required blank lines before statements. Each statement of a block or switch section is decided
/// against its preceding sibling; a formatting root that is itself such a statement is decided against its preceding
/// sibling in the document, which <see cref="FormattingContext.RootListPosition"/> carries
/// </summary>
internal sealed class BlankLineStatementSpacingRewriter : CSharpSyntaxRewriter
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
    public BlankLineStatementSpacingRewriter(FormattingContext context, BlankLineEditor editor, CancellationToken cancellationToken)
    {
        _context = context;
        _editor = editor;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a statement needs the blank line that RH5030 requires after a closing brace, from its preceding
    /// statement and whether the statement itself is exempt as the terminal <see langword="break"/> of its switch section
    /// </summary>
    /// <param name="previous">The preceding statement</param>
    /// <param name="isTerminalDirectSwitchSectionBreak">Whether the current statement is the terminal <see langword="break"/> directly inside its switch section</param>
    /// <returns><see langword="true"/> if the statement follows a closing brace and needs a blank line</returns>
    /// <remarks>
    /// This mirrors RH5030, which carries no directive exemption, unlike the statement-kind rules in
    /// <see cref="NeedsBlankLineForStatementKind"/>. Callers must reposition the insertion past a leading
    /// directive rather than skip it.
    /// </remarks>
    private static bool IsAfterClosingBrace(StatementSyntax previous, bool isTerminalDirectSwitchSectionBreak)
    {
        return previous.GetLastToken().IsKind(SyntaxKind.CloseBraceToken)
               && isTerminalDirectSwitchSectionBreak == false;
    }

    /// <summary>
    /// Determines whether a blank line is required before the specified statement based on its own kind
    /// </summary>
    /// <param name="statement">The current statement</param>
    /// <param name="previous">The preceding statement</param>
    /// <param name="isTerminalDirectSwitchSectionBreak">Whether the current statement is the terminal <see langword="break"/> directly inside its switch section</param>
    /// <returns><see langword="true"/> if a blank line should be inserted before the statement</returns>
    private static bool NeedsBlankLineForStatementKind(StatementSyntax statement, StatementSyntax previous, bool isTerminalDirectSwitchSectionBreak)
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
                return isTerminalDirectSwitchSectionBreak == false;

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
    /// Ensures the blank line the statement-spacing rules require before a statement
    /// </summary>
    /// <param name="statement">The current statement</param>
    /// <param name="previous">The preceding statement</param>
    /// <param name="isTerminalDirectSwitchSectionBreak">Whether the current statement is the terminal <see langword="break"/> directly inside its switch section</param>
    /// <returns>The statement with a blank line inserted before it, or the original if none is required or one already exists</returns>
    private StatementSyntax ApplyStatementSpacing(StatementSyntax statement, StatementSyntax previous, bool isTerminalDirectSwitchSectionBreak)
    {
        if (IsAfterClosingBrace(previous, isTerminalDirectSwitchSectionBreak))
        {
            return _editor.EnsureBlankLineAfterClosingBrace(statement);
        }

        return NeedsBlankLineForStatementKind(statement, previous, isTerminalDirectSwitchSectionBreak)
                   ? _editor.EnsureBlankLineBeforeStatement(statement)
                   : statement;
    }

    /// <summary>
    /// Applies statement-spacing rules to a statement list
    /// </summary>
    /// <param name="statements">Statements to process</param>
    /// <returns>Updated statement list and a modified flag</returns>
    private (SyntaxList<StatementSyntax> Statements, bool Modified) ApplyStatementSpacing(SyntaxList<StatementSyntax> statements)
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
            var currentStatement = newStatements[statementIndex];
            var updatedStatement = ApplyStatementSpacing(currentStatement,
                                                         newStatements[statementIndex - 1],
                                                         BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak(currentStatement));

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
    /// The first call receives the formatting root. After its descendants are visited, a root statement is decided against
    /// its preceding sibling in the document, because the visitor of the list that contains it lies outside this run
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

        if (visited is StatementSyntax statement
            && position.PreviousStatement != null)
        {
            return ApplyStatementSpacing(statement, position.PreviousStatement, position.IsTerminalDirectSwitchSectionBreak);
        }

        return visited;
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

        var result = ApplyStatementSpacing(node.Statements);

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

        var result = ApplyStatementSpacing(node.Statements);

        return result.Modified
                   ? node.WithStatements(result.Statements)
                   : node;
    }

    #endregion // CSharpSyntaxVisitor
}