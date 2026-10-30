using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.Core.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

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
            if (TokenContainmentUtilities.ContainsToken(node, previousToken) == false)
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
    /// token's trailing trivia. The collapse is refused when a comment, preprocessor
    /// directive, or disabled text sits in the gap, because each must keep its own line,
    /// and the original node is returned unchanged. When the tokens directly in front of the token have
    /// no text, such as omitted type arguments, the gap joined is the one to the token with text in front
    /// of them, which holds the line break
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The node containing the token</param>
    /// <param name="token">The token to collapse to the previous line</param>
    /// <returns>The node with the token collapsed to the same line, or the original node when the collapse is refused</returns>
    public static TNode CollapseTokenToSameLine<TNode>(TNode node,
                                                       SyntaxToken token)
        where TNode : SyntaxNode
    {
        var hasPreviousToken = TokenLocator.TryGetPreviousToken(node, token, out var previousToken);

        if (hasPreviousToken
            && previousToken.Span.IsEmpty
            && SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token) == false)
        {
            // Tokens without text, such as omitted type arguments, carry no trivia, so the line break in front of the
            // token lies behind the token with text in front of them; the gap to join is the one to that token
            previousToken = token.GetPreviousToken();

            if (previousToken.IsKind(SyntaxKind.None)
                || TokenContainmentUtilities.ContainsToken(node, previousToken) == false
                || SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token) == false)
            {
                return node;
            }
        }

        if (hasPreviousToken
                ? WouldJoinAcrossUnjoinableTrivia(previousToken, token)
                : SyntaxTriviaUtilities.ContainsUnjoinableTrivia(token.LeadingTrivia))
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
    /// Determines whether a line break separates a token from the token with text in front of it, so that the token stands
    /// first on its line. The decision is the shared line-start fact (<see cref="SyntaxTokenPositionUtilities.IsFirstOnLine"/>):
    /// the line break may be an end-of-line trivia or the one that ends a <c>///</c> comment or a directive, and a token
    /// without text never starts a line. A token whose tree holds no token in front of it stands in a standalone subtree
    /// after rewriting; its leading line break or whitespace is then taken as the start of a new line
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if a line break separates the token from the token in front of it; otherwise, <see langword="false"/></returns>
    public static bool HasLeadingEndOfLine(SyntaxToken token)
    {
        if (token.GetPreviousToken().IsKind(SyntaxKind.None))
        {
            return token.Span.IsEmpty == false
                   && token.LeadingTrivia.Any(static trivia => SyntaxTriviaUtilities.EndsLine(trivia) || trivia.IsKind(SyntaxKind.WhitespaceTrivia));
        }

        return SyntaxTokenPositionUtilities.IsFirstOnLine(token);
    }

    /// <summary>
    /// Determines whether a token's trailing trivia ends its line (<see cref="SyntaxTriviaUtilities.EndsLine"/>)
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token's trailing trivia ends its line; otherwise, <see langword="false"/></returns>
    public static bool HasTrailingEndOfLine(SyntaxToken token)
    {
        return token.TrailingTrivia.Any(SyntaxTriviaUtilities.EndsLine);
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
        return TriviaListUtilities.RemoveTrailingWhitespace(triviaList);
    }

    /// <summary>
    /// Determines whether collapsing <paramref name="movedToken"/> onto the line that ends with
    /// <paramref name="anchorToken"/> would cross trivia that must keep its own line. Besides a comment
    /// (which would absorb the joined token), a preprocessor directive or disabled text also blocks the
    /// join: removing the end-of-line that terminates the directive's line would move the directive off the
    /// start of a line and invalidate it (CS1040) or discard the conditional-compilation boundary it marks,
    /// and the collapse would make no real progress toward joining the two tokens
    /// </summary>
    /// <param name="anchorToken">The token whose trailing end-of-line would be removed by the join</param>
    /// <param name="movedToken">The token that would be pulled onto the anchor token's line</param>
    /// <returns><see langword="true"/> if the join would cross a comment, directive, or disabled text; otherwise, <see langword="false"/></returns>
    public static bool WouldJoinAcrossUnjoinableTrivia(SyntaxToken anchorToken,
                                                       SyntaxToken movedToken)
    {
        return SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(anchorToken, movedToken);
    }

    #endregion // Methods
}