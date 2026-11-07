using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

/// <summary>
/// Neutral edits of trivia lists that several phases apply
/// </summary>
internal static class TriviaListUtilities
{
    #region Methods

    /// <summary>
    /// Finds the first comment in a trivia list. The blank-line boundary in front of a token's comments and the
    /// documentation phase's decision which delimited documentation comment starts a new line both act on the first
    /// comment of a token's leading trivia only, so both locate it here
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns>The zero-based index of the first comment trivia, or -1 if the list holds no comment</returns>
    public static int IndexOfFirstComment(SyntaxTriviaList triviaList)
    {
        for (var triviaIndex = 0; triviaIndex < triviaList.Count; triviaIndex++)
        {
            if (SyntaxTriviaUtilities.IsCommentTrivia(triviaList[triviaIndex]))
            {
                return triviaIndex;
            }
        }

        return -1;
    }

    /// <summary>
    /// Finds the first content trivia on the line that ends at the specified index of a trivia list. The line starts behind
    /// the nearest preceding line break, whether that is end-of-line trivia or the line break a documentation comment or a
    /// directive carries inside its structure. A line break inside the text of a delimited comment does not start a line:
    /// such a comment belongs to the line it starts on. The blank-line phase and the line-break phase both locate the start
    /// of a comment's line here
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <param name="endExclusive">The exclusive end of the line's inspected part; the count of the list for the line of the token behind it</param>
    /// <returns>The index of the first content trivia on that line in front of <paramref name="endExclusive"/>; <c>-1</c> when nothing but whitespace stands there</returns>
    public static int FindFirstContentIndexOnLine(SyntaxTriviaList triviaList, int endExclusive)
    {
        var firstContentIndex = -1;

        for (var triviaIndex = endExclusive - 1; triviaIndex >= 0; triviaIndex--)
        {
            if (triviaList[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia)
                || SyntaxTriviaUtilities.EndsWithLineBreak(triviaList[triviaIndex]))
            {
                break;
            }

            if (triviaList[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia) == false)
            {
                firstContentIndex = triviaIndex;
            }
        }

        return firstContentIndex;
    }

    /// <summary>
    /// Determines whether a trivia is an ordinary, non-documentation <c>//</c> or <c>/* … */</c> comment
    /// </summary>
    /// <param name="trivia">The trivia to check</param>
    /// <returns><see langword="true"/> if the trivia is an ordinary comment</returns>
    public static bool IsOrdinaryComment(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia);
    }

    /// <summary>
    /// Removes whitespace trivia from the end of a trivia list, leaving interior whitespace (such as the space that separates
    /// a kept comment from preceding trivia) untouched
    /// </summary>
    /// <param name="triviaList">The trivia list to clean</param>
    /// <returns>The trivia list without trailing whitespace trivia</returns>
    public static SyntaxTriviaList RemoveTrailingWhitespace(SyntaxTriviaList triviaList)
    {
        var result = triviaList.ToList();

        while (result.Count > 0 && result[result.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            result.RemoveAt(result.Count - 1);
        }

        return SyntaxFactory.TriviaList(result);
    }

    #endregion // Methods
}