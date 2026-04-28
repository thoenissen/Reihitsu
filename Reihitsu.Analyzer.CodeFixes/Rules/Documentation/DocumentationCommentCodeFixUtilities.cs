using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// Shared helpers for documentation-comment line-break code fixes
/// </summary>
internal static class DocumentationCommentCodeFixUtilities
{
    #region Methods

    /// <summary>
    /// Moves the diagnostic content to the next documentation comment line
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    internal static async Task<Document> MoveContentToNextDocumentationLineAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var affectedLine = sourceText.Lines.GetLineFromPosition(diagnosticSpan.Start);
        var replacementStart = diagnosticSpan.Start;

        while (replacementStart > affectedLine.Start
               && IsHorizontalWhitespace(sourceText[replacementStart - 1]))
        {
            replacementStart--;
        }

        var replacementSpan = TextSpan.FromBounds(replacementStart, diagnosticSpan.Start);
        var insertionText = GetLineBreak(sourceText, affectedLine) + GetDocumentationPrefix(sourceText, affectedLine);

        return document.WithText(sourceText.Replace(replacementSpan, insertionText));
    }

    /// <summary>
    /// Gets the documentation prefix for the specified line
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="line">Line</param>
    /// <returns>Documentation prefix</returns>
    private static string GetDocumentationPrefix(SourceText sourceText, TextLine line)
    {
        var lineText = sourceText.ToString(line.Span);
        var elementIndex = lineText.IndexOf('<');

        return elementIndex >= 0 ? lineText.Substring(0, elementIndex) : string.Empty;
    }

    /// <summary>
    /// Gets the line break sequence for the affected line
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="line">Affected line</param>
    /// <returns>Line break sequence</returns>
    private static string GetLineBreak(SourceText sourceText, TextLine line)
    {
        return line.EndIncludingLineBreak > line.End
                   ? sourceText.ToString(TextSpan.FromBounds(line.End, line.EndIncludingLineBreak))
                   : Environment.NewLine;
    }

    /// <summary>
    /// Determines whether the specified character is horizontal whitespace
    /// </summary>
    /// <param name="value">Character to inspect</param>
    /// <returns><see langword="true"/> if the character is a space or tab</returns>
    private static bool IsHorizontalWhitespace(char value)
    {
        return value is ' ' or '\t';
    }

    #endregion // Methods
}