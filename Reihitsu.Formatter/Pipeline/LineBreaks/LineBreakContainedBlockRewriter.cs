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
    /// Gets the block contained by a body-bearing statement, clause, accessor, or lambda
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <param name="block">The contained block when present; otherwise, <see langword="null"/></param>
    /// <returns><see langword="true"/> if the node owns a contained block; otherwise, <see langword="false"/></returns>
    private static bool TryGetContainedBlock(SyntaxNode node,
                                             out BlockSyntax block)
    {
        block = node switch
                {
                    ForStatementSyntax forStatement => forStatement.Statement as BlockSyntax,
                    ForEachStatementSyntax forEachStatement => forEachStatement.Statement as BlockSyntax,
                    ForEachVariableStatementSyntax forEachVariableStatement => forEachVariableStatement.Statement as BlockSyntax,
                    WhileStatementSyntax whileStatement => whileStatement.Statement as BlockSyntax,
                    DoStatementSyntax doStatement => doStatement.Statement as BlockSyntax,
                    UsingStatementSyntax usingStatement => usingStatement.Statement as BlockSyntax,
                    LockStatementSyntax lockStatement => lockStatement.Statement as BlockSyntax,
                    FixedStatementSyntax fixedStatement => fixedStatement.Statement as BlockSyntax,
                    CheckedStatementSyntax checkedStatement => checkedStatement.Block,
                    TryStatementSyntax tryStatement => tryStatement.Block,
                    CatchClauseSyntax catchClause => catchClause.Block,
                    FinallyClauseSyntax finallyClause => finallyClause.Block,
                    AccessorDeclarationSyntax accessor => accessor.Body,
                    SimpleLambdaExpressionSyntax simpleLambda => simpleLambda.Body as BlockSyntax,
                    ParenthesizedLambdaExpressionSyntax parenthesizedLambda => parenthesizedLambda.Body as BlockSyntax,
                    AnonymousMethodExpressionSyntax anonymousMethod => anonymousMethod.Block,
                    _ => null,
                };

        return block != null;
    }

    /// <summary>
    /// Normalizes an <c>if</c> statement's then/else block braces and moves a trailing
    /// inline condition comment onto its own line
    /// </summary>
    /// <param name="node">The if statement</param>
    /// <returns>The updated if statement</returns>
    private IfStatementSyntax NormalizeIfStatement(IfStatementSyntax node)
    {
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

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode Visit(SyntaxNode node)
    {
        if (node == null)
        {
            return null;
        }

        _cancellationToken.ThrowIfCancellationRequested();

        var visited = base.Visit(node);

        if (visited == null)
        {
            return null;
        }

        if (visited is IfStatementSyntax ifStatement)
        {
            return NormalizeIfStatement(ifStatement);
        }

        if (TryGetContainedBlock(visited, out var block))
        {
            return _bracePlacer.NormalizeContainedBlock(visited, block);
        }

        return visited;
    }

    #endregion // CSharpSyntaxVisitor
}