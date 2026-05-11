using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline;

/// <summary>
/// Shared helpers for analyzing token gaps and blank-line counts across formatter phases
/// </summary>
internal static class TokenGapUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether a line break exists between two neighboring tokens
    /// </summary>
    /// <param name="previousToken">The token that precedes the gap</param>
    /// <param name="token">The token that follows the gap</param>
    /// <returns><see langword="true"/> if the token gap contains an end-of-line trivia</returns>
    public static bool HasLineBreakBetween(SyntaxToken previousToken, SyntaxToken token)
    {
        return previousToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))
               || token.LeadingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
    }

    /// <summary>
    /// Counts blank lines between two neighboring tokens while ignoring comment-content lines
    /// </summary>
    /// <param name="previousToken">The token that precedes the gap</param>
    /// <param name="token">The token that follows the gap</param>
    /// <returns>The number of blank lines between the tokens</returns>
    public static int CountBlankLinesBetween(SyntaxToken previousToken, SyntaxToken token)
    {
        var sawLineBreak = false;
        var lineHasContent = false;
        var blankLineCount = 0;

        ProcessGapTrivia(previousToken.TrailingTrivia, ref sawLineBreak, ref lineHasContent, ref blankLineCount);
        ProcessGapTrivia(token.LeadingTrivia, ref sawLineBreak, ref lineHasContent, ref blankLineCount);

        return blankLineCount;
    }

    /// <summary>
    /// Processes trivia that appears in a token gap and updates the blank-line accounting state
    /// </summary>
    /// <param name="triviaList">The trivia sequence to inspect</param>
    /// <param name="sawLineBreak">Tracks whether a line break has already been encountered in the gap</param>
    /// <param name="lineHasContent">Tracks whether the current logical line contains non-whitespace trivia</param>
    /// <param name="blankLineCount">Accumulates the number of blank lines seen in the gap</param>
    public static void ProcessGapTrivia(SyntaxTriviaList triviaList, ref bool sawLineBreak, ref bool lineHasContent, ref int blankLineCount)
    {
        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                if (sawLineBreak && lineHasContent == false)
                {
                    blankLineCount++;
                }

                sawLineBreak = true;
                lineHasContent = false;

                continue;
            }

            lineHasContent = true;
        }
    }

    #endregion // Methods
}