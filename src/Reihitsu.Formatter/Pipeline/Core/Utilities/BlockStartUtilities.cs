using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Data;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

/// <summary>
/// Shared facts about whether a token starts a block or switch section, which the documentation phase reads, and whether
/// it is the first content behind an opening token or of a switch section, which the blank-line phase reads
/// </summary>
internal static class BlockStartUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the token that follows the specified preceding token is the first token in a block or switch
    /// section: the preceding token is an opening brace or the colon of a switch label. A token without a known preceding
    /// token counts as first
    /// </summary>
    /// <param name="previousToken">
    /// The facts about the token that precedes the token being evaluated; for the first token of a detached formatting root,
    /// which has no preceding token in its tree, they say that no token precedes it
    /// </param>
    /// <returns><see langword="true"/> if the token is the first in its containing block or switch section</returns>
    public static bool IsFirstInBlock(PrecedingTokenFacts previousToken)
    {
        return previousToken.Exists == false
               || previousToken.Kind == SyntaxKind.OpenBraceToken
               || previousToken.IsSwitchLabelColon;
    }

    /// <summary>
    /// Determines whether the token that follows the specified preceding token is the first content behind an opening token
    /// or of a switch section, so that no blank line is inserted in front of a comment in its leading trivia
    /// (FMT-BLANK-011): the preceding token opens content (<see cref="PrecedingTokenFacts.OpensContent"/>) or is the colon
    /// of a switch label. A token without a known preceding token counts as first
    /// </summary>
    /// <param name="previousToken">
    /// The facts about the token that precedes the token being evaluated; for the first token of a detached formatting root,
    /// which has no preceding token in its tree, they say that no token precedes it
    /// </param>
    /// <returns><see langword="true"/> if the token is the first content behind an opening token or of a switch section</returns>
    public static bool IsFirstBehindOpeningToken(PrecedingTokenFacts previousToken)
    {
        return previousToken.Exists == false
               || previousToken.OpensContent
               || previousToken.IsSwitchLabelColon;
    }

    #endregion // Methods
}