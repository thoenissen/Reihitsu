using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Shared helpers for documentation-comment code fixes which rewrite line breaks or remove elements
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
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var affectedLine = sourceText.Lines.GetLineFromPosition(diagnosticSpan.Start);
        var replacementStart = diagnosticSpan.Start;

        while (replacementStart > affectedLine.Start
               && IsHorizontalWhitespace(sourceText[replacementStart - 1]))
        {
            replacementStart--;
        }

        var replacementSpan = TextSpan.FromBounds(replacementStart, diagnosticSpan.Start);
        var insertionText = GetLineBreak(sourceText, affectedLine, root) + DocumentationCommentUtilities.GetContinuationPrefix(sourceText, affectedLine);

        return document.WithText(sourceText.Replace(replacementSpan, insertionText));
    }

    /// <summary>
    /// Determines whether the documentation element at the specified span can be removed safely
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="elementSpan">Span of the documentation element</param>
    /// <returns><see langword="true"/> if the element is well formed enough to be removed</returns>
    /// <remarks>
    /// An element without an end tag is left alone. Roslyn extends such an element to the end of the documentation
    /// comment, so its span reaches past the trailing line break and can cover unrelated tags and the declaration
    /// which follows. Removing that span would move source code into the comment or drop neighboring documentation
    /// </remarks>
    internal static bool CanRemoveElement(SyntaxNode root, TextSpan elementSpan)
    {
        var node = root.FindNode(elementSpan, findInsideTrivia: true, getInnermostNodeForTie: true);

        return node is not XmlElementSyntax element
               || element.EndTag.IsMissing == false;
    }

    /// <summary>
    /// Gets the span which has to be removed to delete a documentation element without touching co-located content
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="elementSpan">Span of the documentation element</param>
    /// <returns>The removal span</returns>
    /// <remarks>
    /// Lines which carry nothing but the element are removed completely. As soon as another tag or documentation text
    /// shares the first or the last line, only the element itself and its adjacent horizontal whitespace are removed.
    /// The result is always contained in the lines the element occupies, so no trivia outside those lines is touched
    /// </remarks>
    internal static TextSpan GetElementRemovalSpan(SourceText sourceText, TextSpan elementSpan)
    {
        var startLine = sourceText.Lines.GetLineFromPosition(elementSpan.Start);
        var endLine = sourceText.Lines.GetLineFromPosition(elementSpan.End);
        var isLineStartOwnedByElement = DocumentationCommentUtilities.IsExteriorOnly(sourceText.ToString(TextSpan.FromBounds(startLine.Start, elementSpan.Start)));
        var isLineEndOwnedByElement = string.IsNullOrWhiteSpace(sourceText.ToString(TextSpan.FromBounds(elementSpan.End, endLine.End)));

        if (isLineStartOwnedByElement
            && isLineEndOwnedByElement)
        {
            return TextSpan.FromBounds(startLine.Start, endLine.EndIncludingLineBreak);
        }

        var start = elementSpan.Start;
        var end = elementSpan.End;

        if (isLineEndOwnedByElement)
        {
            end = endLine.End;
        }
        else if (isLineStartOwnedByElement)
        {
            while (end < endLine.End
                   && IsHorizontalWhitespace(sourceText[end]))
            {
                end++;
            }
        }

        if (isLineStartOwnedByElement == false)
        {
            while (start > startLine.Start
                   && IsHorizontalWhitespace(sourceText[start - 1]))
            {
                start--;
            }
        }

        return TextSpan.FromBounds(start, end);
    }

    /// <summary>
    /// Determines whether the specified character is horizontal whitespace
    /// </summary>
    /// <param name="value">Character to inspect</param>
    /// <returns><see langword="true"/> if the character is a space or tab</returns>
    internal static bool IsHorizontalWhitespace(char value)
    {
        return value is ' ' or '\t';
    }

    /// <summary>
    /// Gets the line break sequence for the affected line
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="line">Affected line</param>
    /// <param name="root">Syntax root used to detect the document's end-of-line sequence when the line has no trailing line break</param>
    /// <returns>Line break sequence</returns>
    private static string GetLineBreak(SourceText sourceText, TextLine line, SyntaxNode root)
    {
        return line.EndIncludingLineBreak > line.End
                   ? sourceText.ToString(TextSpan.FromBounds(line.End, line.EndIncludingLineBreak))
                   : ReihitsuFormatterHelpers.DetectEndOfLine(root);
    }

    #endregion // Methods
}