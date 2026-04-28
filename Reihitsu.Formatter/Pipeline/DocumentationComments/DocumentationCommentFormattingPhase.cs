using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Formatter.Pipeline.DocumentationComments;

/// <summary>
/// Normalizes XML documentation comments to repository-specific summary formatting
/// </summary>
internal static class DocumentationCommentFormattingPhase
{
    #region Methods

    /// <summary>
    /// Applies XML documentation summary formatting to the given syntax node
    /// </summary>
    /// <param name="root">The syntax node to format</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted syntax node</returns>
    public static SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var sourceText = root.SyntaxTree.GetText(cancellationToken);
        var replacements = new Dictionary<SyntaxTrivia, SyntaxTrivia>();

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) == false
                || trivia.GetStructure() is not DocumentationCommentTriviaSyntax documentationComment)
            {
                continue;
            }

            var summaryElement = documentationComment.Content
                                                     .OfType<XmlElementSyntax>()
                                                     .FirstOrDefault(static obj => string.Equals(obj.StartTag.Name.LocalName.ValueText, "summary", StringComparison.OrdinalIgnoreCase));

            if (summaryElement == null || SpansAtLeastThreeLines(summaryElement, sourceText))
            {
                continue;
            }

            var updatedCommentText = ExpandSummaryElement(trivia, summaryElement, sourceText);
            var leadingTrivia = SyntaxFactory.ParseLeadingTrivia(updatedCommentText);

            if (leadingTrivia.Count > 0)
            {
                replacements[trivia] = leadingTrivia[0];
            }
        }

        return replacements.Count == 0 ? root : root.ReplaceTrivia(replacements.Keys, (oldTrivia, _) => replacements[oldTrivia]);
    }

    /// <summary>
    /// Expands a summary element to the repository's three-line form
    /// </summary>
    /// <param name="documentationCommentTrivia">Documentation comment trivia</param>
    /// <param name="summaryElement">Summary element</param>
    /// <param name="sourceText">Source text</param>
    /// <returns>The updated documentation comment text</returns>
    private static string ExpandSummaryElement(SyntaxTrivia documentationCommentTrivia, XmlElementSyntax summaryElement, SourceText sourceText)
    {
        var documentationPrefix = GetDocumentationPrefix(sourceText, sourceText.Lines.GetLineFromPosition(summaryElement.StartTag.Span.Start));
        var lineBreak = GetLineBreak(sourceText, sourceText.Lines.GetLineFromPosition(summaryElement.StartTag.Span.Start));
        var contentLines = GetSummaryContentLines(summaryElement, sourceText, documentationPrefix);
        var expandedSummary = $"<summary>{lineBreak}{documentationPrefix}{string.Join(lineBreak + documentationPrefix, contentLines)}{lineBreak}{documentationPrefix}</summary>";
        var commentText = sourceText.ToString(documentationCommentTrivia.FullSpan);
        var relativeStart = summaryElement.Span.Start - documentationCommentTrivia.FullSpan.Start;
        var relativeEnd = summaryElement.Span.End - documentationCommentTrivia.FullSpan.Start;

        return commentText.Substring(0, relativeStart) + expandedSummary + commentText.Substring(relativeEnd);
    }

    /// <summary>
    /// Gets the line break sequence for the affected line
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="line">Affected line</param>
    /// <returns>The line break sequence</returns>
    private static string GetLineBreak(SourceText sourceText, TextLine line)
    {
        return line.EndIncludingLineBreak > line.End
                   ? sourceText.ToString(TextSpan.FromBounds(line.End, line.EndIncludingLineBreak))
                   : Environment.NewLine;
    }

    /// <summary>
    /// Gets the documentation prefix for the specified line
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="line">Affected line</param>
    /// <returns>The documentation prefix</returns>
    private static string GetDocumentationPrefix(SourceText sourceText, TextLine line)
    {
        var lineText = sourceText.ToString(line.Span);
        var elementIndex = lineText.IndexOf('<');

        return elementIndex >= 0 ? lineText.Substring(0, elementIndex) : string.Empty;
    }

    /// <summary>
    /// Extracts normalized summary content lines while preserving inline XML content
    /// </summary>
    /// <param name="summaryElement">Summary element</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="documentationPrefix">Documentation prefix for continuation lines</param>
    /// <returns>The normalized content lines</returns>
    private static List<string> GetSummaryContentLines(XmlElementSyntax summaryElement, SourceText sourceText, string documentationPrefix)
    {
        var contentSpan = TextSpan.FromBounds(summaryElement.StartTag.Span.End, summaryElement.EndTag.Span.Start);
        var rawContent = sourceText.ToString(contentSpan);
        var rawLines = rawContent.Replace("\r\n", "\n")
                                 .Replace('\r', '\n')
                                 .Split('\n');
        var contentLines = new List<string>();

        foreach (var rawLine in rawLines)
        {
            var currentLine = rawLine;

            if (currentLine.StartsWith(documentationPrefix, StringComparison.Ordinal))
            {
                currentLine = currentLine.Substring(documentationPrefix.Length);
            }
            else
            {
                var trimmedStart = currentLine.TrimStart(' ', '\t');

                if (trimmedStart.StartsWith("///", StringComparison.Ordinal))
                {
                    currentLine = trimmedStart.Substring(3).TrimStart();
                }
            }

            currentLine = currentLine.Trim();

            if (string.IsNullOrWhiteSpace(currentLine) == false)
            {
                contentLines.Add(currentLine);
            }
        }

        return contentLines.Count == 0 ? [string.Empty] : contentLines;
    }

    /// <summary>
    /// Determines whether the specified summary element already spans at least three lines
    /// </summary>
    /// <param name="summaryElement">Summary element</param>
    /// <param name="sourceText">Source text</param>
    /// <returns><see langword="true"/> if the summary spans at least three lines</returns>
    private static bool SpansAtLeastThreeLines(XmlElementSyntax summaryElement, SourceText sourceText)
    {
        var startTagLine = sourceText.Lines.GetLineFromPosition(summaryElement.StartTag.Span.Start).LineNumber;
        var endTagLine = sourceText.Lines.GetLineFromPosition(summaryElement.EndTag.Span.Start).LineNumber;

        return endTagLine - startTagLine >= 2;
    }

    #endregion // Methods
}