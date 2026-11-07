using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities;

/// <summary>
/// Shared helpers for trailing-comma removal transforms
/// </summary>
internal static class TrailingCommaRemovalUtilities
{
    #region Methods

    /// <summary>
    /// Moves the trivia of a removed trailing comma to the token the text gives it once the comma is gone. When a line
    /// break already ends the last element's line, the comma stood on a later line, so its comments start a line of their
    /// own, which in the text belongs to the closing brace: the trivia then leads the closing brace (FMT-STRUCT-027).
    /// Otherwise the comma stood on the last element's line, and its trivia trails the last element
    /// </summary>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <param name="lastElement">The element in front of the removed comma</param>
    /// <param name="separator">The removed comma</param>
    /// <param name="closeBraceToken">The closing brace behind the removed comma</param>
    /// <param name="updatedLastElement">The last element, with the comma's trivia behind it when it trails the element</param>
    /// <param name="updatedCloseBraceToken">The closing brace, with the comma's trivia in front of it when it leads the brace</param>
    internal static void PreserveSeparatorTrivia<TNode>(TNode lastElement,
                                                        SyntaxToken separator,
                                                        SyntaxToken closeBraceToken,
                                                        out TNode updatedLastElement,
                                                        out SyntaxToken updatedCloseBraceToken)
        where TNode : SyntaxNode
    {
        updatedLastElement = lastElement;
        updatedCloseBraceToken = closeBraceToken;

        var triviaToPreserve = GetTriviaToPreserve(separator);

        if (triviaToPreserve.Count == 0)
        {
            return;
        }

        var lastToken = lastElement.GetLastToken();

        if (lastToken.TrailingTrivia.Any(SyntaxTriviaUtilities.EndsLine))
        {
            updatedCloseBraceToken = closeBraceToken.WithLeadingTrivia(triviaToPreserve.AddRange(closeBraceToken.LeadingTrivia));

            return;
        }

        updatedLastElement = lastElement.ReplaceToken(lastToken, lastToken.WithTrailingTrivia(lastToken.TrailingTrivia.AddRange(triviaToPreserve)));
    }

    /// <summary>
    /// Gets the separator trivia that should be preserved after removing the trailing comma
    /// </summary>
    /// <param name="separator">Separator token</param>
    /// <returns>The trivia of the separator, its leading trivia only when it holds more than whitespace and line breaks</returns>
    private static SyntaxTriviaList GetTriviaToPreserve(SyntaxToken separator)
    {
        var triviaToPreserve = SyntaxFactory.TriviaList();

        if (ContainsNonFormattingTrivia(separator.LeadingTrivia))
        {
            triviaToPreserve = triviaToPreserve.AddRange(separator.LeadingTrivia);
        }

        return triviaToPreserve.AddRange(separator.TrailingTrivia);
    }

    /// <summary>
    /// Determines whether the trivia list contains content that should be preserved when removing a separator
    /// </summary>
    /// <param name="triviaList">Trivia list</param>
    /// <returns><see langword="true"/> if the trivia contains comments, directives, or other non-formatting content</returns>
    private static bool ContainsNonFormattingTrivia(SyntaxTriviaList triviaList)
    {
        return triviaList.Any(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                                        && trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false);
    }

    #endregion // Methods
}