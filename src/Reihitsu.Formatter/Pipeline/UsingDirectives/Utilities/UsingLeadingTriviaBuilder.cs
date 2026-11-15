using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives.Utilities;

/// <summary>
/// The leading-trivia reconstruction half of the using-directive ordering phase. It rebuilds the
/// leading trivia of each reordered directive so indentation, attached comments and blank-line group
/// separators survive the move. Deciding the order itself belongs to <see cref="UsingGrouping"/>
/// </summary>
internal static class UsingLeadingTriviaBuilder
{
    #region Methods

    /// <summary>
    /// Creates the leading trivia for the reordered using directive
    /// </summary>
    /// <param name="current">Current using directive</param>
    /// <param name="firstLeadingTriviaPrefix">
    /// Trivia that starts the block: the whitespace prefix of the original first using directive, followed by the header
    /// kept at the top and, when that header ends its line, by the whitespace of the original first directive's line
    /// </param>
    /// <param name="startsNewGroup"><see langword="true"/> if the using starts a new group</param>
    /// <param name="isFirst"><see langword="true"/> if the using is the first directive in the block</param>
    /// <param name="lineIndentation">
    /// Indentation to place the directive at instead of the indentation read from its own leading trivia —
    /// used for a directive that did not start its own line before the rebuild but does after it — or
    /// <see langword="null"/> to keep that indentation. Ignored for the first directive
    /// </param>
    /// <param name="endOfLine">Preferred end-of-line sequence</param>
    /// <returns>The leading trivia to apply</returns>
    public static SyntaxTriviaList CreateLeadingTrivia(UsingDirectiveSyntax current,
                                                       SyntaxTriviaList firstLeadingTriviaPrefix,
                                                       bool startsNewGroup,
                                                       bool isFirst,
                                                       SyntaxTriviaList? lineIndentation,
                                                       string endOfLine)
    {
        var leadingTrivia = current.GetLeadingTrivia();
        var firstSignificantTriviaIndex = SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(leadingTrivia);

        if (firstSignificantTriviaIndex < 0)
        {
            if (isFirst)
            {
                return firstLeadingTriviaPrefix;
            }

            var indentation = lineIndentation ?? GetIndentationTrivia(leadingTrivia);

            return startsNewGroup
                       ? SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(endOfLine))
                                      .AddRange(indentation)
                       : indentation;
        }

        var significantLeadingTrivia = SyntaxFactory.TriviaList(leadingTrivia.Skip(firstSignificantTriviaIndex));

        if (isFirst)
        {
            return firstLeadingTriviaPrefix.AddRange(significantLeadingTrivia);
        }

