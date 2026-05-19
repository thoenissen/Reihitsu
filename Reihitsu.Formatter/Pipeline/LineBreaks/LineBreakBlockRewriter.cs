using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for blocks, accessor lists, and switch braces
/// </summary>
internal sealed class LineBreakBlockRewriter : LineBreakRewriter
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public LineBreakBlockRewriter(FormattingContext context,
                                  CancellationToken cancellationToken)
        : base(context,
               cancellationToken)
    {
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
                statements[statementIndex] = NormalizeGapBeforeToken(statements[statementIndex], currentToken, blankLineCount: 0);
                modified = true;

                continue;
            }

            if (TokenGapUtilities.CountBlankLinesBetween(previousToken, currentToken) > 1)
            {
                statements[statementIndex] = NormalizeGapBeforeToken(statements[statementIndex], currentToken, blankLineCount: 1);
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
                statements[statementIndex] = NormalizeGapBeforeToken(statements[statementIndex], currentToken, blankLineCount: 0);
                modified = true;

                continue;
            }

            if (TokenGapUtilities.CountBlankLinesBetween(previousToken, currentToken) > 1)
            {
                statements[statementIndex] = NormalizeGapBeforeToken(statements[statementIndex], currentToken, blankLineCount: 1);
                modified = true;
            }
        }

        return modified
                   ? node.WithStatements(SyntaxFactory.List(statements))
                   : node;
    }

    /// <summary>
    /// Normalizes brace placement for a block contained by a parent syntax node
    /// </summary>
    /// <typeparam name="TNode">The parent syntax node type</typeparam>
    /// <param name="node">The parent node that contains the block</param>
    /// <param name="block">The contained block</param>
    /// <returns>The updated parent node</returns>
    private TNode NormalizeContainedBlock<TNode>(TNode node,
                                                 BlockSyntax block)
        where TNode : SyntaxNode
    {
        node = NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia(node, block.OpenBraceToken, (n, t) => n.ReplaceToken(block.OpenBraceToken, t), blankLineCount: 0);
        node = EnsureFirstContentOnNewLine(node, block.OpenBraceToken);
        node = NormalizeGapBeforeToken(node, block.CloseBraceToken, blankLineCount: 0);

        return node;
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxNode VisitBlock(BlockSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (BlockSyntax)base.VisitBlock(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);
        node = EnsureStatementsStartOnSeparateLines(node);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAccessorList(AccessorListSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (AccessorListSyntax)base.VisitAccessorList(node);

        if (node == null)
        {
            return null;
        }

        if (IsAutoPropertyAccessorList(node))
        {
            return node;
        }

        node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (SwitchStatementSyntax)base.VisitSwitchStatement(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchExpression(SwitchExpressionSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (SwitchExpressionSyntax)base.VisitSwitchExpression(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchSection(SwitchSectionSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (SwitchSectionSyntax)base.VisitSwitchSection(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureStatementsStartOnSeparateLines(node);

        if (node.Statements.Count > 0 && node.Statements[0] is BlockSyntax block)
        {
            node = NormalizeContainedBlock(node, block);
        }

        return node;
    }

    #endregion // CSharpSyntaxRewriter
}