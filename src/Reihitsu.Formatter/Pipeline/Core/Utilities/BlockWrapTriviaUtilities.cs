using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

/// <summary>
/// Carries the comment trivia of code that a phase wraps in new braces across those braces (FMT-PIPE-013). The comments
/// keep the layout the author gave them: the whitespace in front of and between them, the blank lines between them, and a
/// comment written in front of the code on the code's line. Each caller passes the separators its stage of the pipeline
/// needs, because the structural transforms run before the line-break phase and the switch-case brace phase runs after it
/// </summary>
internal static class BlockWrapTriviaUtilities
{
    #region Methods

    /// <summary>
    /// Builds the leading trivia that the wrapped code carries across the brace in front of it. Everything from the first
    /// comment on is kept as written, together with the whitespace in front of it on its line. The whitespace and line breaks
    /// in front of that line are dropped unless <paramref name="keepBlankLinesAboveFirstComment"/> is set, and the whole
    /// trivia is dropped when it holds no comment, because the caller writes the separator in front of the code
    /// </summary>
    /// <param name="leadingTrivia">The original leading trivia of the wrapped code</param>
    /// <param name="keepBlankLinesAboveFirstComment">Whether the blank lines in front of the first comment's line are kept as written</param>
    /// <returns>The carried leading trivia</returns>
    public static SyntaxTriviaList GetCarriedLeadingTrivia(SyntaxTriviaList leadingTrivia, bool keepBlankLinesAboveFirstComment)
    {
        var firstCommentIndex = TriviaListUtilities.IndexOfFirstComment(leadingTrivia);

        if (firstCommentIndex < 0)
        {
            return default;
        }

        if (keepBlankLinesAboveFirstComment)
        {
            return leadingTrivia;
        }

        var lineStartIndex = firstCommentIndex;

        while (lineStartIndex > 0
               && SyntaxTriviaUtilities.EndsLine(leadingTrivia[lineStartIndex - 1]) == false)
        {
            lineStartIndex--;
        }

        return SyntaxFactory.TriviaList(leadingTrivia.Skip(lineStartIndex));
    }

    /// <summary>
    /// Builds the trailing trivia that the wrapped code carries across the brace behind it. The comments are kept with the
    /// whitespace in front of and between them as written. When <paramref name="endsLine"/> is set, the trivia ends behind
    /// the last comment with <paramref name="lineBreak"/>, because the caller's brace starts the next line. Otherwise the
    /// trivia is kept as written, its line break included, and gets <paramref name="lineBreak"/> only when a single-line
    /// comment ends it without one, because the brace behind it would otherwise be part of the comment
    /// </summary>
    /// <param name="trailingTrivia">The original trailing trivia of the wrapped code</param>
    /// <param name="lineBreak">The line break the separator uses</param>
    /// <param name="endsLine">Whether the trivia must end the line</param>
    /// <returns>The carried trailing trivia</returns>
    public static SyntaxTriviaList GetCarriedTrailingTrivia(SyntaxTriviaList trailingTrivia, SyntaxTrivia lineBreak, bool endsLine)
    {
        if (endsLine)
        {
            var lastCommentIndex = -1;

            for (var triviaIndex = 0; triviaIndex < trailingTrivia.Count; triviaIndex++)
            {
                if (SyntaxTriviaUtilities.IsCommentTrivia(trailingTrivia[triviaIndex]))
                {
                    lastCommentIndex = triviaIndex;
                }
            }

            return SyntaxFactory.TriviaList(trailingTrivia.Take(lastCommentIndex + 1))
                                .Add(lineBreak);
        }

        var lastTrivia = trailingTrivia.LastOrDefault(static trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false);

        return lastTrivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                   ? trailingTrivia.Add(lineBreak)
                   : trailingTrivia;
    }

    #endregion // Methods
}