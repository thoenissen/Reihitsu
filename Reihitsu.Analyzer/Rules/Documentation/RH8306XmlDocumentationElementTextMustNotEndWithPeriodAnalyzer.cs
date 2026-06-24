using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8306: XML documentation element text must not end with a period
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8306XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer : DiagnosticAnalyzerBase<RH8306XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8306";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8306XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8306Title), nameof(AnalyzerResources.RH8306MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the specified XML text node contains non-whitespace text
    /// </summary>
    /// <param name="textSyntax">XML text syntax</param>
    /// <returns><see langword="true"/> if the text node contains non-whitespace text</returns>
    private static bool ContainsMeaningfulText(XmlTextSyntax textSyntax)
    {
        return textSyntax.TextTokens.Any(obj => string.IsNullOrWhiteSpace(obj.ValueText) == false);
    }

    /// <summary>
    /// Determines whether the specified character terminates a sentence
    /// </summary>
    /// <param name="value">Character</param>
    /// <returns><see langword="true"/> if the character is a sentence terminator</returns>
    private static bool IsSentenceTerminator(char value)
    {
        return value is '.' or '!' or '?';
    }

    /// <summary>
    /// Gets the visible text content of the XML element
    /// </summary>
    /// <param name="element">XML element</param>
    /// <returns>The concatenated text content with inline elements collapsed to a single separator</returns>
    private static string GetTextContent(XmlElementSyntax element)
    {
        var builder = new StringBuilder();

        foreach (var node in element.Content)
        {
            if (node is XmlTextSyntax textSyntax)
            {
                foreach (var token in textSyntax.TextTokens)
                {
                    builder.Append(token.Text);
                }
            }
            else
            {
                builder.Append(' ');
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Determines whether the XML element text consists of more than one sentence
    /// </summary>
    /// <param name="element">XML element</param>
    /// <returns><see langword="true"/> if the text contains an internal sentence terminator that starts a further sentence</returns>
    /// <remarks>
    /// The heuristic treats a sentence terminator that is followed by whitespace and an uppercase letter as the boundary
    /// between two sentences, so abbreviations and decimal numbers within a single sentence are not misclassified
    /// </remarks>
    private static bool ContainsMultipleSentences(XmlElementSyntax element)
    {
        var text = GetTextContent(element);

        for (var index = 0; index < text.Length; index++)
        {
            if (IsSentenceTerminator(text[index]) == false)
            {
                continue;
            }

            var nextIndex = index + 1;

            if (nextIndex >= text.Length
                || char.IsWhiteSpace(text[nextIndex]) == false)
            {
                continue;
            }

            while (nextIndex < text.Length
                   && char.IsWhiteSpace(text[nextIndex]))
            {
                nextIndex++;
            }

            if (nextIndex < text.Length
                && char.IsUpper(text[nextIndex]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified XML element is covered by the rule
    /// </summary>
    /// <param name="element">XML element</param>
    /// <returns><see langword="true"/> if the rule applies to the element</returns>
    private static bool IsSupportedElement(XmlElementSyntax element)
    {
        var tagName = DocumentationAnalysisUtilities.GetTagName(element);

        return string.Equals(tagName, "summary", StringComparison.OrdinalIgnoreCase)
               || string.Equals(tagName, "remarks", StringComparison.OrdinalIgnoreCase)
               || string.Equals(tagName, "returns", StringComparison.OrdinalIgnoreCase)
               || string.Equals(tagName, "value", StringComparison.OrdinalIgnoreCase)
               || string.Equals(tagName, "param", StringComparison.OrdinalIgnoreCase)
               || string.Equals(tagName, "typeparam", StringComparison.OrdinalIgnoreCase)
               || string.Equals(tagName, "exception", StringComparison.OrdinalIgnoreCase)
               || string.Equals(tagName, "permission", StringComparison.OrdinalIgnoreCase)
               || string.Equals(tagName, "example", StringComparison.OrdinalIgnoreCase)
               || string.Equals(tagName, "para", StringComparison.OrdinalIgnoreCase)
               || string.Equals(tagName, "term", StringComparison.OrdinalIgnoreCase)
               || string.Equals(tagName, "description", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Attempts to find a trailing period in the XML element text content
    /// </summary>
    /// <param name="element">XML element</param>
    /// <param name="span">Period span</param>
    /// <returns><see langword="true"/> if a trailing period was found</returns>
    private static bool TryGetTrailingPeriodSpan(XmlElementSyntax element, out TextSpan span)
    {
        span = default;

        for (var index = element.Content.Count - 1; index >= 0; index--)
        {
            var node = element.Content[index];

            if (node is XmlTextSyntax textSyntax)
            {
                if (TryGetTrailingPeriodSpan(textSyntax, out span))
                {
                    return true;
                }

                if (ContainsMeaningfulText(textSyntax))
                {
                    return false;
                }

                continue;
            }

            if (string.IsNullOrWhiteSpace(node.ToString()) == false)
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to find a trailing period in an XML text node
    /// </summary>
    /// <param name="textSyntax">XML text node</param>
    /// <param name="span">Period span</param>
    /// <returns><see langword="true"/> if a trailing period was found</returns>
    private static bool TryGetTrailingPeriodSpan(XmlTextSyntax textSyntax, out TextSpan span)
    {
        span = default;

        for (var index = textSyntax.TextTokens.Count - 1; index >= 0; index--)
        {
            var token = textSyntax.TextTokens[index];
            var tokenText = token.Text;
            var lastNonWhitespaceIndex = tokenText.Length - 1;

            while (lastNonWhitespaceIndex >= 0
                   && char.IsWhiteSpace(tokenText[lastNonWhitespaceIndex]))
            {
                lastNonWhitespaceIndex--;
            }

            if (lastNonWhitespaceIndex < 0)
            {
                continue;
            }

            if (tokenText[lastNonWhitespaceIndex] != '.')
            {
                return false;
            }

            span = TextSpan.FromBounds(token.SpanStart + lastNonWhitespaceIndex, token.SpanStart + lastNonWhitespaceIndex + 1);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Analyze an XML documentation element
    /// </summary>
    /// <param name="context">Context</param>
    private void OnXmlElement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not XmlElementSyntax element
            || IsSupportedElement(element) == false
            || TryGetTrailingPeriodSpan(element, out var span) == false
            || ContainsMultipleSentences(element))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(Location.Create(element.SyntaxTree, span)));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnXmlElement, SyntaxKind.XmlElement);
    }

    #endregion // DiagnosticAnalyzer
}