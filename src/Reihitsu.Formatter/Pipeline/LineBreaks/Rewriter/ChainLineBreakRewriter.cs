using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Data.Enumerations;
using Reihitsu.Formatter.Extensions;
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
/// The gaps in front of and inside an attached part are <see cref="AttachedPartLineStartRewriter"/>'s, which has joined
/// them before this rewriter runs.
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
    /// Determines whether the gap between two adjacent tokens holds a line break: an end-of-line trivia, or the line break
    /// that ends a <c>///</c> comment, a directive, or disabled text (<see cref="SyntaxTriviaUtilities.AreSeparatedByEndOfLine"/>)
    /// </summary>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="nextToken">The token behind the gap</param>
    /// <returns><see langword="true"/> if the gap holds a line break</returns>
    private static bool HasLineBreak(SyntaxToken previousToken, SyntaxToken nextToken)
    {
        return SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, nextToken);
    }

    /// <summary>
    /// Determines whether a token stands behind the closing line of a delimited comment that spans several lines, on the
    /// line that comment ends on. Such a comment belongs to the line it starts on, so the token does not start its line
    /// even though a line break precedes it in the gap
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token stands behind a multi-line comment's closing line; otherwise, <see langword="false"/></returns>
    private static bool IsBehindMultiLineCommentTail(SyntaxToken token)
    {
        for (var triviaIndex = token.LeadingTrivia.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            var trivia = token.LeadingTrivia[triviaIndex];

            if (SyntaxTriviaUtilities.EndsLine(trivia))
            {
                return false;
            }

            if ((trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                && LineEndingUtilities.ContainsLineTerminator(trivia.ToFullString()))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Decides, per link of a chain, whether the layout of a wrapped chain breaks the line in front of it: the first link
    /// joins the root line, later prefix links keep the user's layout, the first invoked link stays on the root line only
    /// while that line is intact, and every later link starts its own line. A chain the user did not wrap gets no line break
    /// </summary>
    /// <param name="chain">The chain</param>
    /// <returns>Per link, whether a line break is ensured in front of its operator</returns>
    /// <remarks>
    /// The decision reads only the line breaks the user put in front of and inside the link operators and the trivia in
    /// front of the first link. The blank-line phase runs after this phase, so it sees the line breaks this layout writes
    /// and reads no prediction of them (FMT-PIPE-025)
    /// </remarks>
    private static bool[] GetWrappedLayoutLineBreaks(FluentChain chain)
    {
        var lineBreaks = new bool[chain.Links.Count];

        if (chain.IsWrapped == false)
        {
            return lineBreaks;
        }

        var isRootLineIntact = true;

        for (var linkIndex = 0; linkIndex < chain.Links.Count; linkIndex++)
        {
            var link = chain.Links[linkIndex];
            var isWrappedByUser = link.StartsLine || link.HasInnerLineBreak(false);

            if (linkIndex == 0)
            {
                // A gap that holds a comment, a directive or disabled text is not closed, so the root line is broken.
                isRootLineIntact = HasLineBreak(SyntaxTokenNavigationUtilities.GetPreviousToken(link.OperatorToken), link.OperatorToken) == false
                                   || LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(SyntaxTokenNavigationUtilities.GetPreviousToken(link.OperatorToken), link.OperatorToken) == false;
            }
            else if (chain.IsPrefixLink(linkIndex))
            {
                lineBreaks[linkIndex] = isWrappedByUser;
            }
            else if (linkIndex == chain.FirstInvokedLinkIndex)
            {
                lineBreaks[linkIndex] = isWrappedByUser || isRootLineIntact == false;
            }
            else
            {
                lineBreaks[linkIndex] = true;
            }

            if (lineBreaks[linkIndex])
            {
                isRootLineIntact = false;
            }
        }

        return lineBreaks;
    }

    /// <summary>
    /// Selects the gap between two adjacent tokens for closing when it holds a line break: the gap is then joined by
    /// <see cref="LineBreakTriviaUtilities.JoinGaps"/>, which writes the minimal separator (FMT-BREAK-031). A gap that holds a comment, a preprocessor directive or disabled
    /// text is not selected and is left untouched
    /// </summary>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="nextToken">The token behind the gap</param>
    /// <param name="gaps">The gaps to join</param>
    private static void CloseGap(SyntaxToken previousToken,
                                 SyntaxToken nextToken,
                                 List<(SyntaxToken PreviousToken, SyntaxToken Token)> gaps)
    {
        if (HasLineBreak(previousToken, nextToken) == false
            || LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, nextToken))
        {
            return;
        }

        gaps.Add((previousToken, nextToken));
    }

    /// <summary>
    /// Selects the gaps between consecutive tokens of a link operator for closing, except gaps that hold blocking trivia
    /// </summary>
    /// <param name="tokens">The tokens in source order</param>
    /// <param name="gaps">The gaps to join</param>
    private static void CloseInnerGaps(IReadOnlyList<SyntaxToken> tokens,
                                       List<(SyntaxToken PreviousToken, SyntaxToken Token)> gaps)
    {
        for (var tokenIndex = 1; tokenIndex < tokens.Count; tokenIndex++)
        {
            CloseGap(tokens[tokenIndex - 1], tokens[tokenIndex], gaps);
        }
    }

    /// <summary>
    /// Ensures a line break in front of a token. An existing line break in the gap is kept as it is; otherwise one is
    /// appended to the previous token's trailing trivia, behind any block comment that sits there
    /// (<see cref="LineBreakTriviaUtilities.EndLineInFrontOf"/>). A token behind the closing line of a multi-line comment
    /// does not start its line although the gap holds a line break, so a line break is appended to its leading trivia,
    /// behind that comment. Each edit is computed from the original token and writes only the trailing trivia of the
    /// token in front of a link operator or the leading trivia of the operator, a side no gap the chain closes writes
    /// </summary>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="nextToken">The token that must start a line</param>
    /// <param name="edits">The line-break edits to populate, keyed by original token</param>
    private void EnsureLineBreak(SyntaxToken previousToken,
                                 SyntaxToken nextToken,
                                 Dictionary<SyntaxToken, SyntaxToken> edits)
    {
        if (HasLineBreak(previousToken, nextToken))
        {
            if (IsBehindMultiLineCommentTail(nextToken))
            {
                var leadingTrivia = LineBreakTriviaUtilities.RemoveTrailingWhitespace(nextToken.LeadingTrivia);

                edits[nextToken] = nextToken.WithLeadingTrivia(LineBreakTriviaUtilities.AppendEndOfLine(leadingTrivia, _context.EndOfLine));
            }

            return;
        }

        edits[previousToken] = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.EndLineInFrontOf(previousToken, nextToken, _context.EndOfLine));
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
            return node;
        }

        var gaps = new List<(SyntaxToken PreviousToken, SyntaxToken Token)>();
        var lineBreakEdits = new Dictionary<SyntaxToken, SyntaxToken>();

        // A member name split from its dot is rejoined. The gap is no wrap, so a line break that blocking trivia keeps
        // there does not affect the chain's layout. The gaps in front of and inside an attached part are
        // AttachedPartLineStartRewriter's, which has joined them before this rewriter runs.
        foreach (var link in chain.Links)
        {
            CloseGap(link.DotToken, link.Name.GetFirstToken(), gaps);
        }

        // Link operators are never split. A line break in front of or inside an operator is the user's wrap.
        foreach (var link in chain.Links)
        {
            CloseInnerGaps(link.OperatorTokens, gaps);
        }

        // Every layout decision reads the tree before the joins, and the joins and the line-break edits apply in one
        // rewrite, so no token has to be found again in a rewritten tree.
        if (chain.IsWrapped)
        {
            ApplyWrappedLayout(chain, gaps, lineBreakEdits);
        }

        return LineBreakTriviaUtilities.JoinGaps(node, gaps, lineBreakEdits);
    }

    /// <summary>
    /// Applies the layout of a wrapped chain: the first link joins the root line, and the line breaks in front of every
    /// later link are those <see cref="GetWrappedLayoutLineBreaks"/> decides
    /// </summary>
    /// <param name="chain">The chain</param>
    /// <param name="gaps">The gaps to join</param>
    /// <param name="lineBreakEdits">The line-break edits to populate, keyed by original token</param>
    private void ApplyWrappedLayout(FluentChain chain,
                                    List<(SyntaxToken PreviousToken, SyntaxToken Token)> gaps,
                                    Dictionary<SyntaxToken, SyntaxToken> lineBreakEdits)
    {
        var lineBreaks = GetWrappedLayoutLineBreaks(chain);

        for (var linkIndex = 0; linkIndex < chain.Links.Count; linkIndex++)
        {
            var operatorToken = chain.Links[linkIndex].OperatorToken;
            var previousToken = SyntaxTokenNavigationUtilities.GetPreviousToken(operatorToken);

            if (linkIndex == 0)
            {
                CloseGap(previousToken, operatorToken, gaps);
            }
            else if (lineBreaks[linkIndex])
            {
                EnsureLineBreak(previousToken, operatorToken, lineBreakEdits);
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
            && FluentChain.IsOutermostChainNode(original, _context.RootChainBoundary))
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