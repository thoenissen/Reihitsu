using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

/// <summary>
/// Applies trailing-whitespace edits to a token. This is the mechanics half of the horizontal
/// spacing phase: it knows how to set, normalize, and collapse trailing whitespace trivia while
/// preserving non-whitespace trivia (such as inline comments). It carries no spacing policy —
/// the desired space counts are decided by <see cref="SpacingPolicy"/>
/// </summary>
internal sealed class TrailingWhitespaceWriter
{
    #region Fields

    /// <summary>
    /// A single space whitespace trivia
    /// </summary>
    private static readonly SyntaxTrivia _singleSpace = SyntaxFactory.Whitespace(" ");

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Sets the trailing whitespace of a token to the specified number of spaces.
    /// Preserves non-whitespace trivia (such as inline comments)
    /// </summary>
    /// <param name="token">The token whose trailing whitespace to normalize</param>
    /// <param name="desiredSpaces">The desired number of trailing spaces</param>
    /// <returns>The token with adjusted trailing whitespace</returns>
    public SyntaxToken SetTrailingWhitespace(SyntaxToken token, int desiredSpaces)
    {
        var trailing = token.TrailingTrivia;

        if (trailing.Count == 0)
        {
            if (desiredSpaces == 0)
            {
                return token;
            }

            return token.WithTrailingTrivia(SyntaxFactory.Whitespace(new string(' ', desiredSpaces)));
        }

        // Check if trailing trivia is only whitespace
        var allWhitespace = trailing.All(static trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia));

        if (allWhitespace)
        {
            if (desiredSpaces == 0)
            {
                return token.WithTrailingTrivia();
            }

            return token.WithTrailingTrivia(SyntaxFactory.Whitespace(new string(' ', desiredSpaces)));
        }

        // Complex case: non-whitespace trivia present (e.g., inline multi-line comment).
        // Normalize whitespace around the non-whitespace items and set the final spacing.
        return NormalizeTrailingTriviaWithNonWhitespace(token, trailing, desiredSpaces);
    }

    /// <summary>
    /// Collapses multiple consecutive whitespace characters in trailing trivia to a single space.
    /// Does not add or remove whitespace — only normalizes multi-space whitespace trivia items
    /// </summary>
    /// <param name="token">The token whose trailing trivia to normalize</param>
    /// <returns>The token with collapsed trailing whitespace</returns>
    public SyntaxToken CollapseMultipleTrailingSpaces(SyntaxToken token)
    {
        var trailing = token.TrailingTrivia;

        if (trailing.Count == 0)
        {
            return token;
        }

        if (NeedsTrailingSpaceNormalization(trailing) == false)
        {
            return token;
        }

        return token.WithTrailingTrivia(BuildCollapsedTrailingTrivia(trailing));
    }

    /// <summary>
    /// Normalizes trailing trivia that contains non-whitespace items (such as inline comments).
    /// Whitespace between non-whitespace items is collapsed to a single space. The trailing
    /// whitespace after the last non-whitespace item is set to the desired space count
    /// </summary>
    /// <param name="token">The token whose trailing trivia to normalize</param>
    /// <param name="trailing">The trailing trivia list</param>
    /// <param name="desiredSpaces">The desired number of spaces at the end of the trivia</param>
    /// <returns>The token with normalized trailing trivia</returns>
    private static SyntaxToken NormalizeTrailingTriviaWithNonWhitespace(SyntaxToken token, SyntaxTriviaList trailing, int desiredSpaces)
    {
        // Find the index of the last non-whitespace trivia
        var lastNonWhitespaceIndex = -1;

        for (var triviaIndex = trailing.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            if (trailing[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia) == false)
            {
                lastNonWhitespaceIndex = triviaIndex;

                break;
            }
        }

        var newTrivia = SyntaxFactory.TriviaList();
        var prevWasWhitespace = false;

        for (var triviaIndex = 0; triviaIndex < trailing.Count; triviaIndex++)
        {
            var trivia = trailing[triviaIndex];

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (triviaIndex > lastNonWhitespaceIndex)
                {
                    // Whitespace after the last non-whitespace item — will be replaced below
                    continue;
                }

                if (prevWasWhitespace == false)
                {
                    newTrivia = newTrivia.Add(_singleSpace);
                }

                prevWasWhitespace = true;
            }
            else
            {
                newTrivia = newTrivia.Add(trivia);
                prevWasWhitespace = false;
            }
        }

        // Append desired trailing whitespace
        if (desiredSpaces > 0)
        {
            newTrivia = newTrivia.Add(SyntaxFactory.Whitespace(new string(' ', desiredSpaces)));
        }

        return token.WithTrailingTrivia(newTrivia);
    }

    /// <summary>
    /// Determines whether trailing trivia requires whitespace normalization
    /// </summary>
    /// <param name="trailing">The trailing trivia list to inspect</param>
    /// <returns><see langword="true"/> if normalization is needed; otherwise, <see langword="false"/></returns>
    private static bool NeedsTrailingSpaceNormalization(SyntaxTriviaList trailing)
    {
        var prevWasWhitespace = false;

        foreach (var trivia in trailing)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (trivia.Span.Length > 1 || prevWasWhitespace)
                {
                    return true;
                }

                prevWasWhitespace = true;
            }
            else
            {
                prevWasWhitespace = false;
            }
        }

        return false;
    }

    /// <summary>
    /// Builds a trailing trivia list with consecutive whitespace collapsed to single spaces
    /// </summary>
    /// <param name="trailing">The original trailing trivia list</param>
    /// <returns>The normalized trailing trivia list</returns>
    private static SyntaxTriviaList BuildCollapsedTrailingTrivia(SyntaxTriviaList trailing)
    {
        var newTrivia = SyntaxFactory.TriviaList();
        var prevWasWhitespace = false;

        foreach (var trivia in trailing)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (prevWasWhitespace == false)
                {
                    newTrivia = newTrivia.Add(_singleSpace);
                }

                prevWasWhitespace = true;
            }
            else
            {
                newTrivia = newTrivia.Add(trivia);
                prevWasWhitespace = false;
            }
        }

        return newTrivia;
    }

    #endregion // Methods
}