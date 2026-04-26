using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0358: Code must not contain trailing whitespace.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0358CodeMustNotContainTrailingWhitespaceAnalyzer : DiagnosticAnalyzerBase<RH0358CodeMustNotContainTrailingWhitespaceAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0358";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0358CodeMustNotContainTrailingWhitespaceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0358Title), nameof(AnalyzerResources.RH0358MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree for trailing whitespace.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var stringLineIndices = FormattingTextAnalysisUtilities.GetStringLineIndices(root, sourceText);

        for (var lineIndex = 0; lineIndex < sourceText.Lines.Count; lineIndex++)
        {
            if (stringLineIndices.Contains(lineIndex))
            {
                continue;
            }

            var line = sourceText.Lines[lineIndex];
            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, line);

            if (lineText.Length == 0)
            {
                continue;
            }

            var trailingWhitespaceStart = FormattingTextAnalysisUtilities.GetTrailingWhitespaceStart(lineText);

            if (trailingWhitespaceStart >= lineText.Length)
            {
                continue;
            }

            var diagnosticSpan = TextSpan.FromBounds(line.Start + trailingWhitespaceStart, line.End);

            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, diagnosticSpan)));
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