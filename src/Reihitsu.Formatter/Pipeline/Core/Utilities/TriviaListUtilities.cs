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