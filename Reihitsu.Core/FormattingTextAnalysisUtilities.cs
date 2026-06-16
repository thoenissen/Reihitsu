using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Provides helpers for line-oriented formatting analysis
/// </summary>
public static class FormattingTextAnalysisUtilities
{
    #region Methods

    /// <summary>
    /// Finds the first non-blank line in the source text
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <returns>The zero-based line index or -1 if all lines are blank</returns>
    public static int FindFirstNonBlankLineIndex(SourceText sourceText)
    {
        for (var lineIndex = 0; lineIndex < sourceText.Lines.Count; lineIndex++)
        {
            if (IsBlankLine(sourceText, lineIndex) == false)
            {
                return lineIndex;
            }
        }

        return -1;
    }

    /// <summary>
    /// Finds the previous non-blank line before the specified line index
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="startLineIndex">Line index to start from</param>
    /// <returns>The zero-based line index or -1 if no previous non-blank line exists</returns>
    public static int FindPreviousNonBlankLineIndex(SourceText sourceText, int startLineIndex)
    {
        for (var lineIndex = startLineIndex - 1; lineIndex >= 0; lineIndex--)
        {
            if (IsBlankLine(sourceText, lineIndex) == false)
            {
                return lineIndex;
            }
        }

        return -1;
    }

    /// <summary>
    /// Gets the line indices occupied by string literals and interpolated strings
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="sourceText">Source text</param>
    /// <returns>The occupied line indices</returns>
    public static HashSet<int> GetStringLineIndices(SyntaxNode root, SourceText sourceText)
    {
        var stringLineIndices = new HashSet<int>();

        foreach (var node in root.DescendantNodes())
        {
            switch (node)
            {
                case LiteralExpressionSyntax literalExpression when IsTrackedStringLiteral(literalExpression):
                    AddIntersectingLineIndices(stringLineIndices, sourceText, literalExpression.Span);
                    break;

                case InterpolatedStringExpressionSyntax interpolatedString:
                    AddIntersectingLineIndices(stringLineIndices, sourceText, interpolatedString.Span);
                    break;
            }
        }

        return stringLineIndices;
    }

