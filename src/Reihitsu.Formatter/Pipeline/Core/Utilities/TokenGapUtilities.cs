using Microsoft.CodeAnalysis;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

/// <summary>
/// Shared helpers for analyzing token gaps and blank-line counts across formatter phases
/// </summary>
internal static class TokenGapUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether a line break exists between two neighboring tokens. This is the one statement of whether a gap
    /// holds a line break: a line break inside a comment that spans lines counts, so a token behind such a comment's
    /// <c>*/</c> is not broken onto a new line by the line-break phase
    /// </summary>
    /// <param name="previousToken">The token that precedes the gap</param>
    /// <param name="token">The token that follows the gap</param>
    /// <returns><see langword="true"/> if the token gap contains a line break, also one inside a comment</returns>
    public static bool HasLineBreakBetween(SyntaxToken previousToken, SyntaxToken token)
    {
        return TokenGapAnalysis.Between(previousToken, token).HasLineBreak;
    }

    /// <summary>
    /// Determines whether a trivia list that forms a whole gap between two tokens holds a line break, by the same statement
    /// as <see cref="HasLineBreakBetween"/>: a line break inside a comment that spans lines counts
    /// </summary>
    /// <param name="gapTrivia">The trivia of the gap</param>
    /// <returns><see langword="true"/> if the gap contains a line break, also one inside a comment</returns>
    public static bool HasLineBreak(SyntaxTriviaList gapTrivia)
    {
        return TokenGapAnalysis.OfTriviaRange(gapTrivia, 0, gapTrivia.Count).HasLineBreak;
    }

    /// <summary>
    /// Counts blank lines between two neighboring tokens while ignoring comment-content lines
    /// </summary>
    /// <param name="previousToken">The token that precedes the gap</param>
    /// <param name="token">The token that follows the gap</param>
    /// <returns>The number of blank lines between the tokens</returns>
    public static int CountBlankLinesBetween(SyntaxToken previousToken, SyntaxToken token)
    {
        return TokenGapAnalysis.Between(previousToken, token).BlankLineCount;
    }

    #endregion // Methods
}