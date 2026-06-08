using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for blocks, accessor lists, and switch braces
/// </summary>
internal sealed class LineBreakBlockRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The token gap normalizer
    /// </summary>
    private readonly TokenGapNormalizer _gapNormalizer;

    /// <summary>
    /// The brace placer
    /// </summary>
    private readonly BracePlacer _bracePlacer;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="gapNormalizer">The token gap normalizer</param>
    /// <param name="bracePlacer">The brace placer</param>
    public LineBreakBlockRewriter(CancellationToken cancellationToken,
                                  TokenGapNormalizer gapNormalizer,
                                  BracePlacer bracePlacer)
    {
        _cancellationToken = cancellationToken;
        _gapNormalizer = gapNormalizer;
        _bracePlacer = bracePlacer;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Ensures sibling statements in a block start on separate lines
    /// </summary>
    /// <param name="node">The block node</param>
    /// <returns>The updated block</returns>
    private BlockSyntax EnsureStatementsStartOnSeparateLines(BlockSyntax node)
    {
        if (node.Statements.Count <= 1)
        {
            return node;
        }

        var modified = false;
        var statements = node.Statements.ToArray();

        for (var statementIndex = 1; statementIndex < statements.Length; statementIndex++)
        {
            if (statements[statementIndex - 1] is EmptyStatementSyntax
                || statements[statementIndex] is EmptyStatementSyntax)
            {
                continue;
            }

            var previousToken = statements[statementIndex - 1].GetLastToken();
            var currentToken = statements[statementIndex].GetFirstToken();

            if (TokenGapUtilities.HasLineBreakBetween(previousToken, currentToken) == false)
            {
                statements[statementIndex] = _gapNormalizer.NormalizeGapBeforeToken(statements[statementIndex], currentToken, blankLineCount: 0);
                modified = true;

                continue;
            }

            if (TokenGapUtilities.CountBlankLinesBetween(previousToken, currentToken) > 1)
            {
                statements[statementIndex] = _gapNormalizer.NormalizeGapBeforeToken(statements[statementIndex], currentToken, blankLineCount: 1);
                modified = true;
            }
        }

        return modified
                   ? node.WithStatements(SyntaxFactory.List(statements))
                   : node;
    }

    /// <summary>
    /// Ensures sibling statements in a switch section start on separate lines
    /// </summary>
    /// <param name="node">The switch section node</param>
    /// <returns>The updated switch section</returns>
    private SwitchSectionSyntax EnsureStatementsStartOnSeparateLines(SwitchSectionSyntax node)
    {
        if (node.Statements.Count <= 1)
        {
            return node;
        }

        var modified = false;
        var statements = node.Statements.ToArray();

        for (var statementIndex = 1; statementIndex < statements.Length; statementIndex++)
        {
            if (statements[statementIndex - 1] is EmptyStatementSyntax
                || statements[statementIndex] is EmptyStatementSyntax)
            {
                continue;
            }

            var previousToken = statements[statementIndex - 1].GetLastToken();
            var currentToken = statements[statementIndex].GetFirstToken();

            if (TokenGapUtilities.HasLineBreakBetween(previousToken, currentToken) == false)
            {
                statements[statementIndex] = _gapNormalizer.NormalizeGapBeforeToken(statements[statementIndex], currentToken, blankLineCount: 0);
                modified = true;

                continue;
            }

            if (TokenGapUtilities.CountBlankLinesBetween(previousToken, currentToken) > 1)
            {
                statements[statementIndex] = _gapNormalizer.NormalizeGapBeforeToken(statements[statementIndex], currentToken, blankLineCount: 1);
                modified = true;
            }
        }

        return modified
                   ? node.WithStatements(SyntaxFactory.List(statements))
                   : node;
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitBlock(BlockSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (BlockSyntax)base.VisitBlock(node);

        if (node == null)
        {
            return null;
        }

        node = _bracePlacer.EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = _bracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);
        node = EnsureStatementsStartOnSeparateLines(node);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAccessorList(AccessorListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AccessorListSyntax)base.VisitAccessorList(node);

        if (node == null)
        {
            return null;
        }

        if (LineBreakDetection.IsAutoPropertyAccessorList(node))
        {
            return node;
        }

        node = _bracePlacer.EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = _bracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchStatementSyntax)base.VisitSwitchStatement(node);

        if (node == null)
        {
            return null;
        }

        node = _bracePlacer.EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = _bracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchExpression(SwitchExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchExpressionSyntax)base.VisitSwitchExpression(node);

        if (node == null)
        {
            return null;
        }

        node = _bracePlacer.EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = _bracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchSection(SwitchSectionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchSectionSyntax)base.VisitSwitchSection(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureStatementsStartOnSeparateLines(node);

        if (node.Statements.Count > 0 && node.Statements[0] is BlockSyntax block)
        {
            node = _bracePlacer.NormalizeContainedBlock(node, block);
        }

        return node;
    }

    #endregion // CSharpSyntaxVisitor
}