using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.Core.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

/// <summary>
/// Normalizes the gap (line breaks and blank lines) before a token during line-break formatting
/// </summary>
internal sealed class TokenGapNormalizer
{
    #region Fields

    /// <summary>
    /// The end-of-line sequence to emit when inserting line breaks
    /// </summary>
    private readonly string _endOfLine;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="endOfLine">The end-of-line sequence to emit when inserting line breaks</param>
    public TokenGapNormalizer(string endOfLine)
    {
        _endOfLine = endOfLine;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Normalizes the leading gap in a token to the requested number of blank lines
    /// </summary>
    /// <param name="token">The token whose leading trivia should be normalized</param>
    /// <param name="blankLineCount">The number of blank lines to preserve before the token</param>
    /// <returns>The updated token</returns>
    public SyntaxToken NormalizeLeadingGap(SyntaxToken token,
                                           int blankLineCount)
    {
        return NormalizeLeadingGap(token, blankLineCount, previousProvidesLineBreak: false);
    }

    /// <summary>
    /// Normalizes the leading gap in a token to the requested number of blank lines
    /// </summary>
    /// <param name="token">The token whose leading trivia should be normalized</param>
    /// <param name="blankLineCount">The number of blank lines to preserve before the token</param>
    /// <param name="previousProvidesLineBreak">
    /// Whether the previous token's preserved trailing trivia already ends the line. When <see langword="true"/>,
    /// the leading gap emits one fewer line break because the previous token supplies the line-terminating break
    /// </param>
    /// <returns>The updated token</returns>
    public SyntaxToken NormalizeLeadingGap(SyntaxToken token,
                                           int blankLineCount,
                                           bool previousProvidesLineBreak)
    {
        if (HasOwnLineTrailingComment(token.LeadingTrivia) == false)
        {
            // Either there is no comment in the gap, or the comment shares the token's own line (for
            // example a block comment glued to a closing brace). Either way, the token's blank-line
            // policy governs the whole run up to that point, exactly as if no comment were present.
            return NormalizeLeadingGapWithoutContent(token, blankLineCount, previousProvidesLineBreak);
        }

        var lastContentIndex = SyntaxTriviaUtilities.FindLastSignificantTriviaIndex(token.LeadingTrivia);

        // The comment sits on its own line, separate from the token, so it owns the blank-line decision
        // above it. Preserve everything through that comment unchanged; only the run between the comment
        // and the token itself is a placement decision this method owns
        var preservedPrefix = new List<SyntaxTrivia>(lastContentIndex + 1);

        for (var triviaIndex = 0; triviaIndex <= lastContentIndex; triviaIndex++)
        {
            preservedPrefix.Add(token.LeadingTrivia[triviaIndex]);
        }

        var trailingRun = new List<SyntaxTrivia>(token.LeadingTrivia.Count - lastContentIndex - 1);

        for (var triviaIndex = lastContentIndex + 1; triviaIndex < token.LeadingTrivia.Count; triviaIndex++)
        {
            trailingRun.Add(token.LeadingTrivia[triviaIndex]);
        }

        var normalizedTrailingRun = NormalizeTrailingRun(trailingRun);

        var newLeadingTrivia = new List<SyntaxTrivia>(preservedPrefix.Count + normalizedTrailingRun.Count);

        newLeadingTrivia.AddRange(preservedPrefix);
        newLeadingTrivia.AddRange(normalizedTrailingRun);

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newLeadingTrivia));
    }

    /// <summary>
    /// Normalizes the gap before a token without changing the previous token's trailing trivia
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The containing node</param>
    /// <param name="token">The token whose preceding gap should be normalized</param>
    /// <param name="withToken">Function that updates the token on the owning node</param>
    /// <param name="blankLineCount">The number of blank lines to preserve before the token</param>
    /// <returns>The updated node</returns>
    public TNode NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia<TNode>(TNode node,
                                                                             SyntaxToken token,
                                                                             Func<TNode, SyntaxToken, TNode> withToken,
                                                                             int blankLineCount)
        where TNode : SyntaxNode
    {
        token = TokenLocator.GetCurrentToken(node, token);

        if (token.IsMissing || TokenLocator.ContainsToken(node, token) == false)
        {
            return node;
        }

        var hasPreviousToken = TokenLocator.TryGetPreviousToken(node, token, out var previousToken);
        var hasLineBreak = hasPreviousToken && TokenGapUtilities.HasLineBreakBetween(previousToken, token);
        var currentBlankLineCount = hasPreviousToken
                                        ? TokenGapUtilities.CountBlankLinesBetween(previousToken,
                                                                                   token)
                                        : 0;

        if (hasLineBreak && currentBlankLineCount == blankLineCount)
        {
            return node;
        }

        // A token with no predecessor (the first token of the formatted root) has no gap to
        // normalize. Forcing a leading line break here would prepend a spurious blank line.
        if (hasPreviousToken == false)
        {
            return node;
        }

        if (TokenLocator.ContainsToken(node, previousToken) == false)
        {
            // The previous token lies outside the node, so its trailing line break cannot be
            // removed here. When it already ends the line, emit one fewer line break to avoid doubling.
            var previousProvidesLineBreak = LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken);

            return withToken(node, NormalizeLeadingGap(token, blankLineCount, previousProvidesLineBreak));
        }

        previousToken = TokenLocator.GetCurrentToken(node, previousToken);

        var newToken = NormalizeLeadingGap(token, blankLineCount);

        if (HasOwnLineTrailingComment(token.LeadingTrivia))
        {
            // A comment on its own line sits in the gap; it owns the blank line above it, so that
            // token's own trailing trivia stays untouched.
            return withToken(node, newToken);
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia));

        return node.ReplaceTokens(new[] { previousToken, token },
                                  (originalToken, _) => originalToken == previousToken
                                                            ? newPreviousToken
                                                            : newToken);
    }

    /// <summary>
    /// Normalizes the gap before a token owned directly by a syntax node, even when the previous token lies outside that node
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The containing node</param>
    /// <param name="token">The token whose preceding gap should be normalized</param>
    /// <param name="withToken">Function that updates the token on the owning node</param>
    /// <param name="blankLineCount">The number of blank lines to preserve before the token</param>
    /// <returns>The updated node</returns>
    public TNode NormalizeGapBeforeOwnedToken<TNode>(TNode node,
                                                     SyntaxToken token,
                                                     Func<TNode, SyntaxToken, TNode> withToken,
                                                     int blankLineCount)
        where TNode : SyntaxNode
    {
        token = TokenLocator.GetCurrentToken(node, token);

        if (token.IsMissing || TokenLocator.ContainsToken(node, token) == false)
        {
            return node;
        }

        var hasPreviousToken = TokenLocator.TryGetPreviousToken(node, token, out var previousToken);

        if (hasPreviousToken && TokenLocator.ContainsToken(node, previousToken))
        {
            previousToken = TokenLocator.GetCurrentToken(node, previousToken);
        }

        var hasLineBreak = hasPreviousToken && TokenGapUtilities.HasLineBreakBetween(previousToken, token);
        var currentBlankLineCount = hasPreviousToken
                                        ? TokenGapUtilities.CountBlankLinesBetween(previousToken,
                                                                                   token)
                                        : 0;

        if (hasLineBreak && currentBlankLineCount == blankLineCount)
        {
            return node;
        }

        // A token with no predecessor (the first token of the formatted root) has no gap to
        // normalize. Forcing a leading line break here would prepend a spurious blank line.
        if (hasPreviousToken == false)
        {
            return node;
        }

        if (TokenLocator.ContainsToken(node, previousToken) == false)
        {
            // The previous token lies outside the node, so its trailing line break cannot be
            // removed here. When it already ends the line, emit one fewer line break to avoid doubling.
            var previousProvidesLineBreak = LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken);

            return withToken(node, NormalizeLeadingGap(token, blankLineCount, previousProvidesLineBreak));
        }

        var newToken = NormalizeLeadingGap(token, blankLineCount);

        if (HasOwnLineTrailingComment(token.LeadingTrivia))
        {
            // A comment on its own line sits in the gap; it owns the blank line above it, so that
            // token's own trailing trivia stays untouched.
            return withToken(node, newToken);
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));

        return node.ReplaceTokens(new[] { previousToken, token },
                                  (originalToken, _) => originalToken == previousToken
                                                            ? newPreviousToken
                                                            : newToken);
    }

    /// <summary>
    /// Normalizes the gap before a token to the requested number of blank lines
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The containing node</param>
    /// <param name="token">The token whose preceding gap should be normalized</param>
    /// <param name="blankLineCount">The number of blank lines to preserve before the token</param>
    /// <returns>The updated node</returns>
    public TNode NormalizeGapBeforeToken<TNode>(TNode node,
                                                SyntaxToken token,
                                                int blankLineCount)
        where TNode : SyntaxNode
    {
        token = TokenLocator.GetCurrentToken(node, token);

        if (TokenLocator.TryGetPreviousToken(node, token, out var previousToken) == false)
        {
            return node;
        }

        if (TokenLocator.ContainsToken(node, token) == false)
        {
            return node;
        }

        var previousInsideNode = TokenLocator.ContainsToken(node, previousToken);

        if (previousInsideNode)
        {
            previousToken = TokenLocator.GetCurrentToken(node, previousToken);
        }

        var hasLineBreak = TokenGapUtilities.HasLineBreakBetween(previousToken, token);
        var currentBlankLineCount = TokenGapUtilities.CountBlankLinesBetween(previousToken, token);

        if (hasLineBreak && currentBlankLineCount == blankLineCount)
        {
            return node;
        }

        if (previousInsideNode == false)
        {
            // The previous token lies outside the node, so its trailing line break cannot be removed
            // here. When it already ends the line, emit one fewer line break to avoid doubling the gap.
            var detachedToken = NormalizeLeadingGap(token, blankLineCount, LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken));

            return node.ReplaceToken(token, detachedToken);
        }

        var newToken = NormalizeLeadingGap(token, blankLineCount);

        if (HasOwnLineTrailingComment(token.LeadingTrivia))
        {
            // A comment on its own line sits in the gap; it owns the blank line above it, so that
            // token's own trailing trivia stays untouched.
            return node.ReplaceToken(token, newToken);
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));

        return node.ReplaceTokens(new[] { previousToken, token },
                                  (originalToken, _) =>
                                  {
                                      if (originalToken == previousToken)
                                      {
                                          return newPreviousToken;
                                      }

                                      return newToken;
                                  });
    }

    /// <summary>
    /// Determines whether a token's leading gap carries an ordinary <c>//</c> or <c>/* … */</c> comment that
    /// sits on its own line, separate from the token. A comment glued to the token — for example a block
    /// comment directly before a closing brace on the same physical line — does not count: it forms one
    /// visual line with the token, so the token's own blank-line policy governs the whole run instead of the
    /// comment owning a policy of its own. A documentation comment does not count either: it is structured
    /// trivia whose own blank-line placement is governed elsewhere. A gap that carries a preprocessor
    /// directive or disabled text anywhere — even alongside an ordinary comment — does not count either:
    /// that family's blank-line placement is a separate, pre-existing concern this fix does not own, and
    /// folding a directive into this decision would change behavior this fix has no mandate to touch
    /// </summary>
    /// <param name="leadingTrivia">The leading trivia to inspect</param>
    /// <returns><see langword="true"/> if an ordinary comment on its own line precedes the token; otherwise, <see langword="false"/></returns>
    private static bool HasOwnLineTrailingComment(SyntaxTriviaList leadingTrivia)
    {
        if (ContainsDirectiveOrDisabledText(leadingTrivia))
        {
            return false;
        }

        var lastContentIndex = SyntaxTriviaUtilities.FindLastSignificantTriviaIndex(leadingTrivia);

        if (lastContentIndex < 0 || IsOrdinaryComment(leadingTrivia[lastContentIndex]) == false)
        {
            return false;
        }

        for (var triviaIndex = lastContentIndex + 1; triviaIndex < leadingTrivia.Count; triviaIndex++)
        {
            if (leadingTrivia[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a trivia list contains a preprocessor directive or disabled text anywhere
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns><see langword="true"/> if a directive or disabled text is present; otherwise, <see langword="false"/></returns>
    private static bool ContainsDirectiveOrDisabledText(SyntaxTriviaList triviaList)
    {
        foreach (var trivia in triviaList)
        {
            if (SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a trivia is an ordinary, non-documentation <c>//</c> or <c>/* … */</c> comment
    /// </summary>
    /// <param name="trivia">The trivia to check</param>
    /// <returns><see langword="true"/> if the trivia is an ordinary comment; otherwise, <see langword="false"/></returns>
    private static bool IsOrdinaryComment(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia);
    }

    /// <summary>
    /// Normalizes a token's leading gap when it carries no comment or other content — the gap is pure line
    /// breaks and indentation, and the whole run is rewritten to the requested shape
    /// </summary>
    /// <param name="token">The token whose leading trivia should be normalized</param>
    /// <param name="blankLineCount">The number of blank lines to preserve before the token</param>
    /// <param name="previousProvidesLineBreak">
    /// Whether the previous token's preserved trailing trivia already ends the line. When <see langword="true"/>,
    /// the leading gap emits one fewer line break because the previous token supplies the line-terminating break
    /// </param>
    /// <returns>The updated token</returns>
    private SyntaxToken NormalizeLeadingGapWithoutContent(SyntaxToken token,
                                                          int blankLineCount,
                                                          bool previousProvidesLineBreak)
    {
        var suffixStart = 0;
        var lastLeadingEndOfLineIndex = -1;
        var sawNonWhitespaceTrivia = false;

        for (var triviaIndex = 0; triviaIndex < token.LeadingTrivia.Count; triviaIndex++)
        {
            var trivia = token.LeadingTrivia[triviaIndex];

            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                lastLeadingEndOfLineIndex = triviaIndex;

                continue;
            }

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            sawNonWhitespaceTrivia = true;

            break;
        }

        if (sawNonWhitespaceTrivia || lastLeadingEndOfLineIndex >= 0)
        {
            suffixStart = lastLeadingEndOfLineIndex + 1;
        }

        var preservedLeadingTrivia = new List<SyntaxTrivia>(token.LeadingTrivia.Count - suffixStart);

        for (var triviaIndex = suffixStart; triviaIndex < token.LeadingTrivia.Count; triviaIndex++)
        {
            preservedLeadingTrivia.Add(token.LeadingTrivia[triviaIndex]);
        }

        var lineBreakCount = previousProvidesLineBreak
                                 ? blankLineCount
                                 : blankLineCount + 1;

        var newLeadingTrivia = new List<SyntaxTrivia>(lineBreakCount + preservedLeadingTrivia.Count);

        for (var lineBreakIndex = 0; lineBreakIndex < lineBreakCount; lineBreakIndex++)
        {
            newLeadingTrivia.Add(SyntaxFactory.EndOfLine(_endOfLine));
        }

        newLeadingTrivia.AddRange(preservedLeadingTrivia);

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newLeadingTrivia));
    }

    /// <summary>
    /// Normalizes the run of line breaks and indentation between a comment and the token that follows it so the
    /// comment stays directly attached to that token — exactly one line break, never a blank line. The comment
    /// documents what follows it, so any blank-line budget the caller requested belongs above the comment,
    /// where <see cref="Pipeline.BlankLines.BlankLinePhase"/> already owns it; spending it here would insert a
    /// blank line the author never wrote and detach the comment from what it explains
    /// </summary>
    /// <param name="trailingRun">
    /// The trivia between the last content and the token. Always contains at least one end-of-line trivia,
    /// because the caller only reaches this method after confirming one is present
    /// </param>
    /// <returns>The normalized trailing run</returns>
    private List<SyntaxTrivia> NormalizeTrailingRun(List<SyntaxTrivia> trailingRun)
    {
        var lastEndOfLineIndex = -1;

        for (var triviaIndex = 0; triviaIndex < trailingRun.Count; triviaIndex++)
        {
            if (trailingRun[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                lastEndOfLineIndex = triviaIndex;
            }
        }

        var preservedTrailing = new List<SyntaxTrivia>(trailingRun.Count - lastEndOfLineIndex - 1);

        for (var triviaIndex = lastEndOfLineIndex + 1; triviaIndex < trailingRun.Count; triviaIndex++)
        {
            preservedTrailing.Add(trailingRun[triviaIndex]);
        }

        var normalizedRun = new List<SyntaxTrivia>(1 + preservedTrailing.Count)
                            {
                                SyntaxFactory.EndOfLine(_endOfLine)
                            };

        normalizedRun.AddRange(preservedTrailing);

        return normalizedRun;
    }

    #endregion // Methods
}