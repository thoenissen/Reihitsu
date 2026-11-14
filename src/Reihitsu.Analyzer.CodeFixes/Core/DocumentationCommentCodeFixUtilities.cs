using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Shared helpers for documentation-comment code fixes which rewrite line breaks or remove elements
/// </summary>
internal static class DocumentationCommentCodeFixUtilities
{
    #region Methods

    /// <summary>
    /// Moves the diagnostic content to the next documentation comment line. The line break in front of it is the
    /// document's line ending, not the line break of the affected line (SHARED-FIX-008)
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
        var insertionText = ReihitsuFormatterHelpers.DetectEndOfLine(root) + DocumentationCommentUtilities.GetContinuationPrefix(sourceText, affectedLine);

        return document.WithText(sourceText.Replace(replacementSpan, insertionText));
    }

    /// <summary>
    /// Lays out the reported documentation element as the formatter lays it out, and changes nothing else: the other
    /// lines and elements of the comment, the other comments, and the declaration's code stay as they are written
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Span of the reported element</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    /// <remarks>
    /// The layout, the column of the lines the element gains included, is the formatter's alone:
    /// <see cref="ReihitsuFormatter.FormatDocumentationElementInDocumentAsync"/> lays out the element where it stands.
    /// Formatting the declaration instead would lay out its code and its other comments too, and would not reach the
    /// comment of a declaration directly in the compilation unit, whose leading trivia the node-level entry point keeps
    /// as written
    /// </remarks>
    internal static async Task<Document> LayOutElementAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || TryGetElementToLayOut(root, diagnosticSpan, out var element) == false)
        {
            return document;
        }

        return await ReihitsuFormatter.FormatDocumentationElementInDocumentAsync(document, element, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether the reported documentation element can be laid out by <see cref="LayOutElementAsync"/>: it is
    /// closed by an end tag of its own, and its documentation comment starts its own line instead of following code
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="diagnosticSpan">Span of the reported element</param>
    /// <returns><see langword="true"/> if the element can be laid out</returns>
    /// <remarks>
    /// A comment written behind code stays there, because the fix moves neither the declaration nor the code in front of
    /// it, and laying the element out there would put documentation lines behind code, which the formatter moves again
    /// </remarks>
    internal static bool CanLayOutElement(SyntaxNode root, SourceText sourceText, TextSpan diagnosticSpan)
    {
        return TryGetElementToLayOut(root, diagnosticSpan, out var element)
               && element.FirstAncestorOrSelf<DocumentationCommentTriviaSyntax>() is { } documentationComment
               && DocumentationCommentUtilities.IsWrittenBehindSource(documentationComment.ParentTrivia, sourceText) == false;
    }

    /// <summary>
    /// Determines whether the documentation element at the specified span can be removed or unwrapped safely
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="elementSpan">Span of the documentation element</param>
    /// <returns><see langword="true"/> if the element is empty or closed by an end tag of its own</returns>
    /// <remarks>
    /// An element without an end tag of its own is left alone. Roslyn extends an element without an end tag to the end
    /// of the documentation comment, so its span reaches past the trailing line break and can cover unrelated tags and
    /// the declaration which follows, and it closes an element with an end tag of another name, which belongs to the
    /// element around it. Removing or unwrapping that span would move source code into the comment, drop neighboring
    /// documentation, or delete the end tag of the enclosing element
    /// </remarks>
    internal static bool CanRewriteElement(SyntaxNode root, TextSpan elementSpan)
    {
        var node = root.FindNode(elementSpan, findInsideTrivia: true, getInnermostNodeForTie: true);

        return node is not XmlElementSyntax element
               || DocumentationCommentUtilities.HasOwnEndTag(element);
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
    /// Gets the reported documentation element, when it is an element closed by an end tag of its own
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">Span of the reported element</param>
    /// <param name="element">The element</param>
    /// <returns><see langword="true"/> if the element was found</returns>
    private static bool TryGetElementToLayOut(SyntaxNode root, TextSpan diagnosticSpan, out XmlElementSyntax element)
    {
        element = root.FindNode(diagnosticSpan, findInsideTrivia: true, getInnermostNodeForTie: true) as XmlElementSyntax;

        return element != null
               && element.Span == diagnosticSpan
               && DocumentationCommentUtilities.HasOwnEndTag(element);
    }

    #endregion // Methods
}