using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Code fix provider base class for blank-line rules whose fix removes blank lines. It shares the whitespace-only
/// guard of <see cref="WhitespaceSpanRemovalCodeFixProviderBase"/>, which inspects the reported span. A rule that
/// reports one line of a run of blank lines names the whole run as its deleted span (SHARED-WS-001): the fix then
/// widens the reported line to every blank line directly above and below it, which holds nothing but whitespace, so
/// one application removes the run. Any other rule removes the reported span only
/// </summary>
public abstract class BlankLineSpanRemovalCodeFixProviderBase : WhitespaceSpanRemovalCodeFixProviderBase
{
    #region Fields

    /// <summary>
    /// Whether the fix removes the whole run of blank lines the reported line belongs to
    /// </summary>
    private readonly bool _removesRun;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Title</param>
    /// <param name="removesRun">
    /// Whether the fix removes the whole run of blank lines the reported line belongs to, for a rule that reports one line
    /// of the run, rather than only the reported line
    /// </param>
    private protected BlankLineSpanRemovalCodeFixProviderBase(string diagnosticId, string title, bool removesRun)
        : base(diagnosticId, title)
    {
        _removesRun = removesRun;
    }

    #endregion // Constructor

    #region WhitespaceSpanRemovalCodeFixProviderBase

    /// <inheritdoc/>
    private protected override TextSpan GetRemovalSpan(SourceText sourceText, TextSpan diagnosticSpan)
    {
        if (_removesRun == false)
        {
            return diagnosticSpan;
        }

        // The reported line is widened to every blank line directly above and below it that ends with a line break, so one
        // application leaves no blank line of the run behind (SHARED-FIX-001). Every line of the run holds only whitespace,
        // so the removed span stays whitespace-only
        var lines = sourceText.Lines;
        var firstLineIndex = lines.GetLineFromPosition(diagnosticSpan.Start).LineNumber;
        var lastLineIndex = firstLineIndex;

        while (firstLineIndex > 0
               && FormattingTextAnalysisUtilities.IsBlankLine(sourceText, firstLineIndex - 1))
        {
            firstLineIndex--;
        }

        while (lastLineIndex + 1 < lines.Count
               && lines[lastLineIndex + 1].EndIncludingLineBreak > lines[lastLineIndex + 1].End
               && FormattingTextAnalysisUtilities.IsBlankLine(sourceText, lastLineIndex + 1))
        {
            lastLineIndex++;
        }

        return TextSpan.FromBounds(lines[firstLineIndex].Start, lines[lastLineIndex].EndIncludingLineBreak);
    }

    #endregion // WhitespaceSpanRemovalCodeFixProviderBase
}