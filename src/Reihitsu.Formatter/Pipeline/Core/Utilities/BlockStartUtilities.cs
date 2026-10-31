using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Data;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

/// <summary>
/// Shared fact about whether a token starts a block or switch section, which the blank-line phase and the documentation
/// phase read from this one owner
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
    /// The facts about the token that precedes the token being evaluated, resolved through
    /// <see cref="PrecedingTokenFacts.Resolve"/> so that the first token of a detached formatting root reads the facts
    /// about its preceding token in the document
    /// </param>
    /// <returns><see langword="true"/> if the token is the first in its containing block or switch section</returns>
    public static bool IsFirstInBlock(PrecedingTokenFacts previousToken)
    {
        return previousToken.Exists == false
               || previousToken.Kind == SyntaxKind.OpenBraceToken
               || previousToken.IsSwitchLabelColon;
    }

    #endregion // Methods
}