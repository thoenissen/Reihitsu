using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for contained statement bodies and inline condition comments
/// </summary>
internal sealed class LineBreakContainedBlockRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

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
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="gapNormalizer">The token gap normalizer</param>
    /// <param name="bracePlacer">The brace placer</param>
    public LineBreakContainedBlockRewriter(FormattingContext context,
                                           CancellationToken cancellationToken,
                                           TokenGapNormalizer gapNormalizer,
                                           BracePlacer bracePlacer)
    {
        _context = context;
        _cancellationToken = cancellationToken;
        _gapNormalizer = gapNormalizer;
        _bracePlacer = bracePlacer;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Moves a trailing inline comment from an <c>if</c> condition onto its own line
    /// </summary>
    /// <param name="node">The if statement</param>
    /// <returns>The updated if statement</returns>
    private IfStatementSyntax MoveTrailingConditionCommentToOwnLine(IfStatementSyntax node)
    {
        var trailingTrivia = node.CloseParenToken.TrailingTrivia;
        var commentIndex = -1;

        for (var triviaIndex = 0; triviaIndex < trailingTrivia.Count; triviaIndex++)
        {
            if (trailingTrivia[triviaIndex].IsKind(SyntaxKind.SingleLineCommentTrivia)
                || trailingTrivia[triviaIndex].IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                commentIndex = triviaIndex;

                break;
            }
        }

        if (commentIndex < 0)
        {
            return node;
        }

        for (var triviaIndex = commentIndex + 1; triviaIndex < trailingTrivia.Count; triviaIndex++)
        {
            if (trailingTrivia[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia)
                || trailingTrivia[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                continue;
            }

            return node;
        }

        var commentTrivia = trailingTrivia[commentIndex];
        var removeStart = commentIndex;

        while (removeStart > 0 && trailingTrivia[removeStart - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            removeStart--;
        }

        var newTrailingTrivia = new List<SyntaxTrivia>(trailingTrivia.Count - (commentIndex - removeStart + 1));

        for (var triviaIndex = 0; triviaIndex < removeStart; triviaIndex++)
        {
            newTrailingTrivia.Add(trailingTrivia[triviaIndex]);
        }

        for (var triviaIndex = commentIndex + 1; triviaIndex < trailingTrivia.Count; triviaIndex++)
        {
            newTrailingTrivia.Add(trailingTrivia[triviaIndex]);
        }

        var newLeadingTrivia = node.IfKeyword.LeadingTrivia.Add(commentTrivia)
                                                           .Add(SyntaxFactory.EndOfLine(_context.EndOfLine));

        return node.WithCloseParenToken(node.CloseParenToken.WithTrailingTrivia(SyntaxFactory.TriviaList(newTrailingTrivia)))
                   .WithIfKeyword(node.IfKeyword.WithLeadingTrivia(newLeadingTrivia));
    }

    /// <summary>
    /// Normalizes a block-valued statement body when present
    /// </summary>
    /// <typeparam name="TNode">The parent syntax node type</typeparam>
    /// <param name="node">The parent node</param>
    /// <param name="statement">The statement body to normalize</param>
    /// <returns>The updated parent node</returns>
    private TNode NormalizeStatementBody<TNode>(TNode node,
                                                StatementSyntax statement)
        where TNode : SyntaxNode
    {
        if (statement is BlockSyntax block)
        {
            node = _bracePlacer.NormalizeContainedBlock(node, block);
        }

        return node;
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitForStatement(ForStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ForStatementSyntax)base.VisitForStatement(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeStatementBody(node, node.Statement);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ForEachStatementSyntax)base.VisitForEachStatement(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeStatementBody(node, node.Statement);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ForEachVariableStatementSyntax)base.VisitForEachVariableStatement(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeStatementBody(node, node.Statement);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (WhileStatementSyntax)base.VisitWhileStatement(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeStatementBody(node, node.Statement);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitDoStatement(DoStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (DoStatementSyntax)base.VisitDoStatement(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeStatementBody(node, node.Statement);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitUsingStatement(UsingStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (UsingStatementSyntax)base.VisitUsingStatement(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeStatementBody(node, node.Statement);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitLockStatement(LockStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (LockStatementSyntax)base.VisitLockStatement(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeStatementBody(node, node.Statement);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitFixedStatement(FixedStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (FixedStatementSyntax)base.VisitFixedStatement(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeStatementBody(node, node.Statement);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitCheckedStatement(CheckedStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (CheckedStatementSyntax)base.VisitCheckedStatement(node);

        if (node == null)
        {
            return null;
        }

        if (node.Block != null)
        {
            node = _bracePlacer.NormalizeContainedBlock(node, node.Block);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitTryStatement(TryStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (TryStatementSyntax)base.VisitTryStatement(node);

        if (node == null)
        {
            return null;
        }

        return _bracePlacer.NormalizeContainedBlock(node, node.Block);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitCatchClause(CatchClauseSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (CatchClauseSyntax)base.VisitCatchClause(node);

        if (node == null)
        {
            return null;
        }

        return _bracePlacer.NormalizeContainedBlock(node, node.Block);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitFinallyClause(FinallyClauseSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (FinallyClauseSyntax)base.VisitFinallyClause(node);

        if (node == null)
        {
            return null;
        }

        return _bracePlacer.NormalizeContainedBlock(node, node.Block);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AccessorDeclarationSyntax)base.VisitAccessorDeclaration(node);

        if (node == null)
        {
            return null;
        }

        if (node.Body != null)
        {
            node = _bracePlacer.NormalizeContainedBlock(node, node.Body);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SimpleLambdaExpressionSyntax)base.VisitSimpleLambdaExpression(node);

        if (node == null)
        {
            return null;
        }

        if (node.Body is BlockSyntax statementBlock)
        {
            node = _bracePlacer.NormalizeContainedBlock(node, statementBlock);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ParenthesizedLambdaExpressionSyntax)base.VisitParenthesizedLambdaExpression(node);

        if (node == null)
        {
            return null;
        }

        if (node.Body is BlockSyntax statementBlock)
        {
            node = _bracePlacer.NormalizeContainedBlock(node, statementBlock);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AnonymousMethodExpressionSyntax)base.VisitAnonymousMethodExpression(node);

        if (node == null)
        {
            return null;
        }

        if (node.Block != null)
        {
            node = _bracePlacer.NormalizeContainedBlock(node, node.Block);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (IfStatementSyntax)base.VisitIfStatement(node);

        if (node == null)
        {
            return null;
        }

        if (node.Statement is BlockSyntax statementBlock)
        {
            node = _gapNormalizer.NormalizeGapBeforeToken(node, statementBlock.OpenBraceToken, blankLineCount: 0);
            node = _bracePlacer.EnsureFirstContentOnNewLine(node, statementBlock.OpenBraceToken);
            node = _gapNormalizer.NormalizeGapBeforeToken(node, statementBlock.CloseBraceToken, blankLineCount: 0);
        }

        if (node.Else?.Statement is BlockSyntax elseBlock)
        {
            node = _gapNormalizer.NormalizeGapBeforeToken(node, elseBlock.OpenBraceToken, blankLineCount: 0);
            node = _bracePlacer.EnsureFirstContentOnNewLine(node, elseBlock.OpenBraceToken);
            node = _gapNormalizer.NormalizeGapBeforeToken(node, elseBlock.CloseBraceToken, blankLineCount: 0);
        }

        return MoveTrailingConditionCommentToOwnLine(node);
    }

    #endregion // CSharpSyntaxVisitor
}