using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Formatter.Pipeline.DocumentationComments;

/// <summary>
/// Normalizes XML documentation comments to repository-specific layout rules
/// </summary>
internal static class DocumentationCommentFormattingPhase
{
    #region Methods

    /// <summary>
    /// Applies XML documentation comment formatting to the given syntax node
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

            var updatedCommentText = NormalizeDocumentationComment(trivia, documentationComment, sourceText, cancellationToken);

            if (updatedCommentText == null)
            {
                continue;
            }
            var leadingTrivia = SyntaxFactory.ParseLeadingTrivia(updatedCommentText);

            if (leadingTrivia.Count > 0)
            {
                replacements[trivia] = leadingTrivia[0];
            }
        }

        return replacements.Count == 0 ? root : root.ReplaceTrivia(replacements.Keys, (oldTrivia, _) => replacements[oldTrivia]);
    }

    /// <summary>
    /// Builds a single-line form of the XML documentation element
    /// </summary>
    /// <param name="element">XML element</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="documentationPrefix">Documentation prefix for continuation lines</param>
    /// <returns>The normalized single-line element text</returns>
    private static string BuildCollapsedElement(XmlElementSyntax element, SourceText sourceText, string documentationPrefix)
    {
        var contentLines = GetElementContentLines(element, sourceText, documentationPrefix);

        return sourceText.ToString(element.StartTag.Span) + contentLines[0] + sourceText.ToString(element.EndTag.Span);
    }

    /// <summary>
    /// Builds a multi-line form of the XML documentation element
    /// </summary>
    /// <param name="element">XML element</param>
    /// <param name="sourceText">Source text</param>
    /// <returns>The normalized multi-line element text</returns>
    private static string BuildExpandedElement(XmlElementSyntax element, SourceText sourceText)
    {
        var documentationPrefix = GetDocumentationPrefix(sourceText, sourceText.Lines.GetLineFromPosition(element.StartTag.Span.Start));
        var lineBreak = GetLineBreak(sourceText, sourceText.Lines.GetLineFromPosition(element.StartTag.Span.Start));
        var contentLines = GetElementContentLines(element, sourceText, documentationPrefix);

        return $"{sourceText.ToString(element.StartTag.Span)}{lineBreak}{documentationPrefix}{string.Join(lineBreak + documentationPrefix, contentLines)}{lineBreak}{documentationPrefix}{sourceText.ToString(element.EndTag.Span)}";
    }

    /// <summary>
    /// Determines whether the specified element can be collapsed to a single line
    /// </summary>
    /// <param name="element">XML element</param>
    /// <param name="sourceText">Source text</param>
    /// <returns><see langword="true"/> if the element can be collapsed</returns>
    private static bool CanCollapseToSingleLine(XmlElementSyntax element, SourceText sourceText)
    {
        if (IsSummaryElement(element))
        {
            return false;
        }

        var documentationPrefix = GetDocumentationPrefix(sourceText, sourceText.Lines.GetLineFromPosition(element.StartTag.Span.Start));
        var contentLines = GetElementContentLines(element, sourceText, documentationPrefix);

        return contentLines.Count == 1;
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
    /// Extracts normalized element content lines while preserving inline XML content
    /// </summary>
    /// <param name="element">XML element</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="documentationPrefix">Documentation prefix for continuation lines</param>
    /// <returns>The normalized content lines</returns>
    private static List<string> GetElementContentLines(XmlElementSyntax element, SourceText sourceText, string documentationPrefix)
    {
        var contentSpan = TextSpan.FromBounds(element.StartTag.Span.End, element.EndTag.Span.Start);
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
    /// Determines whether the specified element is a summary element
    /// </summary>
    /// <param name="element">Element</param>
    /// <returns><see langword="true"/> if the element is a summary element</returns>
    private static bool IsSummaryElement(XmlElementSyntax element)
    {
        return string.Equals(element.StartTag.Name.LocalName.ValueText, "summary", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the specified element requires line-alignment normalization
    /// </summary>
    /// <param name="element">Element</param>
    /// <param name="sourceText">Source text</param>
    /// <returns><see langword="true"/> if the element requires normalization</returns>
    private static bool NeedsLineAlignmentNormalization(XmlElementSyntax element, SourceText sourceText)
    {
        if (TryGetFirstMeaningfulContentPosition(element, out var firstContentPosition) == false)
        {
            return false;
        }

        var startTagLine = sourceText.Lines.GetLineFromPosition(element.StartTag.Span.Start).LineNumber;
        var firstContentLine = sourceText.Lines.GetLineFromPosition(firstContentPosition).LineNumber;
        var endTagLine = sourceText.Lines.GetLineFromPosition(element.EndTag.Span.Start).LineNumber;

        return startTagLine == firstContentLine
               && firstContentLine != endTagLine;
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

    /// <summary>
    /// Normalizes a documentation comment if any supported XML element requires it
    /// </summary>
    /// <param name="documentationCommentTrivia">Documentation comment trivia</param>
    /// <param name="documentationComment">Structured documentation comment</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The normalized comment text, or <see langword="null"/> if no change is required</returns>
    private static string NormalizeDocumentationComment(SyntaxTrivia documentationCommentTrivia, DocumentationCommentTriviaSyntax documentationComment, SourceText sourceText, CancellationToken cancellationToken)
    {
        var candidates = documentationComment.DescendantNodes()
                                             .OfType<XmlElementSyntax>()
                                             .Where(obj => RequiresNormalization(obj, sourceText))
                                             .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        var candidateSet = new HashSet<XmlElementSyntax>(candidates);
        var normalizedCommentText = sourceText.ToString(documentationCommentTrivia.FullSpan);
        var topLevelCandidates = candidates.Where(obj => obj.Ancestors().OfType<XmlElementSyntax>().Any(candidateSet.Contains) == false)
                                           .OrderByDescending(obj => obj.Span.Start)
                                           .ToList();

        foreach (var element in topLevelCandidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var replacementText = CanCollapseToSingleLine(element, sourceText)
                                      ? BuildCollapsedElement(element, sourceText, GetDocumentationPrefix(sourceText, sourceText.Lines.GetLineFromPosition(element.StartTag.Span.Start)))
                                      : BuildExpandedElement(element, sourceText);
            var relativeStart = element.Span.Start - documentationCommentTrivia.FullSpan.Start;
            var relativeEnd = element.Span.End - documentationCommentTrivia.FullSpan.Start;

            normalizedCommentText = normalizedCommentText.Substring(0, relativeStart) + replacementText + normalizedCommentText.Substring(relativeEnd);
        }

        return normalizedCommentText;
    }

    /// <summary>
    /// Determines whether the specified element requires normalization
    /// </summary>
    /// <param name="element">Element</param>
    /// <param name="sourceText">Source text</param>
    /// <returns><see langword="true"/> if the element requires normalization</returns>
    private static bool RequiresNormalization(XmlElementSyntax element, SourceText sourceText)
    {
        return (IsSummaryElement(element) && SpansAtLeastThreeLines(element, sourceText) == false)
               || NeedsLineAlignmentNormalization(element, sourceText);
    }

    /// <summary>
    /// Attempts to get the position of the first meaningful content within the XML element
    /// </summary>
    /// <param name="element">Element</param>
    /// <param name="position">Position of the first meaningful content</param>
    /// <returns><see langword="true"/> if meaningful content was found</returns>
    private static bool TryGetFirstMeaningfulContentPosition(XmlElementSyntax element, out int position)
    {
        position = 0;

        foreach (var node in element.Content)
        {
            if (node is XmlTextSyntax textSyntax)
            {
                if (TryGetFirstMeaningfulContentPosition(textSyntax, out position))
                {
                    return true;
                }

                continue;
            }

            position = node.SpanStart;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to get the position of the first meaningful content within the XML text node
    /// </summary>
    /// <param name="textSyntax">XML text syntax</param>
    /// <param name="position">Position of the first meaningful content</param>
    /// <returns><see langword="true"/> if meaningful content was found</returns>
    private static bool TryGetFirstMeaningfulContentPosition(XmlTextSyntax textSyntax, out int position)
    {
        position = 0;

        foreach (var token in textSyntax.TextTokens)
        {
            var tokenText = token.Text;

            for (var index = 0; index < tokenText.Length; index++)
            {
                if (char.IsWhiteSpace(tokenText[index]) == false)
                {
                    position = token.SpanStart + index;

                    return true;
                }
            }
        }

        return false;
    }

    #endregion // Methods
}