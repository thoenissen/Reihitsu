using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0451: No content should appear after closing XML tags
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0451NoContentShouldAppearAfterClosingXmlTagsAnalyzer : DiagnosticAnalyzerBase<RH0451NoContentShouldAppearAfterClosingXmlTagsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0451";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0451NoContentShouldAppearAfterClosingXmlTagsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0451Title), nameof(AnalyzerResources.RH0451MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the specified node is a direct XML documentation element
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if the node is an element</returns>
    private static bool IsDirectElement(XmlNodeSyntax node)
    {
        return node is XmlElementSyntax or XmlEmptyElementSyntax;
    }

    /// <summary>
    /// Determines whether two positions occupy the same source line
    /// </summary>
    /// <param name="previousNode">Previous node</param>
    /// <param name="currentPosition">Current position</param>
    /// <param name="sourceText">Source text</param>
    /// <returns><see langword="true"/> if both positions share a line</returns>
    private static bool SharesLine(XmlNodeSyntax previousNode, int currentPosition, SourceText sourceText)
    {
        var previousEnd = previousNode.Span.End == previousNode.Span.Start ? previousNode.Span.End : previousNode.Span.End - 1;
        var previousEndLine = sourceText.Lines.GetLineFromPosition(previousEnd).LineNumber;
        var currentLine = sourceText.Lines.GetLineFromPosition(currentPosition).LineNumber;

        return previousEndLine == currentLine;
    }

    /// <summary>
    /// Attempts to get the first meaningful text span inside the XML text node
    /// </summary>
    /// <param name="textSyntax">XML text syntax</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="span">Span of the first meaningful text</param>
    /// <returns><see langword="true"/> if meaningful text was found</returns>
    private static bool TryGetFirstMeaningfulTextSpan(XmlTextSyntax textSyntax, CancellationToken cancellationToken, out TextSpan span)
    {
        span = default;

        foreach (var token in textSyntax.TextTokens)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tokenText = token.Text;

            for (var index = 0; index < tokenText.Length; index++)
            {
                if (char.IsWhiteSpace(tokenText[index]) == false)
                {
                    span = TextSpan.FromBounds(token.SpanStart + index, token.Span.End);

                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Analyzes a documentation comment
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
            context.CancellationToken.ThrowIfCancellationRequested();

            if (node is XmlTextSyntax textSyntax)
            {
                if (TryGetFirstMeaningfulTextSpan(textSyntax, context.CancellationToken, out var span))
                {
                    if (previousElement != null
                        && SharesLine(previousElement, span.Start, sourceText))
                    {
                        context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Node.SyntaxTree, span)));
                    }

                    previousElement = null;
                }

                continue;
            }

            previousElement = IsDirectElement(node) ? node : null;
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