        var indentationBeforeSignificantTrivia = lineIndentation ?? GetIndentationTriviaBefore(leadingTrivia, firstSignificantTriviaIndex);
        var linePrefix = startsNewGroup
                             ? SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(endOfLine))
                             : SyntaxFactory.TriviaList();

        return linePrefix.AddRange(indentationBeforeSignificantTrivia)
                         .AddRange(significantLeadingTrivia);
    }

    /// <summary>
    /// Determines whether a line break stands in the layout in front of the first significant trivia of a leading
    /// trivia list, so that this trivia starts a line of its own
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia to inspect</param>
    /// <returns><see langword="true"/> if the list holds significant trivia with a line break in front of it; otherwise, <see langword="false"/></returns>
    public static bool HasLineBreakBeforeSignificantTrivia(SyntaxTriviaList leadingTrivia)
    {
        var firstSignificantTriviaIndex = SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(leadingTrivia);

        return firstSignificantTriviaIndex > 0
               && leadingTrivia.Take(firstSignificantTriviaIndex).Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
    }

    /// <summary>
    /// Splits the leading trivia of the original first using directive into a header block that must
    /// stay pinned at the top of the scope and the remainder that continues to belong to the directive
    /// if it is reordered away from the first position. The header is whatever significant trivia is
    /// separated from the directive by a blank line; when no blank line separates the significant
    /// trivia from the directive, the whole significant trivia is treated as the header. Any
    /// whitespace or end-of-line trivia before the first significant trivia is excluded from the
    /// header: callers combine it with <see cref="GetWhitespacePrefix"/>, which already covers that
    /// same span, so including it here would duplicate it. When the header ends its last line (see
    /// <see cref="SyntaxTriviaUtilities.EndsLine"/>) and only whitespace follows, that whitespace starts
    /// the directive's line and stays in the remainder, so the directive keeps the whitespace in front
    /// of its own line when it moves away from the top (FMT-USING-011)
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia of the original first using directive</param>
    /// <returns>The header trivia and the trivia that remains attached to the directive</returns>
    public static (SyntaxTriviaList Header, SyntaxTriviaList Remainder) SplitOriginalFirstHeaderTrivia(SyntaxTriviaList leadingTrivia)
    {
        var firstSignificantTriviaIndex = SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(leadingTrivia);

        if (firstSignificantTriviaIndex < 0)
        {
            return (SyntaxFactory.TriviaList(), leadingTrivia);
        }

        var splitIndex = GetHeaderSplitIndex(leadingTrivia, firstSignificantTriviaIndex);
        var lineStartIndex = splitIndex;

        while (lineStartIndex > firstSignificantTriviaIndex
               && leadingTrivia[lineStartIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            lineStartIndex--;
        }

        if (lineStartIndex > firstSignificantTriviaIndex
            && SyntaxTriviaUtilities.EndsLine(leadingTrivia[lineStartIndex - 1]))
        {
            splitIndex = lineStartIndex;
        }

        var header = leadingTrivia.Skip(firstSignificantTriviaIndex).Take(splitIndex - firstSignificantTriviaIndex);

        return (SyntaxFactory.TriviaList(header), SyntaxFactory.TriviaList(leadingTrivia.Skip(splitIndex)));
    }

    /// <summary>
    /// Gets the whitespace-only prefix from the start of the trivia list
    /// </summary>
    /// <param name="triviaList">Trivia list</param>
    /// <returns>The whitespace-only prefix</returns>
    public static SyntaxTriviaList GetWhitespacePrefix(SyntaxTriviaList triviaList)
    {
        var firstSignificantTriviaIndex = SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(triviaList);

        if (firstSignificantTriviaIndex < 0)
        {
            return triviaList;
        }

        return SyntaxFactory.TriviaList(triviaList.Take(firstSignificantTriviaIndex));
    }

    /// <summary>
    /// Finds the index right after the last blank line within the significant trivia. Comments before
    /// that point are a header block separated from the directive by a blank line; when no blank line
    /// exists, the split lands at the end of the list so the whole significant trivia becomes the header.
    /// What counts as a blank line is the shared measure of <see cref="TokenGapAnalysis"/>
    /// (SHARED-BLANK-001): a line that holds nothing but whitespace, also the line below a single-line
    /// documentation comment, which carries its own line break. A blank line always ends at an
    /// end-of-line trivia, so the split lands behind each one that completes another blank line
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia to inspect</param>
    /// <param name="firstSignificantTriviaIndex">Index of the first significant trivia</param>
    /// <returns>The split index between the header block and the directive-attached remainder</returns>
    private static int GetHeaderSplitIndex(SyntaxTriviaList leadingTrivia, int firstSignificantTriviaIndex)
    {
        var splitIndex = leadingTrivia.Count;
        var blankLineCount = 0;

        for (var triviaIndex = firstSignificantTriviaIndex; triviaIndex < leadingTrivia.Count; triviaIndex++)
        {
            if (leadingTrivia[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia) == false)
            {
                continue;
            }

            var blankLineCountThroughLineBreak = TokenGapAnalysis.OfTriviaRange(leadingTrivia, firstSignificantTriviaIndex, triviaIndex + 1).BlankLineCount;

            if (blankLineCountThroughLineBreak > blankLineCount)
            {
                blankLineCount = blankLineCountThroughLineBreak;
                splitIndex = triviaIndex + 1;
            }
        }

        return splitIndex;
    }

    /// <summary>
    /// Extracts indentation whitespace from the end of a trivia list
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia to inspect</param>
    /// <returns>Trivia list containing only indentation whitespace</returns>
    private static SyntaxTriviaList GetIndentationTrivia(SyntaxTriviaList leadingTrivia)
    {
        var result = new List<SyntaxTrivia>();

        for (var triviaIndex = leadingTrivia.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            if (leadingTrivia[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                result.Insert(0, leadingTrivia[triviaIndex]);
            }
            else
            {
                break;
            }
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Gets the indentation whitespace that appears immediately before the given trivia index
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia</param>
    /// <param name="significantTriviaIndex">Index of the first significant trivia</param>
    /// <returns>Trivia list containing only the indentation whitespace</returns>
    private static SyntaxTriviaList GetIndentationTriviaBefore(SyntaxTriviaList leadingTrivia, int significantTriviaIndex)
    {
        var result = new List<SyntaxTrivia>();

        for (var triviaIndex = significantTriviaIndex - 1; triviaIndex >= 0; triviaIndex--)
        {
            if (leadingTrivia[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                result.Insert(0, leadingTrivia[triviaIndex]);

                continue;
            }

            if (leadingTrivia[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                break;
            }

            result.Clear();

            break;
        }

        return SyntaxFactory.TriviaList(result);
    }

    #endregion // Methods
}