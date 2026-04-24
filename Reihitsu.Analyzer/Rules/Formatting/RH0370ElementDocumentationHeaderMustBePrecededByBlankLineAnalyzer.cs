using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0370: Element documentation header must be preceded by blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0370ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer : DiagnosticAnalyzerBase<RH0370ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0370";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0370ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0370Title), nameof(AnalyzerResources.RH0370MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var sourceText = context.Tree.GetText(context.CancellationToken);

        for (var lineIndex = 1; lineIndex < sourceText.Lines.Count; lineIndex++)
        {
            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[lineIndex]).TrimStart();

            if (lineText.StartsWith("///", StringComparison.Ordinal) == false)
            {
                continue;
            }

            var previousLineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[lineIndex - 1]).TrimStart();

            if (previousLineText.StartsWith("///", StringComparison.Ordinal)
                || FormattingTextAnalysisUtilities.IsBlankLine(sourceText, lineIndex - 1))
            {
                continue;
            }

            var lineTextWithIndentation = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[lineIndex]);
            var documentationStart = lineTextWithIndentation.IndexOf("///", StringComparison.Ordinal);
            var diagnosticStart = sourceText.Lines[lineIndex].Start + documentationStart;

            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(diagnosticStart, diagnosticStart + 3))));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}