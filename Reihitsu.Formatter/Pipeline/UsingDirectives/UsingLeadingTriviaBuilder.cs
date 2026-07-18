using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives;

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
    /// <param name="firstLeadingTriviaPrefix">Whitespace prefix from the first using directive</param>
    /// <param name="startsNewGroup"><see langword="true"/> if the using starts a new group</param>
    /// <param name="isFirst"><see langword="true"/> if the using is the first directive in the block</param>
    /// <param name="endOfLine">Preferred end-of-line sequence</param>
    /// <returns>The leading trivia to apply</returns>
    public static SyntaxTriviaList CreateLeadingTrivia(UsingDirectiveSyntax current,
                                                       SyntaxTriviaList firstLeadingTriviaPrefix,
                                                       bool startsNewGroup,
                                                       bool isFirst,
                                                       string endOfLine)
    {
        var leadingTrivia = current.GetLeadingTrivia();
        var firstSignificantTriviaIndex = GetFirstSignificantTriviaIndex(leadingTrivia);

        if (firstSignificantTriviaIndex < 0)
        {
            if (isFirst)
            {
                return firstLeadingTriviaPrefix;
            }

            var indentation = GetIndentationTrivia(leadingTrivia);

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

        var indentationBeforeSignificantTrivia = GetIndentationTriviaBefore(leadingTrivia, firstSignificantTriviaIndex);
        var linePrefix = startsNewGroup
                             ? SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(endOfLine))
                             : SyntaxFactory.TriviaList();

        return linePrefix.AddRange(indentationBeforeSignificantTrivia)
                         .AddRange(significantLeadingTrivia);
    }

    /// <summary>
    /// Splits the leading trivia of the original first using directive into a header block that must
    /// stay pinned at the top of the scope and the remainder that continues to belong to the directive
    /// if it is reordered away from the first position. The header is whatever significant trivia is
    /// separated from the directive by a blank line; when no blank line separates the significant
    /// trivia from the directive, the whole significant trivia is treated as the header. Any
    /// whitespace or end-of-line trivia before the first significant trivia is excluded from the
    /// header: callers combine it with <see cref="GetWhitespacePrefix"/>, which already covers that
    /// same span, so including it here would duplicate it
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia of the original first using directive</param>
    /// <returns>The header trivia and the trivia that remains attached to the directive</returns>
    public static (SyntaxTriviaList Header, SyntaxTriviaList Remainder) SplitOriginalFirstHeaderTrivia(SyntaxTriviaList leadingTrivia)
    {
        var firstSignificantTriviaIndex = GetFirstSignificantTriviaIndex(leadingTrivia);

        if (firstSignificantTriviaIndex < 0)
        {
            return (SyntaxFactory.TriviaList(), leadingTrivia);
        }

        var splitIndex = GetHeaderSplitIndex(leadingTrivia, firstSignificantTriviaIndex);
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
        var result = new List<SyntaxTrivia>();

        foreach (var trivia in triviaList)
        {
            if (IsWhitespaceOrEndOfLineTrivia(trivia) == false)
            {
                break;
            }

            result.Add(trivia);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Gets the first index containing non-whitespace trivia
    /// </summary>
    /// <param name="triviaList">Trivia list</param>
    /// <returns>The index of the first significant trivia, or -1 when none exists</returns>
    private static int GetFirstSignificantTriviaIndex(SyntaxTriviaList triviaList)
    {
        for (var triviaIndex = 0; triviaIndex < triviaList.Count; triviaIndex++)
        {
            if (IsWhitespaceOrEndOfLineTrivia(triviaList[triviaIndex]) == false)
            {
                return triviaIndex;
            }
        }

        return -1;
    }

    /// <summary>
    /// Finds the index right after the last blank line within the significant trivia. Comments before
    /// that point are a header block separated from the directive by a blank line; when no blank line
    /// exists, the split lands at the end of the list so the whole significant trivia becomes the header
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia to inspect</param>
    /// <param name="firstSignificantTriviaIndex">Index of the first significant trivia</param>
    /// <returns>The split index between the header block and the directive-attached remainder</returns>
    private static int GetHeaderSplitIndex(SyntaxTriviaList leadingTrivia, int firstSignificantTriviaIndex)
    {
        var splitIndex = leadingTrivia.Count;

        for (var triviaIndex = firstSignificantTriviaIndex; triviaIndex < leadingTrivia.Count - 1; triviaIndex++)
        {
            if (leadingTrivia[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia)
                && leadingTrivia[triviaIndex + 1].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                splitIndex = triviaIndex + 2;
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

    /// <summary>
    /// Determines whether the trivia is whitespace or an end-of-line marker
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia is whitespace-only</returns>
    private static bool IsWhitespaceOrEndOfLineTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.WhitespaceTrivia)
               || trivia.IsKind(SyntaxKind.EndOfLineTrivia);
    }

    #endregion // Methods
}