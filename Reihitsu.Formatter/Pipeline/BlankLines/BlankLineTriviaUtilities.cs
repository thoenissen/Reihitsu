using Microsoft.CodeAnalysis;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Shared helpers for blank-line trivia analysis
/// </summary>
internal static class BlankLineTriviaUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the specified trivia embeds its terminating line break
    /// </summary>
    /// <param name="trivia">The trivia to inspect</param>
    /// <returns><see langword="true"/> if the trivia text ends with a line break; otherwise, <see langword="false"/></returns>
    internal static bool EndsWithLineBreak(SyntaxTrivia trivia)
    {
        if (SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia) == false
            && SyntaxTriviaUtilities.IsDocumentationCommentTrivia(trivia) == false)
        {
            return false;
        }

        var text = trivia.ToFullString();

        return text.EndsWith("\n", StringComparison.Ordinal) || text.EndsWith("\r", StringComparison.Ordinal);
    }

    #endregion // Methods
}