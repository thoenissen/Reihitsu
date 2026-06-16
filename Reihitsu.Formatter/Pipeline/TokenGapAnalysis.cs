using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline;

/// <summary>
/// Immutable result of analysing a token gap or trivia range for line breaks and blank lines
/// </summary>
internal readonly struct TokenGapAnalysis
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="hasLineBreak">Whether any line break was found</param>
    /// <param name="blankLineCount">Number of blank lines found</param>
    private TokenGapAnalysis(bool hasLineBreak, int blankLineCount)
    {
        HasLineBreak = hasLineBreak;
        BlankLineCount = blankLineCount;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Whether the analysed gap contains at least one line break
    /// </summary>
    public bool HasLineBreak { get; }

    /// <summary>
    /// Number of blank lines found in the analysed gap
    /// </summary>
    public int BlankLineCount { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Analyses the full gap between two adjacent tokens
    /// </summary>
    /// <param name="previous">The token preceding the gap</param>
    /// <param name="next">The token following the gap</param>
    /// <returns>Analysis result for the gap</returns>
    public static TokenGapAnalysis Between(SyntaxToken previous, SyntaxToken next)
    {
        return Between(previous, next, next.LeadingTrivia.Count);
    }

    /// <summary>
    /// Analyses the gap between two adjacent tokens up to a leading-trivia prefix of the next token
    /// </summary>
    /// <param name="previous">The token preceding the gap</param>
    /// <param name="next">The token following the gap</param>
    /// <param name="leadingTriviaEndExclusive">Exclusive upper bound in the leading-trivia list of <paramref name="next"/></param>
    /// <returns>Analysis result for the gap</returns>
    public static TokenGapAnalysis Between(SyntaxToken previous, SyntaxToken next, int leadingTriviaEndExclusive)
    {
        var sawLineBreak = false;
        var lineHasContent = false;
        var blankLineCount = 0;

        AnalyzeTriviaList(previous.TrailingTrivia, 0, previous.TrailingTrivia.Count, ref sawLineBreak, ref lineHasContent, ref blankLineCount);
        AnalyzeTriviaList(next.LeadingTrivia, 0, leadingTriviaEndExclusive, ref sawLineBreak, ref lineHasContent, ref blankLineCount);

        return new TokenGapAnalysis(sawLineBreak, blankLineCount);
    }

    /// <summary>
    /// Analyses the leading-trivia prefix of a token when no previous token is available
    /// </summary>
    /// <param name="token">The token whose leading trivia should be analysed</param>
    /// <param name="endExclusive">Exclusive upper bound in the leading-trivia list</param>
    /// <returns>Analysis result for the leading-trivia range</returns>
    public static TokenGapAnalysis OfLeadingTrivia(SyntaxToken token, int endExclusive)
    {
        return OfTriviaRange(token.LeadingTrivia, 0, endExclusive);
    }

    /// <summary>
    /// Analyses an arbitrary subrange of a trivia list
    /// </summary>
    /// <param name="trivia">The trivia list to analyse</param>
    /// <param name="startIndex">Inclusive start index</param>
    /// <param name="endExclusive">Exclusive end index</param>
    /// <returns>Analysis result for the trivia range</returns>
    public static TokenGapAnalysis OfTriviaRange(SyntaxTriviaList trivia, int startIndex, int endExclusive)
    {
        var sawLineBreak = false;
        var lineHasContent = false;
        var blankLineCount = 0;

        AnalyzeTriviaList(trivia, startIndex, endExclusive, ref sawLineBreak, ref lineHasContent, ref blankLineCount);

        return new TokenGapAnalysis(sawLineBreak, blankLineCount);
    }

    /// <summary>
    /// Determines whether the specified list of trivia constitutes a blank line
    /// </summary>
    /// <param name="triviaLine">The trivia items that form a single logical line</param>
    /// <returns><see langword="true"/> if the line contains only whitespace and an end-of-line</returns>
    public static bool IsBlankLine(IReadOnlyList<SyntaxTrivia> triviaLine)
    {
        var hasEndOfLine = false;

        foreach (var triviaItem in triviaLine)
        {
            var kind = triviaItem.Kind();

            if (kind == SyntaxKind.EndOfLineTrivia)
            {
                hasEndOfLine = true;
            }
            else if (kind != SyntaxKind.WhitespaceTrivia)
            {
                return false;
            }
        }

        return hasEndOfLine;
    }

    /// <summary>
    /// Accumulates blank-line gap state over a trivia subrange
    /// </summary>
    /// <param name="triviaList">Trivia list to analyse</param>
    /// <param name="startIndex">Inclusive start index</param>
    /// <param name="endExclusive">Exclusive end index</param>
    /// <param name="sawLineBreak">Tracks whether a line break has already been encountered</param>
    /// <param name="lineHasContent">Tracks whether the current logical line contains non-whitespace trivia</param>
    /// <param name="blankLineCount">Accumulates the number of blank lines seen</param>
    private static void AnalyzeTriviaList(SyntaxTriviaList triviaList,
                                          int startIndex,
                                          int endExclusive,
                                          ref bool sawLineBreak,
                                          ref bool lineHasContent,
                                          ref int blankLineCount)
    {
        for (var triviaIndex = startIndex; triviaIndex < endExclusive; triviaIndex++)
        {
            var trivia = triviaList[triviaIndex];

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                if (sawLineBreak && lineHasContent == false)
                {
                    blankLineCount++;
                }

                sawLineBreak = true;
                lineHasContent = false;

                continue;
            }

            lineHasContent = true;

            var text = trivia.ToFullString();
            var textIndex = 0;

            while (textIndex < text.Length)
            {
                var lineBreakLength = GetLineBreakLength(text, textIndex);

                if (lineBreakLength == 0)
                {
                    textIndex++;

                    continue;
                }

                if (sawLineBreak && lineHasContent == false)
                {
                    blankLineCount++;
                }

                sawLineBreak = true;
                lineHasContent = textIndex + lineBreakLength < text.Length;
                textIndex += lineBreakLength;
            }
        }
    }

    /// <summary>
    /// Gets the length of the line-break sequence that starts at the specified index
    /// </summary>
    /// <param name="text">The text to inspect</param>
    /// <param name="index">The index to inspect</param>
    /// <returns>The length of the line-break sequence; otherwise, <c>0</c></returns>
    private static int GetLineBreakLength(string text, int index)
    {
        if (text[index] == '\r')
        {
            return index + 1 < text.Length && text[index + 1] == '\n'
                       ? 2
                       : 1;
        }

        return text[index] == '\n'
                   ? 1
                   : 0;
    }

    #endregion // Methods
}