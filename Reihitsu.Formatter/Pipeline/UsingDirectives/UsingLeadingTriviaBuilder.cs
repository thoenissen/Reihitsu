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