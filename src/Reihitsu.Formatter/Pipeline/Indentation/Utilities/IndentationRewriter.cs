using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.Core.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Utilities;

/// <summary>
/// Applies the computed layout model to a syntax tree by writing each line's column in front of the element its text
/// starts at, as <see cref="LineStartIndex"/> names it, then moving the lines of multi-line raw string literals by the
/// shifts the same model computed
/// </summary>
internal static class IndentationRewriter
{
    #region Methods

    /// <summary>
    /// Applies indentation from the layout model to the syntax tree. Every line is placed first, at the layout's column
    /// before the move; then the content lines, the hole lines, and the closing marker of each multi-line raw string
    /// literal move to the column <see cref="LayoutModel.GetColumnAfterRawStringShift"/> gives their line, which adds the
    /// shift every anchor on those lines was measured with, so the literal's closing marker ends up under its opening
    /// quotes (FMT-RAW-001). A placed line moves from the layout's column, not from the whitespace written for it, so a
    /// line whose column is below 0 before the move gets no whitespace here and its column from the move. The model
    /// numbers the lines of the root's tree, and placing lines changes no line break, so the rewritten tree has the same
    /// line numbers; the root is the root of its own tree, as <see cref="IndentationPhase"/> makes an attached input
    /// </summary>
    /// <param name="root">The syntax tree root</param>
    /// <param name="model">The computed layout model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The syntax tree with corrected indentation</returns>
    public static SyntaxNode Apply(SyntaxNode root, LayoutModel model, CancellationToken cancellationToken)
    {
        var lineStarts = LineStartIndex.For(root.SyntaxTree);
        var placedColumns = new Dictionary<int, int>();
        var indented = root.ReplaceTokens(root.DescendantTokens(), (original, rewritten) => ApplyIndentation(original, rewritten, lineStarts, model, placedColumns, cancellationToken));

        return RawStringLineShifts.MoveLines(indented,
                                             (lineNumber, writtenColumn) => model.GetColumnAfterRawStringShift(lineNumber, placedColumns.TryGetValue(lineNumber, out var placedColumn) ? placedColumn : writtenColumn),
                                             cancellationToken);
    }

    #endregion // Methods

    #region Private methods

