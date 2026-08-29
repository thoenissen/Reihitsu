using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives.Utilities;

/// <summary>
/// The trailing-trivia reconstruction half of the using-directive ordering phase. Reordering carries
/// each directive's original trailing trivia along with its node, but that trivia was authored for the
/// directive's old neighbor. It only becomes wrong when the old neighbor relationship it encodes cannot
/// safely carry over: a directive that ends up with a successor needs a genuine gap between the two —
/// existing whitespace, an existing line break, or a self-terminating block comment already provide one
/// and are left untouched, while empty trailing trivia or an unterminated single-line comment do not and
/// gain exactly one appended line break. A directive that ends up last needs none of that — nothing
/// follows it within the block — except that an unterminated single-line comment it already carries must
/// still be closed before the block's own original closing shape (whitespace and/or a line break,
/// transplanted rather than reduced to a single flag) is appended after it
/// </summary>
internal static class UsingTrailingTriviaBuilder
{
    #region Methods

    /// <summary>
    /// Creates the trailing trivia for a reordered directive
    /// </summary>
    /// <param name="current">Current using directive</param>
    /// <param name="isLast"><see langword="true"/> if the directive is the last in the reordered block</param>
    /// <param name="originalBlockTerminalTrivia">
    /// The layout trivia (whitespace and end-of-line) that trailed the original, pre-reorder block
    /// </param>
    /// <param name="endOfLine">Preferred end-of-line sequence</param>
    /// <returns>The trailing trivia to apply</returns>
    public static SyntaxTriviaList CreateTrailingTrivia(UsingDirectiveSyntax current,
                                                        bool isLast,
                                                        SyntaxTriviaList originalBlockTerminalTrivia,
                                                        string endOfLine)
    {
        var trailingTrivia = current.GetTrailingTrivia();

        if (isLast)
        {
            var contentPrefix = StripTrailingLayoutTrivia(trailingTrivia);

            if (EndsInUnterminatedSingleLineComment(contentPrefix) && StartsWithLineBreak(originalBlockTerminalTrivia) == false)
            {
                return contentPrefix.Add(SyntaxFactory.EndOfLine(endOfLine)).AddRange(originalBlockTerminalTrivia);
            }

            return contentPrefix.AddRange(originalBlockTerminalTrivia);
        }

        return RequiresSeparatingLineBreak(trailingTrivia)
                   ? trailingTrivia.Add(SyntaxFactory.EndOfLine(endOfLine))
                   : trailingTrivia;
    }

    /// <summary>
    /// Extracts the trailing run of whitespace and end-of-line trivia from a trivia list — the layout
    /// shape that trailed whatever content, if any, came before it
    /// </summary>
    /// <param name="trivia">Trivia list to inspect</param>
    /// <returns>The trailing layout trivia</returns>
    public static SyntaxTriviaList GetTrailingLayoutTrivia(SyntaxTriviaList trivia)
    {
        return SyntaxFactory.TriviaList(trivia.Skip(FindTrailingLayoutSplitIndex(trivia)));
    }

    /// <summary>
    /// Determines whether a directive's trailing trivia provides no safe separation from whatever
    /// immediately follows it: either nothing trails the directive at all, or the trivia ends in a
    /// single-line comment that has not been terminated by a line break and would otherwise swallow the
    /// next directive's text into itself
    /// </summary>
    /// <param name="trailingTrivia">Trailing trivia to inspect</param>
    /// <returns><see langword="true"/> if a line break must be added; otherwise, <see langword="false"/></returns>
    private static bool RequiresSeparatingLineBreak(SyntaxTriviaList trailingTrivia)
    {
        return trailingTrivia.Count == 0 || EndsInUnterminatedSingleLineComment(trailingTrivia);
    }

    /// <summary>
    /// Determines whether a trivia list ends in a single-line comment that has not been terminated by a
    /// line break. Such a comment extends to the end of whatever line it sits on, so any content placed
    /// after it — a transplanted block terminator included — would be silently absorbed into the comment
    /// unless a line break closes it first. A self-terminating block comment carries no such risk and is
    /// not covered here
    /// </summary>
    /// <param name="trivia">Trivia list to inspect</param>
    /// <returns><see langword="true"/> if the list ends in an unterminated single-line comment; otherwise, <see langword="false"/></returns>
    private static bool EndsInUnterminatedSingleLineComment(SyntaxTriviaList trivia)
    {
        if (trivia.Count == 0)
        {
            return false;
        }

        var lastTrivia = trivia[trivia.Count - 1];

        return lastTrivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || lastTrivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia);
    }

    /// <summary>
    /// Determines whether a trivia list begins with an end-of-line trivia. When the block's own
    /// terminating trivia starts this way, appending it after an unterminated single-line comment already
    /// closes that comment, so no additional line break needs to be inserted ahead of it
    /// </summary>
    /// <param name="trivia">Trivia list to inspect</param>
    /// <returns><see langword="true"/> if the list begins with an end-of-line trivia; otherwise, <see langword="false"/></returns>
    private static bool StartsWithLineBreak(SyntaxTriviaList trivia)
    {
        return trivia.Count > 0 && trivia[0].IsKind(SyntaxKind.EndOfLineTrivia);
    }

    /// <summary>
    /// Removes the trailing run of whitespace and end-of-line trivia from a trivia list, keeping any
    /// content — such as a comment — that precedes it
    /// </summary>
    /// <param name="trivia">Trivia list to strip</param>
    /// <returns>The trivia list without its trailing layout trivia</returns>
    private static SyntaxTriviaList StripTrailingLayoutTrivia(SyntaxTriviaList trivia)
    {
        return SyntaxFactory.TriviaList(trivia.Take(FindTrailingLayoutSplitIndex(trivia)));
    }

    /// <summary>
    /// Finds the index at which the trailing run of whitespace and end-of-line trivia begins
    /// </summary>
    /// <param name="trivia">Trivia list to inspect</param>
    /// <returns>The split index between any leading content and the trailing layout trivia</returns>
    private static int FindTrailingLayoutSplitIndex(SyntaxTriviaList trivia)
    {
        var splitIndex = trivia.Count;

        while (splitIndex > 0 && IsLayoutTrivia(trivia[splitIndex - 1]))
        {
            splitIndex--;
        }

        return splitIndex;
    }

    /// <summary>
    /// Determines whether a trivia is whitespace or an end-of-line marker
    /// </summary>
    /// <param name="trivia">The trivia to check</param>
    /// <returns><see langword="true"/> if the trivia is whitespace or an end-of-line marker; otherwise, <see langword="false"/></returns>
    private static bool IsLayoutTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia);
    }

    #endregion // Methods
}