using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8305: Summary element must span at least three lines
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8305";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8305Title), nameof(AnalyzerResources.RH8305MessageFormat))
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
        if (context.Node is not DocumentationCommentTriviaSyntax documentationComment
            || DocumentationAnalysisUtilities.IsPrimaryConstructorRun(context))
        {
            return;
        }

        // Every top-level summary is judged on its own, whatever its position among the others
        var summaryElements = documentationComment.Content
                                                  .OfType<XmlElementSyntax>()
                                                  .Where(element => element.StartTag.Name.LocalName.ValueText == "summary")
                                                  .ToList();

        if (summaryElements.Count == 0)
        {
            return;
        }

        var sourceText = context.Node.SyntaxTree.GetText(context.CancellationToken);

        // The formatter keeps a comment stranded behind code, or one with an element without an end tag of its own, as
        // written, so the fix would change nothing and the diagnostic would stay for good
        if (DocumentationCommentUtilities.IsLeftAsWritten(documentationComment.ParentTrivia, sourceText))
        {
            return;
        }

        foreach (var summaryElement in summaryElements)
        {
            var startTagLine = sourceText.Lines.GetLineFromPosition(summaryElement.StartTag.Span.Start).LineNumber;
            var endTagLine = sourceText.Lines.GetLineFromPosition(summaryElement.EndTag.Span.Start).LineNumber;

            if (endTagLine - startTagLine < 2)
            {
                context.ReportDiagnostic(CreateDiagnostic(summaryElement.GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDocumentationCommentTrivia, SyntaxKind.SingleLineDocumentationCommentTrivia);
    }

    #endregion // DiagnosticAnalyzer
}