using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

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

    /// <summary>
    /// Determines whether two tokens written without a space between them would lex as different tokens: two
    /// identifier or keyword characters meeting, two slashes or a slash and an asterisk starting a comment, or a prefix
    /// operator gluing into an increment, decrement, or logical operator with its operand
    /// </summary>
    /// <param name="previousToken">The first token</param>
    /// <param name="token">The second token</param>
    /// <returns><see langword="true"/> if the tokens must keep a space between them; otherwise, <see langword="false"/></returns>
    public static bool WouldMergeWhenTight(SyntaxToken previousToken, SyntaxToken token)
    {
        var last = previousToken.Text[previousToken.Text.Length - 1];
        var first = token.Text[0];

        if (SyntaxFacts.IsIdentifierPartCharacter(last)
            && (SyntaxFacts.IsIdentifierPartCharacter(first) || first == '@'))
        {
            return true;
        }

        if (last == '/' && (first is '/' or '*'))
        {
            return true;
        }

        return UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator(previousToken, token);
    }

    #endregion // Methods
}