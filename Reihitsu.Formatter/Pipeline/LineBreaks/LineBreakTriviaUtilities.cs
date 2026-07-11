using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Trivia helpers used by line-break normalization without coupling them to the full rewriter
/// </summary>
internal static class LineBreakTriviaUtilities
{
    #region Methods

    /// <summary>
    /// Prepends an end-of-line trivia to a token's leading trivia
    /// </summary>
    /// <param name="token">The token to modify</param>
    /// <param name="endOfLine">The end-of-line sequence to prepend</param>
    /// <returns>The token with an end-of-line trivia prepended to its leading trivia</returns>
    public static SyntaxToken PrependEndOfLine(SyntaxToken token,
                                               string endOfLine)
    {
        var newLeading = token.LeadingTrivia.Insert(0, SyntaxFactory.EndOfLine(endOfLine));

        return token.WithLeadingTrivia(newLeading);
    }

    /// <summary>
    /// Appends an end-of-line trivia to a trivia list
    /// </summary>
    /// <param name="triviaList">The trivia list to extend</param>
    /// <param name="endOfLine">The end-of-line sequence to append</param>
    /// <returns>The trivia list with an end-of-line trivia appended</returns>
    public static SyntaxTriviaList AppendEndOfLine(SyntaxTriviaList triviaList,
                                                   string endOfLine)
    {
        return triviaList.Add(SyntaxFactory.EndOfLine(endOfLine));
    }

    /// <summary>
    /// Removes whitespace trivia from the end of a trivia list, leaving interior whitespace
    /// (such as the space that separates a kept comment from preceding trivia) untouched
    /// </summary>
    /// <param name="triviaList">The trivia list to clean</param>
    /// <returns>The trivia list without trailing whitespace trivia</returns>
    public static SyntaxTriviaList StripTrailingWhitespace(SyntaxTriviaList triviaList)
    {
        return RemoveTrailingWhitespace(triviaList);
    }

    /// <summary>
    /// Moves a token to a new line by prepending an end-of-line trivia to its leading trivia.
    /// Also strips any trailing whitespace from the previous token to avoid orphaned spaces
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The node containing the token</param>
    /// <param name="token">The token to move to a new line</param>
    /// <param name="endOfLine">The end-of-line sequence to insert</param>
    /// <returns>The node with the token moved to a new line</returns>
    public static TNode MoveTokenToNewLine<TNode>(TNode node,
                                                  SyntaxToken token,
                                                  string endOfLine)
        where TNode : SyntaxNode
    {
        var newToken = PrependEndOfLine(token, endOfLine);
        var previousToken = token.GetPreviousToken();

        if (previousToken != default
            && previousToken.IsKind(SyntaxKind.None) == false
            && HasTrailingEndOfLine(previousToken) == false
            && previousToken.TrailingTrivia.Any(SyntaxKind.WhitespaceTrivia))
        {
            if (TokenLocator.ContainsToken(node, previousToken) == false)
            {
                return node.ReplaceToken(token, newToken);
            }

            var newPreviousToken = previousToken.WithTrailingTrivia(RemoveTrailingWhitespace(previousToken.TrailingTrivia));

            return node.ReplaceTokens([previousToken, token],
                                      (original, _) =>
                                      {
                                          if (original == previousToken)
                                          {
                                              return newPreviousToken;
                                          }

                                          return newToken;
                                      });
        }

        return node.ReplaceToken(token, newToken);
    }

    /// <summary>
    /// Collapses a token to the same line as the previous token by removing any
    /// end-of-line trivia from both the token's leading trivia and the previous
    /// token's trailing trivia
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The node containing the token</param>
    /// <param name="token">The token to collapse to the previous line</param>
    /// <returns>The node with the token collapsed to the same line</returns>
    public static TNode CollapseTokenToSameLine<TNode>(TNode node,
                                                       SyntaxToken token)
        where TNode : SyntaxNode
    {
        var hasPreviousToken = TokenLocator.TryGetPreviousToken(node, token, out var previousToken);

        if (hasPreviousToken
                ? WouldJoinIntoComment(previousToken, token)
                : ContainsCommentOrDirective(token.LeadingTrivia))
        {
            return node;
        }

        var newToken = RemoveLeadingEndOfLineAndWhitespace(token);

        if (hasPreviousToken && HasTrailingEndOfLine(previousToken))
        {
            var newPreviousToken = previousToken.WithTrailingTrivia(RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia));

            return node.ReplaceTokens([previousToken, token],
                                      (original, _) =>
                                      {
                                          if (original == previousToken)
                                          {
                                              return newPreviousToken;
                                          }

                                          return newToken;
                                      });
        }

