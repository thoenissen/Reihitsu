using Microsoft.CodeAnalysis;

namespace Reihitsu.Core;

/// <summary>
/// Finds the token in front of or behind a token with the result of <see cref="SyntaxToken.GetPreviousToken"/> and
/// <see cref="SyntaxToken.GetNextToken"/> in their default form: the nearest token with text, skipping zero-width tokens,
/// skipped tokens, directives, and documentation comments. Roslyn's navigation walks the children of the token's parent,
/// and of each ancestor it climbs to, one by one until it reaches the token, so asking it for every token of a parent with
/// many children — a long list, an enum, a class with many members, a block with many statements — costs time that grows
/// with the square of the child count. Here the token is found by its position instead: climbing to the nearest ancestor
/// that holds the neighbouring position and descending from there with <see cref="SyntaxNode.FindToken"/>, which picks
/// each child by position, so the cost no longer depends on how many siblings the token has
/// </summary>
public static class SyntaxTokenNavigationUtilities
{
    #region Methods

    /// <summary>
    /// Gets the token in front of a token, as <c>token.GetPreviousToken()</c> does
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>The nearest preceding token with text, or <see langword="default"/> when there is none</returns>
    public static SyntaxToken GetPreviousToken(SyntaxToken token)
    {
        var fullStart = token.FullSpan.Start;

        for (var node = token.Parent; node != null; node = node.Parent)
        {
            if (node.FullSpan.Start < fullStart)
            {
                var candidate = node.FindToken(fullStart - 1);

                // A token without text that carries trivia holds the position too; Roslyn's navigation skips it
                return candidate.Span.IsEmpty == false
                           ? candidate
                           : candidate.GetPreviousToken();
            }
        }

        // No ancestor reaches in front of the token: it is the first of its tree, or of a structured trivia, whose
        // navigation continues into the token that carries the trivia
        return token.GetPreviousToken();
    }

    /// <summary>
    /// Gets the token behind a token, as <c>token.GetNextToken()</c> does
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>The nearest following token with text, or <see langword="default"/> when there is none</returns>
    public static SyntaxToken GetNextToken(SyntaxToken token)
    {
        var fullEnd = token.FullSpan.End;

        for (var node = token.Parent; node != null; node = node.Parent)
        {
            if (node.FullSpan.End > fullEnd)
            {
                var candidate = node.FindToken(fullEnd);

                // A token without text that carries trivia, such as the end of the file behind a final comment, holds the
                // position too; Roslyn's navigation skips it
                return candidate.Span.IsEmpty == false
                           ? candidate
                           : candidate.GetNextToken();
            }
        }

        // No ancestor reaches behind the token: it is the last of its tree, or of a structured trivia, whose navigation
        // continues into the token that carries the trivia
        return token.GetNextToken();
    }

    #endregion // Methods
}