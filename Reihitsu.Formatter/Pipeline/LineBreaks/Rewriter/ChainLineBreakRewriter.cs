using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

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
    /// Collapses an invoked member-access dot onto the same line as a preceding null-forgiving operator,
    /// so that <c>!\n.Member()</c> becomes <c>!.Member()</c>
    /// </summary>
    /// <param name="node">The invocation expression to process</param>
    /// <returns>The invocation with its member-access dot collapsed beside <c>!</c></returns>
    private static InvocationExpressionSyntax CollapseMemberAccessToNullForgivingOperator(InvocationExpressionSyntax node)
    {
        if (node.Expression is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Expression is not PostfixUnaryExpressionSyntax
            || LineBreakTriviaUtilities.HasLeadingEndOfLine(memberAccess.OperatorToken) == false)
        {
            return node;
        }

        return LineBreakTriviaUtilities.CollapseTokenToSameLine(node, memberAccess.OperatorToken);
    }

    /// <summary>
    /// Returns the pending replacement recorded for a token, or the token itself when none exists.
    /// The chain normalization steps write into one shared replacement map, and more than one step
    /// can legitimately touch the same token — rejoining a member name onto its dot clears that dot's
    /// trailing end-of-line, and a later collapse may still need to clear the same token's leading
    /// trivia. Composing on the pending token keeps the steps order-independent instead of letting
    /// the last writer silently discard an earlier edit
    /// </summary>
    /// <param name="token">The original token to look up</param>
    /// <param name="replacements">The token replacement map built so far</param>
    /// <returns>The pending replacement, or <paramref name="token"/> when it has not been replaced</returns>
    private static SyntaxToken GetPendingToken(SyntaxToken token,
                                               Dictionary<SyntaxToken, SyntaxToken> replacements)
    {
        return replacements.TryGetValue(token, out var pending)
                   ? pending
                   : token;
    }

    /// <summary>
    /// Chooses the dot the first-link collapse should consider: the chain's own first dot when the
    /// chain wraps at that dot, and otherwise the first invoked link, exactly as before.
    /// <para>
    /// <see cref="ChainWalker.CollectInvokedLinkDots"/> reports invoked links only, so a chain whose
    /// own first dot is a plain, non-invoked property access (<c>a</c> ⏎ <c>.Prop?.ToString()</c>)
    /// never offered that dot to the collapse at all, and the chain stayed split at a boundary no
    /// predicate ever tested (issue #683). Taking the first dot from the wider alignment set — the
    /// same set <c>MethodChainAlignmentContributor</c> aligns against — closes that gap.
    /// </para>
    /// <para>
    /// The substitution is deliberately confined to the chain's <em>own first</em> dot. A chain that
    /// already starts on its root line has made a wrapping choice further along, and pulling that
    /// later link back would undo it — so such a chain still answers with its first invoked link and
    /// keeps today's behavior, including the intermediate-member-access refusal that keeps a fluent
    /// chain wrapped.
    /// </para>
    /// <para>
    /// A postfix null-forgiving operator is not a chain dot: <c>a!</c> is the chain's root, not its
    /// first link, so the search skips it and considers the <c>.</c> that follows
    /// </para>
    /// </summary>
    /// <param name="node">The outermost chain node</param>
    /// <param name="firstInvokedDot">The chain's first invoked link dot, used as the fallback</param>
    /// <returns>The dot token the collapse should consider</returns>
    private static SyntaxToken FindFirstWrappedChainOperator(SyntaxNode node,
                                                             SyntaxToken firstInvokedDot)
    {
        if (node is not ExpressionSyntax expression)
        {
            return firstInvokedDot;
        }

        var spineDots = new List<SyntaxToken>();

        ChainWalker.CollectAlignmentDots(expression, spineDots);

        var firstChainDot = spineDots.Find(static dot => dot.Parent is not PostfixUnaryExpressionSyntax);

        if (firstChainDot.IsKind(SyntaxKind.None) == false
            && LineBreakTriviaUtilities.HasLeadingEndOfLine(firstChainDot))
        {
            return firstChainDot;
        }

        return firstInvokedDot;
    }

    /// <summary>
    /// Records the replacements that rejoin a member name onto its own member-access or
    /// member-binding dot when a line break separates the two (<c>x.</c> ⏎ <c>Name</c>).
    /// <para>
    /// That break lives in the dot's <em>trailing</em> trivia, which no chain predicate inspects:
    /// every other chain decision tests a token's leading trivia, so the split survives untouched and
    /// the orphaned name is later re-indented to block level. The only existing code that clears the
    /// slot is <see cref="CollapseChainToSingleLine"/>, which a chain reaches only while
    /// <see cref="IsCollapsibleChain"/> holds (issue #685).
    /// </para>
    /// <para>
    /// The dot is always immediately followed by its own name token, so the pair being joined is the
    /// pair the unjoinable-trivia guard inspects. A comment, preprocessor directive, or disabled text
    /// between the two keeps the split
    /// </para>
    /// </summary>
    /// <param name="node">The outermost chain node</param>
    /// <param name="replacements">The token replacement map to populate</param>
    private static void CollectMemberNameRejoinReplacements(SyntaxNode node,
                                                            Dictionary<SyntaxToken, SyntaxToken> replacements)
    {
        if (node is not ExpressionSyntax expression)
        {
            return;
        }

        var operatorTokens = new List<SyntaxToken>();
        var otherTokens = new List<SyntaxToken>();

        ChainWalker.CollectSpineTokens(expression, operatorTokens, otherTokens);

        foreach (var operatorToken in operatorTokens)
        {
            if (operatorToken.Parent is not MemberAccessExpressionSyntax
                && operatorToken.Parent is not MemberBindingExpressionSyntax)
            {
                continue;
            }

            var nameToken = operatorToken.GetNextToken();

            if (nameToken == default
                || nameToken.IsKind(SyntaxKind.None)
                || LineBreakTriviaUtilities.HasLeadingEndOfLine(nameToken) == false
                || LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(operatorToken, nameToken))
            {
                continue;
            }

            replacements[nameToken] = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(GetPendingToken(nameToken, replacements));

            if (LineBreakTriviaUtilities.HasTrailingEndOfLine(operatorToken))
            {
                var pendingOperator = GetPendingToken(operatorToken, replacements);

                replacements[operatorToken] = pendingOperator.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(pendingOperator.TrailingTrivia));
            }
        }
    }

    /// <summary>
    /// Rejoins split member names on a chain that no other normalization step visits, so the
    /// <c>x.</c> ⏎ <c>Name</c> split is closed on member-access chains as well as on the invoked
    /// chains <see cref="NormalizeChain"/> handles (issue #685)
    /// </summary>
    /// <param name="node">The outermost chain node</param>
    /// <returns>The node with split member names rejoined</returns>
    private static SyntaxNode RejoinSplitMemberNames(SyntaxNode node)
    {
        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        CollectMemberNameRejoinReplacements(node, replacements);

        if (replacements.Count == 0)
        {
            return node;
        }

        return node.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
    }

    /// <summary>
    /// Collapses the chain's leading dot onto the line before it when that dot starts a continuation
    /// line. The caller decides which dot that is — see <see cref="FindFirstWrappedChainOperator"/> —
    /// and this method only refuses the join: a dot whose own receiver is another member or
    /// conditional access belongs to a fluent chain that stays wrapped, and a comment, directive, or
    /// disabled text in the gap keeps the two tokens on separate lines
    /// </summary>
    /// <param name="firstDot">The chain dot to collapse</param>
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

        replacements[firstDot] = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(GetPendingToken(firstDot, replacements));

        if (previousToken != default
            && previousToken.IsKind(SyntaxKind.None) == false
            && LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken))
        {
            var pendingPrevious = GetPendingToken(previousToken, replacements);

            replacements[previousToken] = pendingPrevious.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(pendingPrevious.TrailingTrivia));
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
    /// Normalizes a method chain or conditional access chain.
    /// <para>
    /// Three decisions are made against three different token sets, and keeping them apart is what
    /// makes the chain converge. The collapse candidate comes from the wider alignment set bounded by
    /// the first invoked link, so a wrapped non-invoked property access is considered too (issue
    /// #683). The member-name rejoin walks the spine's trailing trivia, which no other step inspects
    /// (issue #685). Whether every continuation link must start its own line stays a question about
    /// the <em>invoked</em> links alone: a chain that only wrapped a non-invoked prefix dot collapses
    /// back onto one line and must not have breaks inserted into it.
    /// </para>
    /// <para>
    /// A comment directly above the collapse candidate is likewise a decision about one of the three,
    /// not about the chain as a whole: it refuses that one join, and leaves the rejoin and the
    /// continuation breaks to their own trivia guards (issues #685, #689). A comment further down the
    /// chain — for example above the first invoked link, while an earlier, uncommented non-invoked
    /// prefix dot is the actual collapse candidate — no longer suppresses the collapse (issue #699):
    /// the gap it occupies is never the one <see cref="TryCollapseFirstChainDot"/> would join across.
    /// </para>
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

        var firstWrappedDot = FindFirstWrappedChainOperator(node, chainDots[0]);
        var hasWrappedCandidate = firstWrappedDot.IsKind(SyntaxKind.None) == false;
        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        CollectMemberNameRejoinReplacements(node, replacements);

        // A comment directly above the collapse candidate refuses that one join; it must not
        // suppress the rejoin or the continuation-break pass, which do not touch the commented gap.
        // The predicate stays comment-only and deliberately does not use the wider
        // WouldJoinAcrossUnjoinableTrivia that the alignment phase applies (issue #489): widening it
        // would newly suppress the collapse for a chain carrying a directive above the candidate,
        // which today collapses the wrapped prefix dot and aligns the remaining links under it — a
        // strictly better layout than leaving it wrapped at block indentation.
        if (hasWrappedCandidate
            && ReihitsuFormatterHelpers.HasCommentDirectlyAbove(firstWrappedDot) == false)
        {
            TryCollapseFirstChainDot(firstWrappedDot, replacements);
        }

        if (chainDots.Exists(LineBreakTriviaUtilities.HasLeadingEndOfLine))
        {
            EnsureContinuationDotsStartOnNewLine(chainDots, replacements);
        }

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

            var pendingDot = GetPendingToken(chainDots[dotIndex], replacements);
            var newLeading = pendingDot.LeadingTrivia.Insert(0, endOfLine);

            replacements[chainDots[dotIndex]] = pendingDot.WithLeadingTrivia(newLeading);

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

        node = CollapseMemberAccessToNullForgivingOperator(node);

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

            return node;
        }

        return RejoinSplitMemberNames(node);
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

        if (isOutermost == false)
        {
            return node;
        }

        if (IsCollapsibleChain(node))
        {
            return CollapseChainToSingleLine(node);
        }

        return RejoinSplitMemberNames(node);
    }

    #endregion // CSharpSyntaxVisitor
}