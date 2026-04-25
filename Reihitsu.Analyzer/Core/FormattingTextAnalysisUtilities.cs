using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Provides helpers for line-oriented formatting analysis.
/// </summary>
internal static class FormattingTextAnalysisUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the specified text contains any non-whitespace characters.
    /// </summary>
    /// <param name="text">Text to inspect</param>
    /// <returns><see langword="true"/> if the text contains non-whitespace characters</returns>
    internal static bool ContainsNonWhitespace(string text)
    {
        return string.IsNullOrWhiteSpace(text) == false;
    }

    /// <summary>
    /// Finds the first non-blank line in the source text.
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <returns>The zero-based line index or -1 if all lines are blank</returns>
    internal static int FindFirstNonBlankLineIndex(SourceText sourceText)
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
    /// Finds the previous non-blank line before the specified line index.
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="startLineIndex">Line index to start from</param>
    /// <returns>The zero-based line index or -1 if no previous non-blank line exists</returns>
    internal static int FindPreviousNonBlankLineIndex(SourceText sourceText, int startLineIndex)
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
    /// Gets the line indices occupied by multi-line raw strings.
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="sourceText">Source text</param>
    /// <returns>The occupied line indices</returns>
    internal static HashSet<int> GetRawStringLineIndices(SyntaxNode root, SourceText sourceText)
    {
        var rawStringLineIndices = new HashSet<int>();

        foreach (var token in root.DescendantTokens().Where(currentToken => currentToken.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)))
        {
            AddIntersectingLineIndices(rawStringLineIndices, sourceText, token.FullSpan);
        }

        foreach (var interpolatedString in root.DescendantNodes()
                                               .OfType<InterpolatedStringExpressionSyntax>()
                                               .Where(IsRawInterpolatedString))
        {
            AddIntersectingLineIndices(rawStringLineIndices, sourceText, interpolatedString.FullSpan);
        }

        return rawStringLineIndices;
    }

    /// <summary>
    /// Gets the text of a line without the line break characters.
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="line">Line</param>
    /// <returns>Line text</returns>
    internal static string GetLineText(SourceText sourceText, TextLine line)
    {
        return sourceText.ToString(TextSpan.FromBounds(line.Start, line.End));
    }

    /// <summary>
    /// Gets the index where trailing whitespace starts.
    /// </summary>
    /// <param name="lineText">Line text</param>
    /// <returns>The zero-based index of trailing whitespace</returns>
    internal static int GetTrailingWhitespaceStart(string lineText)
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
    /// Determines whether the specified token is part of an interpolated string.
    /// </summary>
    /// <param name="token">Token to inspect</param>
    /// <returns><see langword="true"/> if the token belongs to an interpolated string</returns>
    internal static bool IsInsideInterpolatedString(SyntaxToken token)
    {
        return token.Parent?.AncestorsAndSelf().OfType<InterpolatedStringExpressionSyntax>().Any() == true;
    }

    /// <summary>
    /// Determines whether the specified line is blank.
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="lineIndex">Line index</param>
    /// <returns><see langword="true"/> if the line is blank</returns>
    internal static bool IsBlankLine(SourceText sourceText, int lineIndex)
    {
        return string.IsNullOrWhiteSpace(GetLineText(sourceText, sourceText.Lines[lineIndex]));
    }

    /// <summary>
    /// Adds every line index touched by the specified span.
    /// </summary>
    /// <param name="lineIndices">Target set</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="span">Span to map</param>
    private static void AddIntersectingLineIndices(ISet<int> lineIndices, SourceText sourceText, TextSpan span)
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
    /// Determines whether the specified interpolated string uses raw-string delimiters.
    /// </summary>
    /// <param name="interpolatedString">Interpolated string</param>
    /// <returns><see langword="true"/> if the string is raw</returns>
    private static bool IsRawInterpolatedString(InterpolatedStringExpressionSyntax interpolatedString)
    {
        return interpolatedString.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken);
    }

    #endregion // Methods
}