using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Trivia helpers used by line-break normalization without coupling them to the full rewriter
/// </summary>
internal static class LineBreakTriviaUtilities
{
    #region Methods

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

    #endregion // Methods
}