    /// <summary>
    /// Applies indentation to a single token based on the layout model. The text of a line starts at one element, which
    /// <see cref="LineStartIndex"/> names: a comment, directive, documentation comment, or disabled text in the token's
    /// leading trivia, or the token itself. The line's column is written in front of that element, replacing the whitespace
    /// that stands there, and nowhere else on the line; every other trivia keeps its text. Each element's line is the one
    /// Roslyn's line table gives its position, which is the line the layout model numbers. A whitespace-only line at the
    /// end of the leading trivia of a token without text, such as the end of the file, takes the column its line has
    /// </summary>
    /// <param name="original">The original token from the syntax tree</param>
    /// <param name="rewritten">The rewritten token</param>
    /// <param name="lineStarts">The line starts of the tree</param>
    /// <param name="model">The computed layout model</param>
    /// <param name="placedColumns">Receives the column before the move of every line placed, and of every later line of a placed <c>///</c> comment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The token with corrected indentation</returns>
    private static SyntaxToken ApplyIndentation(SyntaxToken original, SyntaxToken rewritten, LineStartIndex lineStarts, LayoutModel model, Dictionary<int, int> placedColumns, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (original.IsMissing)
        {
            return rewritten;
        }

        var originalTrivia = original.LeadingTrivia;
        var trivia = rewritten.LeadingTrivia;
        var result = new List<SyntaxTrivia>(trivia.Count + 1);
        var pendingWhitespace = new List<SyntaxTrivia>();
        var placesLine = false;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
        {
            var triviaItem = trivia[triviaIndex];

            if (triviaItem.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                pendingWhitespace.Add(triviaItem);

                continue;
            }

            if (triviaItem.IsKind(SyntaxKind.EndOfLineTrivia) == false
                && lineStarts.IsLineStart(originalTrivia[triviaIndex].FullSpan.Start, out var lineNumber))
            {
                placesLine = true;

                if (WriteLineStart(result, pendingWhitespace, lineNumber, model, placedColumns, IsNonRegionDirectiveTrivia(triviaItem)) is { } column
                    && triviaItem.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                {
                    triviaItem = DocumentationCommentUtilities.AlignContinuationExteriorMarkers(triviaItem, column);

                    // The later lines of the comment take the column of its first line, before the move as after it
                    var lastLine = lineStarts.GetLine(originalTrivia[triviaIndex].Span.End - 1);

                    for (var continuationLine = lineNumber + 1; continuationLine <= lastLine; continuationLine++)
                    {
                        placedColumns[continuationLine] = column;
                    }
                }
            }
            else
            {
                result.AddRange(pendingWhitespace);
            }

            pendingWhitespace.Clear();
            result.Add(triviaItem);
        }

        if (original.Span.IsEmpty == false
            && lineStarts.IsLineStart(original.SpanStart, out var tokenLine))
        {
            placesLine = true;

            WriteLineStart(result, pendingWhitespace, tokenLine, model, placedColumns, false);
        }
        else if (original.Span.IsEmpty
                 && pendingWhitespace.Count > 0
                 && TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(original, originalTrivia.Count - pendingWhitespace.Count)
                 && model.TryGetLayout(lineStarts.GetLine(original.SpanStart), out _))
        {
            placesLine = true;

            WriteLineStart(result, pendingWhitespace, lineStarts.GetLine(original.SpanStart), model, placedColumns, false);
        }
        else
        {
            result.AddRange(pendingWhitespace);
        }

        return placesLine
                   ? rewritten.WithLeadingTrivia(SyntaxFactory.TriviaList(result))
                   : rewritten;
    }

    /// <summary>
    /// Writes the start of a line in place of the whitespace in front of the element its text starts at. The whitespace in
    /// front of an active non-region directive is removed, so the directive starts in column 0. A line with a layout gets
    /// its column before the lines of multi-line raw string literals move, and that column is recorded for the move; a
    /// column below 0, which only a line that moves has, gets no whitespace, and the move gives the line its column. A
    /// line without a layout keeps its whitespace. A byte order mark at the start of the whitespace is kept in front of
    /// whatever is written
    /// </summary>
    /// <param name="result">The rebuilt trivia list</param>
    /// <param name="whitespace">The whitespace in front of the element</param>
    /// <param name="lineNumber">The 0-based line the element starts</param>
    /// <param name="model">The layout model</param>
    /// <param name="placedColumns">Receives the column before the move of the line, when it has a layout</param>
    /// <param name="isNonRegionDirective">Whether the element is an active non-region directive</param>
    /// <returns>The layout's column before the move, or <see langword="null"/> when the line has no layout or starts with a directive</returns>
    private static int? WriteLineStart(List<SyntaxTrivia> result, List<SyntaxTrivia> whitespace, int lineNumber, LayoutModel model, Dictionary<int, int> placedColumns, bool isNonRegionDirective)
    {
        var hasByteOrderMark = whitespace.Count > 0 && ByteOrderMarkUtilities.StartsWithByteOrderMark(whitespace[0]);

        if (isNonRegionDirective)
        {
            if (hasByteOrderMark)
            {
                result.Add(ByteOrderMarkUtilities.CreateByteOrderMarkTrivia());
            }

            return null;
        }

        if (model.TryGetLayout(lineNumber, out var layout) == false)
        {
            if (hasByteOrderMark)
            {
                result.Add(ByteOrderMarkUtilities.CreateByteOrderMarkTrivia());
            }
            else
            {
                result.AddRange(whitespace);
            }

            return null;
        }

        if (hasByteOrderMark)
        {
            result.Add(ByteOrderMarkUtilities.CreateByteOrderMarkTrivia());
        }

        placedColumns[lineNumber] = layout.Column;

        if (layout.Column > 0)
        {
            result.Add(SyntaxFactory.Whitespace(new string(' ', layout.Column)));
        }

        return layout.Column;
    }

    /// <summary>
    /// Determines whether the trivia entry represents an active non-region preprocessor directive
    /// </summary>
    /// <param name="trivia">The trivia entry to inspect</param>
    /// <returns><see langword="true"/> if the trivia is an active non-region directive; otherwise, <see langword="false"/></returns>
    private static bool IsNonRegionDirectiveTrivia(SyntaxTrivia trivia)
    {
        return trivia.HasStructure
               && trivia.GetStructure() is DirectiveTriviaSyntax { IsActive: true }
               && SyntaxTriviaUtilities.IsRegionDirective(trivia) == false;
    }

    #endregion // Private methods
}