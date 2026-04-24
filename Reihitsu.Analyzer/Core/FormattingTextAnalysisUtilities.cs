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
    /// Determines whether the specified line is blank.
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="lineIndex">Line index</param>
    /// <returns><see langword="true"/> if the line is blank</returns>
    internal static bool IsBlankLine(SourceText sourceText, int lineIndex)
    {
        return string.IsNullOrWhiteSpace(GetLineText(sourceText, sourceText.Lines[lineIndex]));
    }

    #endregion // Methods
}