using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.DocumentationComments;

/// <summary>
/// The element-building half of the documentation-comment phase. It decides whether a single XML
/// element needs normalizing and rebuilds it in collapsed or expanded form, preserving the inline
/// content. Locating candidates inside the comment and fixing the <c>///</c> line prefixes belongs to
/// <see cref="DocumentationCommentFormattingPhase"/>
/// </summary>
internal static class DocCommentElementNormalizer
{
    #region Methods

    /// <summary>
    /// Determines whether the specified element requires normalization
    /// </summary>
    /// <param name="element">Element</param>
    /// <param name="sourceText">Source text</param>
    /// <returns><see langword="true"/> if the element requires normalization</returns>
    public static bool RequiresNormalization(XmlElementSyntax element, SourceText sourceText)
    {
        return (IsSummaryElement(element) && SpansAtLeastThreeLines(element, sourceText) == false)
               || NeedsLineAlignmentNormalization(element, sourceText);
    }

    /// <summary>
    /// Builds the normalized text for the specified element, collapsing it to a single line when possible
    /// </summary>
    /// <param name="element">XML element</param>
    /// <param name="sourceText">Source text</param>
    /// <returns>The normalized element text</returns>
    public static string BuildReplacement(XmlElementSyntax element, SourceText sourceText)
    {
        return CanCollapseToSingleLine(element, sourceText)
                   ? BuildCollapsedElement(element, sourceText, DocumentationCommentUtilities.GetContinuationPrefix(sourceText, sourceText.Lines.GetLineFromPosition(element.StartTag.Span.Start)))
                   : BuildExpandedElement(element, sourceText);
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

        return $"{sourceText.ToString(element.StartTag.Span)}{contentLines[0]}{sourceText.ToString(element.EndTag.Span)}";
    }

    /// <summary>
    /// Builds a multi-line form of the XML documentation element
    /// </summary>
    /// <param name="element">XML element</param>
    /// <param name="sourceText">Source text</param>
    /// <returns>The normalized multi-line element text</returns>
    private static string BuildExpandedElement(XmlElementSyntax element, SourceText sourceText)
    {
        var documentationPrefix = DocumentationCommentUtilities.GetContinuationPrefix(sourceText, sourceText.Lines.GetLineFromPosition(element.StartTag.Span.Start));
        var lineBreak = GetLineBreak(sourceText, sourceText.Lines.GetLineFromPosition(element.StartTag.Span.Start));
        var contentLines = GetElementContentLines(element, sourceText, documentationPrefix);
        var formattedContentLines = contentLines.Select(obj => string.IsNullOrWhiteSpace(obj) && contentLines.Count > 1
                                                                   ? documentationPrefix.TrimEnd()
                                                                   : $"{documentationPrefix}{obj}");

        return $"{sourceText.ToString(element.StartTag.Span)}{lineBreak}{string.Join(lineBreak, formattedContentLines)}{lineBreak}{documentationPrefix}{sourceText.ToString(element.EndTag.Span)}";
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

        var documentationPrefix = DocumentationCommentUtilities.GetContinuationPrefix(sourceText, sourceText.Lines.GetLineFromPosition(element.StartTag.Span.Start));
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
    /// Extracts normalized element content lines while preserving inline XML content and the
    /// significant indentation found inside elements such as <c>&lt;code&gt;</c>
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
            var currentLine = StripDocumentationPrefix(rawLine, documentationPrefix);

            contentLines.Add(currentLine);
        }

        while (contentLines.Count > 0 && string.IsNullOrWhiteSpace(contentLines[0]))
        {
            contentLines.RemoveAt(0);
        }

        while (contentLines.Count > 0 && string.IsNullOrWhiteSpace(contentLines[contentLines.Count - 1]))
        {
            contentLines.RemoveAt(contentLines.Count - 1);
        }

        return contentLines.Count == 0 ? [string.Empty] : contentLines;
    }

    /// <summary>
    /// Removes the documentation exterior from a raw content line while preserving any indentation that
    /// follows it, so significant whitespace inside elements such as <c>&lt;code&gt;</c> survives
    /// </summary>
    /// <param name="rawLine">Raw content line</param>
    /// <param name="documentationPrefix">Documentation prefix for continuation lines</param>
    /// <returns>The content of the line with the exterior removed and trailing whitespace trimmed</returns>
    private static string StripDocumentationPrefix(string rawLine, string documentationPrefix)
    {
        if (rawLine.StartsWith(documentationPrefix, StringComparison.Ordinal))
        {
            return rawLine.Substring(documentationPrefix.Length).TrimEnd();
        }

        var trimmedStart = rawLine.TrimStart(' ', '\t');

        if (trimmedStart.StartsWith("///", StringComparison.Ordinal))
        {
            var afterExterior = trimmedStart.Substring(3);

            return (afterExterior.StartsWith(" ", StringComparison.Ordinal) ? afterExterior.Substring(1) : afterExterior).TrimEnd();
        }

        return rawLine.Trim();
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