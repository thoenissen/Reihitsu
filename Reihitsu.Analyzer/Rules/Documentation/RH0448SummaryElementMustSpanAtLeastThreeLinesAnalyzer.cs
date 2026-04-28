using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0448: Summary element must span at least three lines
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0448SummaryElementMustSpanAtLeastThreeLinesAnalyzer : DiagnosticAnalyzerBase<RH0448SummaryElementMustSpanAtLeastThreeLinesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0448";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0448SummaryElementMustSpanAtLeastThreeLinesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0448Title), nameof(AnalyzerResources.RH0448MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a single-line documentation comment
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDocumentationCommentTrivia(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not DocumentationCommentTriviaSyntax documentationComment)
        {
            return;
        }

        var summaryElement = documentationComment.Content
                                                 .OfType<XmlElementSyntax>()
                                                 .FirstOrDefault(element => element.StartTag.Name.LocalName.ValueText == "summary");

        if (summaryElement == null)
        {
            return;
        }

        var sourceText = context.Node.SyntaxTree.GetText(context.CancellationToken);
        var startTagSpan = summaryElement.StartTag.Span;
        var endTagSpan = summaryElement.EndTag.Span;

        var startTagLine = sourceText.Lines.GetLineFromPosition(startTagSpan.Start).LineNumber;
        var endTagLine = sourceText.Lines.GetLineFromPosition(endTagSpan.Start).LineNumber;

        if (endTagLine - startTagLine < 2)
        {
            context.ReportDiagnostic(CreateDiagnostic(summaryElement.GetLocation()));
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