using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Shared helper for trailing-comma code fixes
/// </summary>
internal static class TrailingCommaCodeFixHelper
{
    #region Methods

    /// <summary>
    /// Removes the trailing comma represented by the provided diagnostic span
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    public static async Task<Document> RemoveTrailingCommaAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var commaToken = root.FindToken(diagnosticSpan.Start);

        if (commaToken.IsKind(SyntaxKind.CommaToken) == false)
        {
            return document;
        }

        return document.WithText(RemoveTrailingComma(sourceText, commaToken));
    }

    /// <summary>
    /// Removes the trailing comma while preserving attached comments and trivia, and leaves the line as the formatter would
    /// leave it (SHARED-FIX-007): a line that held only the comma is removed; a comma at the end of its line takes the
    /// whitespace in front of it along, so no whitespace ends the line; and a comma that starts its line in front of the
    /// closing brace leaves that brace in the column of the line that holds the matching opening brace
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="commaToken">Comma token</param>
    /// <returns>The updated source text</returns>
    private static SourceText RemoveTrailingComma(SourceText sourceText, SyntaxToken commaToken)
    {
        var commaLine = sourceText.Lines.GetLineFromPosition(commaToken.SpanStart);

        if (commaLine.ToString().Trim() == ",")
        {
            return sourceText.Replace(TextSpan.FromBounds(commaLine.Start, commaLine.EndIncludingLineBreak), string.Empty);
        }

        var start = FormattingTextAnalysisUtilities.GetLeadingWhitespaceRunStart(sourceText, commaToken.SpanStart, commaLine.Start);
        var end = commaLine.End - sourceText.ToString(TextSpan.FromBounds(commaToken.Span.End, commaLine.End)).TrimStart(' ', '\t').Length;
        var nextToken = commaToken.GetNextToken();

        if (end == commaLine.End)
        {
            return sourceText.Replace(TextSpan.FromBounds(start, end), string.Empty);
        }

        if (start == commaLine.Start
            && nextToken.IsKind(SyntaxKind.CloseBraceToken)
            && nextToken.SpanStart == end
            && nextToken.Parent?.ChildTokens().FirstOrDefault(obj => obj.IsKind(SyntaxKind.OpenBraceToken)) is { RawKind: not 0 } openBrace)
        {
            var openBraceLine = sourceText.Lines.GetLineFromPosition(openBrace.SpanStart);

            return sourceText.Replace(TextSpan.FromBounds(start, end), FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, openBraceLine)));
        }

        return sourceText.Replace(commaToken.Span, string.Empty);
    }

    #endregion // Methods
}