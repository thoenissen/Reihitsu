using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

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
    /// Removes the trailing comma while preserving attached comments and trivia
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

        return sourceText.Replace(commaToken.Span, string.Empty);
    }

    #endregion // Methods
}