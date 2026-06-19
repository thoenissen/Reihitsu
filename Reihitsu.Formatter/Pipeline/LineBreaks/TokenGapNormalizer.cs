using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

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

        var newLeadingTrivia = new List<SyntaxTrivia>(blankLineCount + preservedLeadingTrivia.Count + 1);

        for (var lineBreakIndex = 0; lineBreakIndex <= blankLineCount; lineBreakIndex++)
        {
            newLeadingTrivia.Add(SyntaxFactory.EndOfLine(_endOfLine));
        }

        newLeadingTrivia.AddRange(preservedLeadingTrivia);

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

        var newToken = NormalizeLeadingGap(token, blankLineCount);

        if (TokenLocator.ContainsToken(node, previousToken) == false)
        {
            return withToken(node, newToken);
        }

        previousToken = TokenLocator.GetCurrentToken(node, previousToken);

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

        var newToken = NormalizeLeadingGap(token, blankLineCount);

        if (TokenLocator.ContainsToken(node, previousToken) == false)
        {
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

        if (TokenLocator.ContainsToken(node, previousToken))
        {
            previousToken = TokenLocator.GetCurrentToken(node, previousToken);
        }

        var hasLineBreak = TokenGapUtilities.HasLineBreakBetween(previousToken, token);
        var currentBlankLineCount = TokenGapUtilities.CountBlankLinesBetween(previousToken, token);

        if (hasLineBreak && currentBlankLineCount == blankLineCount)
        {
            return node;
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));
        var newToken = NormalizeLeadingGap(token, blankLineCount);

        if (TokenLocator.ContainsToken(node, previousToken) == false)
        {
            return node.ReplaceToken(token, newToken);
        }

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

    #endregion // Methods
}