using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0371: Code must not contain blank lines at the start of the file
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0371CodeMustNotContainBlankLinesAtStartOfFileAnalyzer : DiagnosticAnalyzerBase<RH0371CodeMustNotContainBlankLinesAtStartOfFileAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0371";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0371CodeMustNotContainBlankLinesAtStartOfFileAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0371Title), nameof(AnalyzerResources.RH0371MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree for leading blank lines
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var sourceText = context.Tree.GetText(context.CancellationToken);

        if (sourceText.Length == 0)
        {
            return;
        }

        var firstNonBlankLineIndex = FormattingTextAnalysisUtilities.FindFirstNonBlankLineIndex(sourceText);

        if (firstNonBlankLineIndex == 0)
        {
            return;
        }

        var endPosition = firstNonBlankLineIndex > 0
                              ? sourceText.Lines[firstNonBlankLineIndex].Start
                              : sourceText.Length;
        var diagnosticSpan = TextSpan.FromBounds(0, endPosition);

        context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, diagnosticSpan)));
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