    /// <summary>
    /// Gets the line indices that the trivia-based blank-line formatting never rewrites
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="sourceText">Source text</param>
    /// <returns>The line indices occupied by tracked strings, multi-line comments, and preprocessor-disabled text</returns>
    public static HashSet<int> GetNonFormattableLineIndices(SyntaxNode root, SourceText sourceText)
    {
        var lineIndices = GetStringLineIndices(root, sourceText);

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
                || trivia.IsKind(SyntaxKind.DisabledTextTrivia))
            {
                AddIntersectingLineIndices(lineIndices, sourceText, trivia.FullSpan);
            }
        }

        return lineIndices;
    }

    /// <summary>
    /// Gets the line indices whose first non-whitespace token matches the specified predicate
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="predicate">Predicate evaluated for each token</param>
    /// <returns>The matching line indices</returns>
    public static HashSet<int> GetLineIndicesBeginningWithToken(SyntaxNode root, SourceText sourceText, Func<SyntaxToken, bool> predicate)
    {
        var lineIndices = new HashSet<int>();

        foreach (var token in root.DescendantTokens())
        {
            if (predicate(token) == false)
            {
                continue;
            }

            var line = sourceText.Lines.GetLineFromPosition(token.SpanStart);

            if (string.IsNullOrWhiteSpace(sourceText.ToString(TextSpan.FromBounds(line.Start, token.SpanStart))))
            {
                lineIndices.Add(line.LineNumber);
            }
        }

        return lineIndices;
    }

    /// <summary>
    /// Gets the line indices whose last non-whitespace token matches the specified predicate
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="predicate">Predicate evaluated for each token</param>
    /// <returns>The matching line indices</returns>
    public static HashSet<int> GetLineIndicesEndingWithToken(SyntaxNode root, SourceText sourceText, Func<SyntaxToken, bool> predicate)
    {
        var lineIndices = new HashSet<int>();

        foreach (var token in root.DescendantTokens())
        {
            if (predicate(token) == false)
            {
                continue;
            }

            var line = sourceText.Lines.GetLineFromPosition(token.Span.End);

            if (string.IsNullOrWhiteSpace(sourceText.ToString(TextSpan.FromBounds(token.Span.End, line.End))))
            {
                lineIndices.Add(line.LineNumber);
            }
        }

        return lineIndices;
    }

    /// <summary>
    /// Enumerates blank lines that immediately precede one of the specified target lines
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="targetLineIndices">Line indices that must be preceded by a blank line</param>
    /// <returns>The preceding blank lines</returns>
    public static IEnumerable<TextLine> EnumeratePrecedingBlankLines(SourceText sourceText, ISet<int> targetLineIndices)
    {
        for (var lineIndex = 1; lineIndex < sourceText.Lines.Count; lineIndex++)
        {
            if (targetLineIndices.Contains(lineIndex)
                && IsBlankLine(sourceText, lineIndex - 1))
            {
                yield return sourceText.Lines[lineIndex - 1];
            }
        }
    }

    /// <summary>
    /// Enumerates blank lines that immediately follow one of the specified target lines
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="targetLineIndices">Line indices that must be followed by a blank line</param>
    /// <returns>The following blank lines</returns>
    public static IEnumerable<TextLine> EnumerateFollowingBlankLines(SourceText sourceText, ISet<int> targetLineIndices)
    {
        for (var lineIndex = 0; lineIndex < sourceText.Lines.Count - 1; lineIndex++)
        {
            if (targetLineIndices.Contains(lineIndex)
                && IsBlankLine(sourceText, lineIndex + 1))
            {
                yield return sourceText.Lines[lineIndex + 1];
            }
        }
    }

    /// <summary>
    /// Enumerates blank lines that are immediately preceded by another blank line
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="excludedLineIndices">Line indices that should be ignored</param>
    /// <returns>The blank lines that follow another blank line</returns>
    public static IEnumerable<TextLine> EnumerateConsecutiveBlankLines(SourceText sourceText, ISet<int> excludedLineIndices)
    {
        for (var lineIndex = 1; lineIndex < sourceText.Lines.Count; lineIndex++)
        {
            if (excludedLineIndices.Contains(lineIndex)
                || excludedLineIndices.Contains(lineIndex - 1))
            {
                continue;
            }

            if (IsBlankLine(sourceText, lineIndex)
                && IsBlankLine(sourceText, lineIndex - 1))
            {
                yield return sourceText.Lines[lineIndex];
            }
        }
    }

    /// <summary>
    /// Gets the text of a line without the line break characters
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="line">Line</param>
    /// <returns>Line text</returns>
    public static string GetLineText(SourceText sourceText, TextLine line)
    {
        return sourceText.ToString(TextSpan.FromBounds(line.Start, line.End));
    }

    /// <summary>
    /// Gets the start index of the contiguous run of spaces and tabs that ends at the given position
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="position">Exclusive end position of the run</param>
    /// <param name="lowerBound">Lowest index the scan may reach</param>
    /// <returns>The start index of the whitespace run; equals <paramref name="position"/> when no run is present</returns>
    public static int GetLeadingWhitespaceRunStart(SourceText sourceText, int position, int lowerBound)
    {
        var start = position;

        while (start > lowerBound
               && (sourceText[start - 1] == ' ' || sourceText[start - 1] == '\t'))
        {
            start--;
        }

        return start;
    }

    /// <summary>
    /// Gets the leading whitespace of a line
    /// </summary>
    /// <param name="lineText">Line text</param>
    /// <returns>The leading whitespace</returns>
    public static string GetLeadingWhitespace(string lineText)
    {
        var length = 0;

        while (length < lineText.Length
               && char.IsWhiteSpace(lineText[length]))
        {
            length++;
        }

        return lineText.Substring(0, length);
    }

    /// <summary>
    /// Gets the index where trailing whitespace starts
    /// </summary>
    /// <param name="lineText">Line text</param>
    /// <returns>The zero-based index of trailing whitespace</returns>
    public static int GetTrailingWhitespaceStart(string lineText)
    {
        var lastNonWhitespaceIndex = lineText.Length - 1;

        while (lastNonWhitespaceIndex >= 0
               && char.IsWhiteSpace(lineText[lastNonWhitespaceIndex]))
        {
            lastNonWhitespaceIndex--;
        }

        return lastNonWhitespaceIndex + 1;
    }

    /// <summary>
    /// Determines whether the specified token is part of an interpolated string
    /// </summary>
    /// <param name="token">Token to inspect</param>
    /// <returns><see langword="true"/> if the token belongs to an interpolated string</returns>
    public static bool IsInsideInterpolatedString(SyntaxToken token)
    {
        return token.Parent?.AncestorsAndSelf().OfType<InterpolatedStringExpressionSyntax>().Any() == true;
    }

    /// <summary>
    /// Determines whether the specified line is blank
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="lineIndex">Line index</param>
    /// <returns><see langword="true"/> if the line is blank</returns>
    public static bool IsBlankLine(SourceText sourceText, int lineIndex)
    {
        return string.IsNullOrWhiteSpace(GetLineText(sourceText, sourceText.Lines[lineIndex]));
    }

    /// <summary>
    /// Adds every line index touched by the specified span
    /// </summary>
    /// <param name="lineIndices">Target set</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="span">Span to map</param>
    private static void AddIntersectingLineIndices(HashSet<int> lineIndices, SourceText sourceText, TextSpan span)
    {
        var startLineIndex = sourceText.Lines.GetLineFromPosition(span.Start).LineNumber;
        var endPosition = span.Length == 0 ? span.End : span.End - 1;
        var endLineIndex = sourceText.Lines.GetLineFromPosition(endPosition).LineNumber;

        for (var lineIndex = startLineIndex; lineIndex <= endLineIndex; lineIndex++)
        {
            lineIndices.Add(lineIndex);
        }
    }

    /// <summary>
    /// Determines whether the specified literal expression represents a tracked string literal
    /// </summary>
    /// <param name="literalExpression">Literal expression</param>
    /// <returns><see langword="true"/> if the literal should be tracked</returns>
    private static bool IsTrackedStringLiteral(LiteralExpressionSyntax literalExpression)
    {
        if (literalExpression.IsKind(SyntaxKind.StringLiteralExpression) == false
            && literalExpression.IsKind(SyntaxKind.Utf8StringLiteralExpression) == false)
        {
            return false;
        }

        return true;
    }

    #endregion // Methods
}