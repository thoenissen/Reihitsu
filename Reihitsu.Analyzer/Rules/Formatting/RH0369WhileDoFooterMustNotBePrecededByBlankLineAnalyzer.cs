using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0369: While-do footer must not be preceded by blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0369WhileDoFooterMustNotBePrecededByBlankLineAnalyzer : DiagnosticAnalyzerBase<RH0369WhileDoFooterMustNotBePrecededByBlankLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0369";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0369WhileDoFooterMustNotBePrecededByBlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0369Title), nameof(AnalyzerResources.RH0369MessageFormat))
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

            if (lineText.StartsWith("while", StringComparison.Ordinal)
                && FormattingTextAnalysisUtilities.IsBlankLine(sourceText, lineIndex - 1))
            {
                var blankLine = sourceText.Lines[lineIndex - 1];
                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(blankLine.Start, blankLine.EndIncludingLineBreak))));
            }
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