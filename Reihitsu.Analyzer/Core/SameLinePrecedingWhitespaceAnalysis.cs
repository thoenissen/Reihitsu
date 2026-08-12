using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Analyzes horizontal whitespace immediately before a source position
/// </summary>
internal static class SameLinePrecedingWhitespaceAnalysis
{
    #region Methods

    /// <summary>
    /// Gets the horizontal whitespace run immediately before a source position when the run follows source on the same line
    /// </summary>
    /// <param name="sourceText">Source text to inspect</param>
    /// <param name="position">Exclusive end of the whitespace run</param>
    /// <returns>The same-line whitespace span, or <see langword="null"/> when there is no run or the run is line-leading indentation</returns>
    internal static TextSpan? GetSpan(SourceText sourceText, int position)
    {
        var start = position;
        var lineStart = sourceText.Lines.GetLineFromPosition(position).Start;

        while (start > lineStart
               && (sourceText[start - 1] == ' ' || sourceText[start - 1] == '\t'))
        {
            start--;
        }

        return start > lineStart
               && start < position
                   ? TextSpan.FromBounds(start, position)
                   : null;
    }

    #endregion // Methods
}