using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Core.Enumerations;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Applies the line-break rules of member-access chains, based on the shared <see cref="FluentChain"/> model.
/// <para>
/// A chain consists of a root, links (<c>.</c>, <c>?.</c>, <c>!.</c> or <c>!?.</c> and a member name) and attached
/// parts (argument lists, element accesses, conditional element accesses and a null-forgiving operator that is not part
/// of a link operator). The rules, applied to every outermost chain node:
/// </para>
/// <list type="bullet">
/// <item><description>A member name split from its dot is rejoined; such a split is no wrap.</description></item>
/// <item>
/// <description>
/// In a chain with at least one link, an attached part joins the element in front of it; a chain without links only
/// joins its conditional element accesses.
/// </description>
/// </item>
/// <item><description>A link operator is never split: a line break inside it moves in front of its first token.</description></item>
/// <item>
/// <description>
/// The formatter never starts a wrap. A chain counts as wrapped when the user put a line break in
/// front of or inside any link operator.
/// </description>
/// </item>
/// <item>
/// <description>
/// In a wrapped chain the first link joins the root line; later prefix links keep the user's
/// layout; the first invoked link stays on the root line only while that line is intact; every later link starts its own
/// line.
/// </description>
/// </item>
/// </list>
/// <para>
/// A gap that holds a comment, a preprocessor directive or disabled text is never closed, so trivia is neither moved
/// nor absorbed. The indentation phase aligns whatever such a gap keeps on its own line
/// </para>
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
    /// Returns the pending replacement recorded for a token, or the token itself when none exists. Several rules may
    /// touch the same token — one clears its leading trivia, another its trailing trivia — so every rule composes on the
    /// pending token instead of letting the last writer discard an earlier edit
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
    /// Determines whether the gap between two adjacent tokens holds a line break
    /// </summary>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="nextToken">The token behind the gap</param>
    /// <returns><see langword="true"/> if the gap holds a line break</returns>
    private static bool HasLineBreak(SyntaxToken previousToken, SyntaxToken nextToken)
    {
        return LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken)
               || nextToken.LeadingTrivia.Any(SyntaxKind.EndOfLineTrivia);
    }

    /// <summary>
    /// Closes the gap between two adjacent tokens: their line breaks and the whitespace around them are removed. A gap
    /// that holds a comment, a preprocessor directive or disabled text is left untouched
    /// </summary>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="nextToken">The token behind the gap</param>
    /// <param name="replacements">The token replacement map to populate</param>
    /// <returns><see langword="true"/> if the gap holds no line break afterwards; <see langword="false"/> if blocking trivia keeps it</returns>
    private static bool TryCloseGap(SyntaxToken previousToken,
                                    SyntaxToken nextToken,
                                    Dictionary<SyntaxToken, SyntaxToken> replacements)
    {
        if (HasLineBreak(previousToken, nextToken) == false)
        {
            return true;
        }

        if (LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, nextToken))
        {
            return false;
        }

        var pendingPrevious = GetPendingToken(previousToken, replacements);

        replacements[previousToken] = pendingPrevious.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(pendingPrevious.TrailingTrivia)));
        replacements[nextToken] = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(GetPendingToken(nextToken, replacements));

        return true;
    }

    /// <summary>
    /// Closes the gaps between consecutive tokens of an operator or attached part, except gaps that hold blocking trivia
    /// </summary>
    /// <param name="tokens">The tokens in source order</param>
    /// <param name="replacements">The token replacement map to populate</param>
    private static void CloseInnerGaps(IReadOnlyList<SyntaxToken> tokens,
                                       Dictionary<SyntaxToken, SyntaxToken> replacements)
    {
        for (var tokenIndex = 1; tokenIndex < tokens.Count; tokenIndex++)
        {
            TryCloseGap(tokens[tokenIndex - 1], tokens[tokenIndex], replacements);
        }
    }

    /// <summary>
    /// Joins the conditional element accesses of a chain without links (<c>a</c> ⏎ <c>?[0]</c>). Other attached parts of
    /// such a chain keep the user's layout
    /// </summary>
    /// <param name="node">The outermost chain node</param>
    /// <returns>The node with its conditional element accesses joined</returns>
    private static SyntaxNode JoinConditionalElementAccesses(ExpressionSyntax node)
    {
        var chain = FluentChain.Create(node, false);

        if (chain == null)
        {
            return node;
        }

        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        foreach (var attachedPart in chain.AttachedParts)
        {
            if (attachedPart.Kind != FluentChainAttachedPartKind.ConditionalElementAccess)
            {
                continue;
            }

            TryCloseGap(attachedPart.FirstToken.GetPreviousToken(), attachedPart.FirstToken, replacements);
            CloseInnerGaps(attachedPart.Tokens, replacements);
        }

        return Replace(node, replacements);
    }

    /// <summary>
    /// Applies the recorded token replacements to a node
    /// </summary>
    /// <param name="node">The node</param>
    /// <param name="replacements">The token replacements</param>
    /// <returns>The node with the replacements applied</returns>
    private static SyntaxNode Replace(SyntaxNode node, Dictionary<SyntaxToken, SyntaxToken> replacements)
    {
        if (replacements.Count == 0)
        {
            return node;
        }

        return node.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
    }

    /// <summary>
    /// Ensures a line break in front of a token. An existing line break in the gap is kept as it is; otherwise one is
    /// appended to the previous token's trailing trivia, behind any block comment that sits there
    /// </summary>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="nextToken">The token that must start a line</param>
    /// <param name="replacements">The token replacement map to populate</param>
    private void EnsureLineBreak(SyntaxToken previousToken,
                                 SyntaxToken nextToken,
                                 Dictionary<SyntaxToken, SyntaxToken> replacements)
    {
        if (HasLineBreak(previousToken, nextToken))
        {
            return;
        }

        var pendingPrevious = GetPendingToken(previousToken, replacements);
        var trailingTrivia = LineBreakTriviaUtilities.RemoveTrailingWhitespace(pendingPrevious.TrailingTrivia);

        replacements[previousToken] = pendingPrevious.WithTrailingTrivia(LineBreakTriviaUtilities.AppendEndOfLine(trailingTrivia, _context.EndOfLine));
    }

    /// <summary>
    /// Normalizes the line breaks of an outermost chain node
    /// </summary>
    /// <param name="node">The outermost chain node</param>
    /// <returns>The node with normalized chain line breaks</returns>
    private SyntaxNode NormalizeChain(ExpressionSyntax node)
    {
        var chain = FluentChain.Create(node);

        if (chain == null)
        {
            return JoinConditionalElementAccesses(node);
        }

        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        // A member name split from its dot is rejoined, and an attached part joins the element in front of it. Neither
        // gap is a wrap, so a line break that blocking trivia keeps there does not affect the chain's layout.
        foreach (var link in chain.Links)
        {
            TryCloseGap(link.DotToken, link.Name.GetFirstToken(), replacements);
        }

        foreach (var attachedPart in chain.AttachedParts)
        {
            TryCloseGap(attachedPart.FirstToken.GetPreviousToken(), attachedPart.FirstToken, replacements);
            CloseInnerGaps(attachedPart.Tokens, replacements);
        }

        // Link operators are never split. A line break in front of or inside an operator is the user's wrap.
        var wrappedLinks = new bool[chain.Links.Count];

        for (var linkIndex = 0; linkIndex < chain.Links.Count; linkIndex++)
        {
            var link = chain.Links[linkIndex];

            wrappedLinks[linkIndex] = link.StartsLine
                                      || link.HasInnerLineBreak(false);

            CloseInnerGaps(link.OperatorTokens, replacements);
        }

        if (chain.IsWrapped)
        {
            ApplyWrappedLayout(chain, wrappedLinks, replacements);
        }

        return Replace(node, replacements);
    }

    /// <summary>
    /// Applies the layout of a wrapped chain: the first link joins the root line, later prefix links keep the user's
    /// layout, the first invoked link stays on the root line only while that line is intact, and every later link starts
    /// its own line
    /// </summary>
    /// <param name="chain">The chain</param>
    /// <param name="wrappedLinks">Per link, whether the user put a line break in front of or inside its operator</param>
    /// <param name="replacements">The token replacement map to populate</param>
    private void ApplyWrappedLayout(FluentChain chain,
                                    bool[] wrappedLinks,
                                    Dictionary<SyntaxToken, SyntaxToken> replacements)
    {
        var isRootLineIntact = true;

        for (var linkIndex = 0; linkIndex < chain.Links.Count; linkIndex++)
        {
            var operatorToken = chain.Links[linkIndex].OperatorToken;
            var previousToken = operatorToken.GetPreviousToken();

            if (linkIndex == 0)
            {
                if (TryCloseGap(previousToken, operatorToken, replacements) == false)
                {
                    isRootLineIntact = false;
                }
            }
            else if (chain.IsPrefixLink(linkIndex))
            {
                if (wrappedLinks[linkIndex])
                {
                    EnsureLineBreak(previousToken, operatorToken, replacements);

                    isRootLineIntact = false;
                }
            }
            else if (linkIndex == chain.FirstInvokedLinkIndex)
            {
                if (wrappedLinks[linkIndex]
                    || isRootLineIntact == false)
                {
                    EnsureLineBreak(previousToken, operatorToken, replacements);

                    isRootLineIntact = false;
                }
            }
            else
            {
                EnsureLineBreak(previousToken, operatorToken, replacements);
            }
        }
    }

    /// <summary>
    /// Normalizes a chain node after its children were visited, when the node is the outermost node of its chain
    /// </summary>
    /// <param name="original">The node before its children were visited</param>
    /// <param name="visited">The node after its children were visited</param>
    /// <returns>The normalized node</returns>
    private SyntaxNode NormalizeIfOutermost(SyntaxNode original, SyntaxNode visited)
    {
        if (visited is ExpressionSyntax expression
            && FluentChain.IsOutermostChainNode(original))
        {
            return NormalizeChain(expression);
        }

        return visited;
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        return NormalizeIfOutermost(node, base.VisitInvocationExpression(node));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        return NormalizeIfOutermost(node, base.VisitConditionalAccessExpression(node));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        return NormalizeIfOutermost(node, base.VisitMemberAccessExpression(node));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitElementAccessExpression(ElementAccessExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        return NormalizeIfOutermost(node, base.VisitElementAccessExpression(node));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        return NormalizeIfOutermost(node, base.VisitPostfixUnaryExpression(node));
    }

    #endregion // CSharpSyntaxVisitor
}