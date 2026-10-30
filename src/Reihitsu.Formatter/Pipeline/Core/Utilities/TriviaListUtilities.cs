using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

/// <summary>
/// Neutral edits of trivia lists that several phases apply
/// </summary>
internal static class TriviaListUtilities
{
    #region Methods

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