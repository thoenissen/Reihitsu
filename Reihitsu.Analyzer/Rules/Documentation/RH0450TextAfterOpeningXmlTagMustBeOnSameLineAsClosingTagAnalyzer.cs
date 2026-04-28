using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0450: Text after opening XML tag must be on same line as closing tag
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer : DiagnosticAnalyzerBase<RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0450";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0450Title), nameof(AnalyzerResources.RH0450MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Attempts to get the position of the first meaningful content within the XML element
    /// </summary>
    /// <param name="element">XML element</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="position">Position of the first meaningful content</param>
    /// <returns><see langword="true"/> if meaningful content was found</returns>
    private static bool TryGetFirstMeaningfulContentPosition(XmlElementSyntax element, CancellationToken cancellationToken, out int position)
    {
        position = 0;

        foreach (var node in element.Content)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (node is XmlTextSyntax textSyntax)
            {
                if (TryGetFirstMeaningfulContentPosition(textSyntax, cancellationToken, out position))
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
    /// Determines whether the specified element violates the rule
    /// </summary>
    /// <param name="element">XML element</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the element violates the rule</returns>
    private static bool ViolatesRule(XmlElementSyntax element, CancellationToken cancellationToken)
    {
        if (TryGetFirstMeaningfulContentPosition(element, cancellationToken, out var firstContentPosition) == false)
        {
            return false;
        }

        var sourceText = element.SyntaxTree.GetText(cancellationToken);
        var startTagLine = sourceText.Lines.GetLineFromPosition(element.StartTag.Span.Start).LineNumber;
        var firstContentLine = sourceText.Lines.GetLineFromPosition(firstContentPosition).LineNumber;
        var endTagLine = sourceText.Lines.GetLineFromPosition(element.EndTag.Span.Start).LineNumber;

        return startTagLine == firstContentLine
               && firstContentLine != endTagLine;
    }

    /// <summary>
    /// Attempts to get the position of the first meaningful content within the XML text node
    /// </summary>
    /// <param name="textSyntax">XML text syntax</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="position">Position of the first meaningful content</param>
    /// <returns><see langword="true"/> if meaningful content was found</returns>
    private static bool TryGetFirstMeaningfulContentPosition(XmlTextSyntax textSyntax, CancellationToken cancellationToken, out int position)
    {
        position = 0;

        foreach (var token in textSyntax.TextTokens)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

    /// <summary>
    /// Analyzes an XML documentation element
    /// </summary>
    /// <param name="context">Context</param>
    private void OnXmlElement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not XmlElementSyntax element
            || ViolatesRule(element, context.CancellationToken) == false)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(element.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnXmlElement, SyntaxKind.XmlElement);
    }

    #endregion // DiagnosticAnalyzer
}