        return node.ReplaceToken(token, newToken);
    }

    /// <summary>
    /// Determines whether a token's leading trivia contains an end-of-line trivia,
    /// either directly or preceded only by whitespace
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token has a leading end-of-line trivia; otherwise, <see langword="false"/></returns>
    public static bool HasLeadingEndOfLine(SyntaxToken token)
    {
        if (token.LeadingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
        {
            return true;
        }

        var previousToken = token.GetPreviousToken();

        if (previousToken != default && previousToken.IsKind(SyntaxKind.None) == false)
        {
            return previousToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
        }

        // When previous token is unavailable (standalone subtree after rewriting),
        // treat leading whitespace as indentation on a new line.
        return token.LeadingTrivia.Any(SyntaxKind.WhitespaceTrivia);
    }

    /// <summary>
    /// Determines whether a token's trailing trivia contains an end-of-line trivia
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token has a trailing end-of-line trivia; otherwise, <see langword="false"/></returns>
    public static bool HasTrailingEndOfLine(SyntaxToken token)
    {
        return token.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
    }

    /// <summary>
    /// Determines whether two tokens sit on different lines
    /// </summary>
    /// <param name="openToken">The opening token</param>
    /// <param name="closeToken">The closing token</param>
    /// <returns><see langword="true"/> if the tokens span multiple lines; otherwise, <see langword="false"/></returns>
    public static bool SpansMultipleLines(SyntaxToken openToken,
                                          SyntaxToken closeToken)
    {
        return openToken.GetLocation().GetLineSpan().StartLinePosition.Line
               != closeToken.GetLocation().GetLineSpan().StartLinePosition.Line;
    }

    /// <summary>
    /// Determines whether collapsing <paramref name="movedToken"/> onto the line that ends with
    /// <paramref name="anchorToken"/> would corrupt protected trivia in the join gap. A join must be
    /// refused when the gap between the two tokens — the anchor token's trailing trivia or the moved
    /// token's leading trivia — contains a comment, a preprocessor directive, or disabled text. Removing
    /// the end-of-line that terminates a comment would absorb the joined token into it, and removing the
    /// end-of-line that terminates a directive would re-emit the directive mid-line (CS1040) or discard
    /// the conditional-compilation boundary it marks
    /// </summary>
    /// <param name="anchorToken">The token whose trailing end-of-line would be removed by the join</param>
    /// <param name="movedToken">The token that would be pulled onto the anchor token's line</param>
    /// <returns><see langword="true"/> if the join would corrupt protected trivia; otherwise, <see langword="false"/></returns>
    public static bool WouldJoinIntoComment(SyntaxToken anchorToken,
                                            SyntaxToken movedToken)
    {
        return ContainsCommentOrDirective(anchorToken.TrailingTrivia)
               || ContainsCommentOrDirective(movedToken.LeadingTrivia);
    }

    /// <summary>
    /// Removes all leading end-of-line and whitespace trivia from a token.
    /// Preserves other trivia such as comments and preprocessor directives
    /// </summary>
    /// <param name="token">The token to modify</param>
    /// <returns>The token with leading end-of-line and whitespace trivia removed</returns>
    public static SyntaxToken RemoveLeadingEndOfLineAndWhitespace(SyntaxToken token)
    {
        var newLeading = new List<SyntaxTrivia>();
        var skipping = true;

        foreach (var trivia in token.LeadingTrivia)
        {
            if (skipping
                && (trivia.IsKind(SyntaxKind.EndOfLineTrivia) || trivia.IsKind(SyntaxKind.WhitespaceTrivia)))
            {
                continue;
            }

            skipping = false;
            newLeading.Add(trivia);
        }

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newLeading));
    }

    /// <summary>
    /// Removes trailing end-of-line trivia (and any whitespace immediately before it) from a trivia list
    /// </summary>
    /// <param name="triviaList">The trivia list to modify</param>
    /// <returns>The trivia list with trailing end-of-line trivia removed</returns>
    public static SyntaxTriviaList RemoveTrailingEndOfLineTrivia(SyntaxTriviaList triviaList)
    {
        var result = new List<SyntaxTrivia>();

        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                while (result.Count > 0 && result[result.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    result.RemoveAt(result.Count - 1);
                }

                continue;
            }

            result.Add(trivia);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Removes trailing whitespace trivia from a trivia list
    /// </summary>
    /// <param name="triviaList">The trivia list to modify</param>
    /// <returns>The trivia list with trailing whitespace removed</returns>
    public static SyntaxTriviaList RemoveTrailingWhitespace(SyntaxTriviaList triviaList)
    {
        var result = triviaList.ToList();

        while (result.Count > 0 && result[result.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            result.RemoveAt(result.Count - 1);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Determines whether a trivia list contains comment trivia (single-line, multi-line, or documentation),
    /// a preprocessor directive, or disabled text — trivia whose terminating end-of-line must not be removed
    /// by a line join
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns><see langword="true"/> if the list contains a comment, directive, or disabled text; otherwise, <see langword="false"/></returns>
    private static bool ContainsCommentOrDirective(SyntaxTriviaList triviaList)
    {
        return triviaList.Any(static trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                                               || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                                               || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                                               || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
                                               || SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia));
    }

    #endregion // Methods
}