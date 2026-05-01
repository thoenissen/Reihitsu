using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0363: Opening brace must not be followed by blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0363OpeningBraceMustNotBeFollowedByBlankLineAnalyzer : DiagnosticAnalyzerBase<RH0363OpeningBraceMustNotBeFollowedByBlankLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0363";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0363OpeningBraceMustNotBeFollowedByBlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0363Title), nameof(AnalyzerResources.RH0363MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var rawStringLineIndices = FormattingTextAnalysisUtilities.GetRawStringLineIndices(root, sourceText);

        for (var lineIndex = 0; lineIndex < sourceText.Lines.Count - 1; lineIndex++)
        {
            if (rawStringLineIndices.Contains(lineIndex)
                || rawStringLineIndices.Contains(lineIndex + 1))
            {
                continue;
            }

            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[lineIndex]).Trim();

            if (lineText.EndsWith("{", StringComparison.Ordinal)
                && FormattingTextAnalysisUtilities.IsBlankLine(sourceText, lineIndex + 1))
            {
                var blankLine = sourceText.Lines[lineIndex + 1];

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