using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives.Utilities;

/// <summary>
/// The trailing-trivia reconstruction half of the using-directive ordering phase. Reordering carries
/// each directive's original trailing trivia along with its node, but that trivia was authored for the
/// directive's old neighbor. It only becomes wrong when the old neighbor relationship it encodes cannot
/// safely carry over: a directive that ends up with a successor must not share its line with it. When
/// <see cref="RequiresSeparatingLineBreak"/> finds that the trailing trivia does not already separate the
/// two, the whitespace run at the end of the trailing trivia is replaced by exactly one line break, so a
/// block comment stays on the directive's line and no trailing whitespace is left behind; otherwise the
/// trailing trivia is left untouched. A directive that ends up last needs none of that — nothing follows
/// it within the block — except that an unterminated single-line comment it already carries must still be closed before the
/// block's own original closing shape (whitespace and/or a line break, transplanted rather than reduced
/// to a single flag) is appended after it
/// </summary>
internal static class UsingTrailingTriviaBuilder
{
    #region Methods

    /// <summary>
    /// Creates the trailing trivia for a reordered directive
    /// </summary>
    /// <param name="current">Current using directive</param>
    /// <param name="isLast"><see langword="true"/> if the directive is the last in the reordered block</param>
    /// <param name="requiresSeparatingLineBreak">
    /// <see langword="true"/> if the directive is not the last one and must gain a line break before its
    /// successor, as decided by <see cref="RequiresSeparatingLineBreak"/>
    /// </param>
    /// <param name="originalBlockTerminalTrivia">
    /// The layout trivia (whitespace and end-of-line) that trailed the original, pre-reorder block
    /// </param>
    /// <param name="endOfLine">Preferred end-of-line sequence</param>
    /// <returns>The trailing trivia to apply</returns>
    public static SyntaxTriviaList CreateTrailingTrivia(UsingDirectiveSyntax current,
                                                        bool isLast,
                                                        bool requiresSeparatingLineBreak,
                                                        SyntaxTriviaList originalBlockTerminalTrivia,
                                                        string endOfLine)
    {
        var trailingTrivia = current.GetTrailingTrivia();

        if (isLast)
        {
            var contentPrefix = StripTrailingLayoutTrivia(trailingTrivia);

            if (EndsInUnterminatedSingleLineComment(contentPrefix) && ContainsLineBreak(originalBlockTerminalTrivia) == false)
            {
                return contentPrefix.Add(SyntaxFactory.EndOfLine(endOfLine)).AddRange(originalBlockTerminalTrivia);
            }

            return contentPrefix.AddRange(originalBlockTerminalTrivia);
        }

        return requiresSeparatingLineBreak
                   ? StripTrailingLayoutTrivia(trailingTrivia).Add(SyntaxFactory.EndOfLine(endOfLine))
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
    /// Determines whether a directive must gain a line break before its successor in the rebuilt block.
    /// That is the case when its trailing trivia ends in an unterminated single-line comment, which would
    /// otherwise swallow whatever follows it, and when its trailing trivia contains no end-of-line trivia
    /// while the successor would otherwise not start a line of its own. A line break inside a comment
    /// between the two already places the successor on a later line: a block comment in the trailing
    /// trivia that spans lines, or a documentation comment leading the successor that spans lines — a
    /// single-line documentation comment always does. Neither counts as a line shared with the successor
    /// unless the successor starts a new group, whose blank-line separator only forms a blank line after
    /// trailing trivia that ends in an end-of-line trivia
    /// </summary>
    /// <param name="trailingTrivia">Trailing trivia of the directive</param>
    /// <param name="successorLeadingTrivia">Leading trivia of the directive that follows it</param>
    /// <param name="successorStartsNewGroup"><see langword="true"/> if the successor starts a new group</param>
    /// <returns><see langword="true"/> if a line break must be added; otherwise, <see langword="false"/></returns>
    public static bool RequiresSeparatingLineBreak(SyntaxTriviaList trailingTrivia, SyntaxTriviaList successorLeadingTrivia, bool successorStartsNewGroup)
    {
        if (EndsInUnterminatedSingleLineComment(trailingTrivia))
        {
            return true;
        }

        if (ContainsLineBreak(trailingTrivia))
        {
            return false;
        }

        if (successorStartsNewGroup)
        {
            return true;
        }

        return SpansLine(trailingTrivia) == false
               && LeadingDocumentationCommentSpansLine(successorLeadingTrivia) == false;
    }

    /// <summary>
    /// Determines whether a trivia list contains an end-of-line trivia anywhere in it. For the block's own
    /// terminating trivia, which <see cref="GetTrailingLayoutTrivia"/> guarantees contains only whitespace
    /// and end-of-line trivia, anything ahead of its first end-of-line is therefore whitespace, and
    /// re-parsing it directly after an unterminated single-line comment absorbs that whitespace into the
    /// comment's own text the same way it would have absorbed it originally, so the terminal trivia
    /// already closes the comment on its own without an additional inserted break
    /// </summary>
    /// <param name="trivia">Trivia list to inspect</param>
    /// <returns><see langword="true"/> if the list contains an end-of-line trivia; otherwise, <see langword="false"/></returns>
    public static bool ContainsLineBreak(SyntaxTriviaList trivia)
    {
        return trivia.Any(static item => item.IsKind(SyntaxKind.EndOfLineTrivia));
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
    /// Determines whether the text of a trivia list spans a line, which for trailing trivia without an
    /// end-of-line trivia means it holds a block comment that spans lines
    /// </summary>
    /// <param name="trivia">Trivia list to inspect</param>
    /// <returns><see langword="true"/> if the text contains a line break; otherwise, <see langword="false"/></returns>
    private static bool SpansLine(SyntaxTriviaList trivia)
    {
        return trivia.ToFullString().IndexOfAny(['\r', '\n']) >= 0;
    }

    /// <summary>
    /// Determines whether the documentation comments at the start of a successor's leading trivia already
    /// end the line they start on. Whitespace and documentation comments are walked from the first
    /// significant trivia on: a single-line documentation comment, or a delimited documentation comment
    /// whose text spans lines, separates the successor from the line; any other trivia — an ordinary
    /// comment or an end-of-line trivia that a reorder carried in front of the successor — or the end of
    /// the list does not
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia to inspect</param>
    /// <returns><see langword="true"/> if a leading documentation comment spans a line; otherwise, <see langword="false"/></returns>
    private static bool LeadingDocumentationCommentSpansLine(SyntaxTriviaList leadingTrivia)
    {
        foreach (var trivia in leadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            {
                return true;
            }

            if (trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) == false)
            {
                return false;
            }

            if (SpansLine(SyntaxFactory.TriviaList(trivia)))
            {
                return true;
            }
        }

        return false;
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