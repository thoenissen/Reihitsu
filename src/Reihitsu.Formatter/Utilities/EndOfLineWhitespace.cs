using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Utilities;

/// <summary>
/// Owns the policy that removes the whitespace a token's trailing trivia ends with when the line ends right after it
/// </summary>
internal static class EndOfLineWhitespace
{
    #region Methods

    /// <summary>
    /// Removes the <see cref="SyntaxKind.WhitespaceTrivia"/> that ends a token's trailing trivia when the line break that
    /// ends the line lives in the following token's leading trivia rather than in the trailing trivia itself
    /// </summary>
    /// <param name="trailingTrivia">The trailing trivia of the token</param>
    /// <param name="followingLeadingTrivia">The leading trivia of the token that follows it</param>
    /// <returns>The trailing trivia without the whitespace that would end the line, or the unchanged list</returns>
    /// <remarks>
    /// Document-level cleanup applies this to every token inside the formatted root, and node-level formatting applies it to the
    /// token in front of the formatted target, which no pipeline phase reaches. The documentation-comment phase trims the same
    /// whitespace on its own when it moves a <c>///</c> comment, because it appends the line break to that trailing trivia itself
    /// </remarks>
    internal static SyntaxTriviaList StripBeforeFollowingLineBreak(SyntaxTriviaList trailingTrivia, SyntaxTriviaList followingLeadingTrivia)
    {
        if (trailingTrivia.Count == 0
            || trailingTrivia[trailingTrivia.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia) == false
            || trailingTrivia.Any(SyntaxKind.EndOfLineTrivia)
            || followingLeadingTrivia.Count == 0
            || followingLeadingTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia) == false)
        {
            return trailingTrivia;
        }

        return trailingTrivia.RemoveAt(trailingTrivia.Count - 1);
    }

    #endregion // Methods
}