using Microsoft.CodeAnalysis;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities;

/// <summary>
/// Shared helpers for trailing-comma removal transforms
/// </summary>
internal static class TrailingCommaRemovalUtilities
{
    #region Methods

    /// <summary>
    /// Moves the trailing trivia of a removed trailing comma to the token the text gives it once the comma is gone. The
    /// comma's leading trivia holds only whitespace and line breaks, because the precheck refuses a comment, a directive,
    /// or disabled text in front of a comma on a later line (FMT-UNSUP-009), so it is dropped. When a line break already
    /// ends the last element's line, the comma stood on a later line, so the comments behind it stand on a line that in
    /// the text belongs to the closing brace: the trivia then leads the closing brace (FMT-STRUCT-027). Otherwise the comma
    /// stood on the last element's line, and its trivia trails the last element
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

        var triviaToPreserve = separator.TrailingTrivia;

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

    #endregion // Methods
}