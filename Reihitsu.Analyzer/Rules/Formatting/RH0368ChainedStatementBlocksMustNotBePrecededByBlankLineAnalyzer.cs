using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0368: Chained statement blocks must not be preceded by blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0368ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer : DiagnosticAnalyzerBase<RH0368ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0368";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0368ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0368Title), nameof(AnalyzerResources.RH0368MessageFormat))
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

        for (var lineIndex = 1; lineIndex < sourceText.Lines.Count; lineIndex++)
        {
            if (rawStringLineIndices.Contains(lineIndex))
            {
                continue;
            }

            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[lineIndex]).TrimStart();

            if ((lineText.StartsWith("else", StringComparison.Ordinal)
                 || lineText.StartsWith("catch", StringComparison.Ordinal)
                 || lineText.StartsWith("finally", StringComparison.Ordinal))
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