using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for method chains and conditional access chains
/// </summary>
internal sealed class ChainLineBreakRewriter : CSharpSyntaxRewriter
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

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public ChainLineBreakRewriter(FormattingContext context,
                                  CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Collapses the first <see cref="MemberBindingExpressionSyntax"/> in the
    /// <see cref="ConditionalAccessExpressionSyntax.WhenNotNull"/> subtree onto the
    /// same line as the <c>?</c> operator token, so that <c>?\n.Member()</c> becomes <c>?.Member()</c>
    /// </summary>
    /// <param name="node">The conditional access expression to process</param>
    /// <returns>The modified node with the member binding collapsed</returns>
    private static ConditionalAccessExpressionSyntax CollapseMemberBindingToQuestionToken(ConditionalAccessExpressionSyntax node)
    {
        var memberBinding = node.WhenNotNull
                                .DescendantNodesAndSelf()
                                .OfType<MemberBindingExpressionSyntax>()
                                .FirstOrDefault();

        if (memberBinding == null)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(memberBinding.OperatorToken) == false)
        {
            return node;
        }

        return LineBreakTriviaUtilities.CollapseTokenToSameLine(node, memberBinding.OperatorToken);
    }

    /// <summary>
    /// Normalizes a chain containing a single dot token
    /// </summary>
    /// <param name="node">The chain node</param>
    /// <param name="chainDot">The chain dot token</param>
    /// <returns>The updated chain node</returns>
    private static SyntaxNode NormalizeSingleChainDot(SyntaxNode node,
                                                      SyntaxToken chainDot)
    {
        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(chainDot)
            && ChainWalker.DotHasIntermediateMemberAccess(chainDot) == false
            && ReihitsuFormatterHelpers.HasCommentDirectlyAbove(chainDot) == false)
        {
            return LineBreakTriviaUtilities.CollapseTokenToSameLine(node, chainDot);
        }

        return node;
    }

    /// <summary>
    /// Collapses the first chain dot onto the root line when it starts on a continuation line
    /// </summary>
    /// <param name="firstDot">The first chain dot token</param>
    /// <param name="replacements">The token replacement map to populate</param>
    private static void TryCollapseFirstChainDot(SyntaxToken firstDot,
                                                 Dictionary<SyntaxToken, SyntaxToken> replacements)
    {
        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstDot) == false
            || ChainWalker.DotHasIntermediateMemberAccess(firstDot))
        {
            return;
        }

        var previousToken = firstDot.GetPreviousToken();

        if (previousToken != default
            && previousToken.IsKind(SyntaxKind.None) == false
            && LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, firstDot))
        {
            return;
        }

        replacements[firstDot] = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(firstDot);

        if (previousToken != default
            && previousToken.IsKind(SyntaxKind.None) == false
            && LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken))
        {
            replacements[previousToken] = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia));
        }
    }

    /// <summary>
    /// Determines whether a short access chain (at most one spine invocation that does not wrap its
    /// arguments and has no intermediate member access) should be rejoined onto a single line
    /// </summary>
    /// <param name="expression">The chain expression to inspect</param>
    /// <returns><see langword="true"/> if the chain is eligible to be collapsed; otherwise, <see langword="false"/></returns>
    private static bool IsCollapsibleChain(ExpressionSyntax expression)
    {
        return ChainWalker.CountSpineInvocations(expression) <= 1
               && ChainWalker.HasMultiLineArgumentList(expression) == false
               && ChainWalker.ChainHasIntermediateMemberAccess(expression) == false;
    }

    /// <summary>
    /// Determines whether any of the spine tokens carry a comment, preprocessor directive, or disabled
    /// text that would be lost or merged if the chain were rejoined onto a single line
    /// </summary>
    /// <param name="tokens">The spine tokens to inspect</param>
    /// <returns><see langword="true"/> if a token carries a comment, directive, or disabled text; otherwise, <see langword="false"/></returns>
    private static bool SpineHasUnjoinableTrivia(List<SyntaxToken> tokens)
    {
        return tokens.Exists(token => LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(token, token));
    }

    /// <summary>
    /// Rejoins a short access chain (at most one invocation on its spine) onto a single line by
    /// removing the end-of-line and indentation trivia at every chain-link boundary. Argument lists
    /// and chains that carry comments are left untouched
    /// </summary>
    /// <param name="node">The outermost chain node</param>
    /// <returns>The node with its spine collapsed onto a single line</returns>
    private static SyntaxNode CollapseChainToSingleLine(SyntaxNode node)
    {
        if (node is not ExpressionSyntax expression)
        {
            return node;
        }

        var operatorTokens = new List<SyntaxToken>();
        var otherTokens = new List<SyntaxToken>();

        ChainWalker.CollectSpineTokens(expression, operatorTokens, otherTokens);

        if (SpineHasUnjoinableTrivia(operatorTokens) || SpineHasUnjoinableTrivia(otherTokens))
        {
            return node;
        }

        // The root token keeps its leading trivia so the statement's indentation is preserved, and the
        // last token keeps its trailing trivia so a line break that belongs to the enclosing expression
        // (for example a wrapped binary operator after the chain) is not absorbed. Only the interior
        // chain-link boundaries are collapsed onto a single line.
        var rootToken = expression.GetFirstToken();
        var lastToken = expression.GetLastToken();
        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        foreach (var token in operatorTokens)
        {
            var current = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(token);

            if (token != lastToken)
            {
                current = current.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(current.TrailingTrivia)));
            }

            replacements[token] = current;
        }

        foreach (var token in otherTokens)
        {
            var current = token == rootToken
                              ? token
                              : LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(token);

            if (token != lastToken)
            {
                current = current.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(current.TrailingTrivia));
            }

            replacements[token] = current;
        }

        if (replacements.Count == 0)
        {
            return node;
        }

        return node.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
    }

    /// <summary>
    /// Normalizes a method chain or conditional access chain
    /// </summary>
    /// <param name="node">The outermost chain node (invocation or conditional access)</param>
    /// <returns>The node with normalized chain line breaks</returns>
    private SyntaxNode NormalizeChain(SyntaxNode node)
    {
        var chainDots = new List<SyntaxToken>();

        ChainWalker.CollectInvokedLinkDots(node, chainDots);

        if (chainDots.Count == 0)
        {
            return node;
        }

        if (chainDots.Count == 1)
        {
            return NormalizeSingleChainDot(node, chainDots[0]);
        }

        if (chainDots.Exists(LineBreakTriviaUtilities.HasLeadingEndOfLine) == false)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(chainDots[0])
            && ReihitsuFormatterHelpers.HasCommentDirectlyAbove(chainDots[0]))
        {
            return node;
        }

        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        TryCollapseFirstChainDot(chainDots[0], replacements);
        EnsureContinuationDotsStartOnNewLine(chainDots, replacements);

        if (replacements.Count == 0)
        {
            return node;
        }

        return node.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
    }

    /// <summary>
    /// Ensures continuation dots in a chain start on their own lines
    /// </summary>
    /// <param name="chainDots">The chain dot tokens</param>
    /// <param name="replacements">The token replacement map to populate</param>
    private void EnsureContinuationDotsStartOnNewLine(List<SyntaxToken> chainDots,
                                                      Dictionary<SyntaxToken, SyntaxToken> replacements)
    {
        var endOfLine = SyntaxFactory.EndOfLine(_context.EndOfLine);

        for (var dotIndex = 1; dotIndex < chainDots.Count; dotIndex++)
        {
            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(chainDots[dotIndex]))
            {
                continue;
            }

            var newLeading = chainDots[dotIndex].LeadingTrivia.Insert(0, endOfLine);
            replacements[chainDots[dotIndex]] = chainDots[dotIndex].WithLeadingTrivia(newLeading);

            var previousToken = chainDots[dotIndex].GetPreviousToken();

            if (previousToken != default
                && previousToken.IsKind(SyntaxKind.None) == false
                && replacements.ContainsKey(previousToken) == false
                && previousToken.TrailingTrivia.Any(SyntaxKind.WhitespaceTrivia))
            {
                replacements[previousToken] = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(previousToken.TrailingTrivia));
            }
        }
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var isOutermost = ChainWalker.IsOutermostChainInvocation(node);

        node = (InvocationExpressionSyntax)base.VisitInvocationExpression(node);

        if (node == null)
        {
            return null;
        }

        if (isOutermost)
        {
            if (IsCollapsibleChain(node))
            {
                return CollapseChainToSingleLine(node);
            }

            return NormalizeChain(node);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var isOutermost = node.Parent is not ConditionalAccessExpressionSyntax;

        node = (ConditionalAccessExpressionSyntax)base.VisitConditionalAccessExpression(node);

        if (node == null)
        {
            return null;
        }

        if (isOutermost == false)
        {
            return node;
        }

        if (IsCollapsibleChain(node))
        {
            return CollapseChainToSingleLine(node);
        }

        if (ChainWalker.ContainsInvocation(node.WhenNotNull))
        {
            node = (ConditionalAccessExpressionSyntax)NormalizeChain(node);
            node = CollapseMemberBindingToQuestionToken(node);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var isOutermost = ChainWalker.IsOutermostChainNode(node);

        node = (MemberAccessExpressionSyntax)base.VisitMemberAccessExpression(node);

        if (node == null)
        {
            return null;
        }

        if (isOutermost && IsCollapsibleChain(node))
        {
            return CollapseChainToSingleLine(node);
        }

        return node;
    }

    #endregion // CSharpSyntaxVisitor
}