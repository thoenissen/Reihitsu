using Microsoft.CodeAnalysis;

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
        return TokenGapAnalysis.Between(previousToken, token).HasLineBreak;
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