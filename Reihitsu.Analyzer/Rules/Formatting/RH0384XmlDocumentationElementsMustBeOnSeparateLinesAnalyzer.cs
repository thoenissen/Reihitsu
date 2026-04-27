using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0384: XML documentation elements must be on separate lines.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0384XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer : DiagnosticAnalyzerBase<RH0384XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0384";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0384XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0384Title), nameof(AnalyzerResources.RH0384MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the specified XML text node contains non-whitespace text.
    /// </summary>
    /// <param name="textSyntax">XML text syntax</param>
    /// <returns><see langword="true"/> if the text node contains non-whitespace text</returns>
    private static bool ContainsMeaningfulText(XmlTextSyntax textSyntax)
    {
        return textSyntax.TextTokens.Any(obj => string.IsNullOrWhiteSpace(obj.ValueText) == false);
    }

    /// <summary>
    /// Determines whether the specified node is an XML documentation element.
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if the node is an element</returns>
    private static bool IsElement(XmlNodeSyntax node)
    {
        return node is XmlElementSyntax or XmlEmptyElementSyntax;
    }

    /// <summary>
    /// Determines whether two XML nodes occupy the same source line.
    /// </summary>
    /// <param name="previousNode">Previous node</param>
    /// <param name="currentNode">Current node</param>
    /// <param name="sourceText">Source text</param>
    /// <returns><see langword="true"/> if the nodes share a line</returns>
    private static bool SharesLine(XmlNodeSyntax previousNode, XmlNodeSyntax currentNode, SourceText sourceText)
    {
        var previousEnd = previousNode.Span.End == previousNode.Span.Start ? previousNode.Span.End : previousNode.Span.End - 1;
        var previousEndLine = sourceText.Lines.GetLineFromPosition(previousEnd).LineNumber;
        var currentStartLine = sourceText.Lines.GetLineFromPosition(currentNode.Span.Start).LineNumber;

        return previousEndLine == currentStartLine;
    }

    /// <summary>
    /// Analyzes a single-line documentation comment.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDocumentationCommentTrivia(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not DocumentationCommentTriviaSyntax documentationComment)
        {
            return;
        }

        var sourceText = context.Node.SyntaxTree.GetText(context.CancellationToken);
        XmlNodeSyntax previousElement = null;

        foreach (var node in documentationComment.Content)
        {
            if (node is XmlTextSyntax textSyntax)
            {
                if (ContainsMeaningfulText(textSyntax))
                {
                    previousElement = null;
                }

                continue;
            }

            if (IsElement(node) == false)
            {
                previousElement = null;

                continue;
            }

            if (previousElement != null
                && SharesLine(previousElement, node, sourceText))
            {
                context.ReportDiagnostic(CreateDiagnostic(node.GetLocation()));
            }

            previousElement = node;
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDocumentationCommentTrivia, SyntaxKind.SingleLineDocumentationCommentTrivia);
    }

    #endregion // DiagnosticAnalyzer
}