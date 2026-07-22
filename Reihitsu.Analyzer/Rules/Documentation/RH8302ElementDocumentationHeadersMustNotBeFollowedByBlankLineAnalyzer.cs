using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8302: Element documentation headers must not be followed by blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8302";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8302Title), nameof(AnalyzerResources.RH8302MessageFormat))
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
        var nonFormattableLineIndices = FormattingTextAnalysisUtilities.GetNonFormattableLineIndices(root, sourceText);

        var lineIndex = 0;

        while (lineIndex < sourceText.Lines.Count - 1)
        {
            if (nonFormattableLineIndices.Contains(lineIndex))
            {
                lineIndex++;

                continue;
            }

            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[lineIndex]);

            if (DocumentationAnalysisUtilities.IsDocumentationLine(lineText) == false)
            {
                lineIndex++;

                continue;
            }

            var nextLineIndex = lineIndex + 1;

            while (nextLineIndex < sourceText.Lines.Count
                   && nonFormattableLineIndices.Contains(nextLineIndex) == false
                   && DocumentationAnalysisUtilities.IsDocumentationLine(FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[nextLineIndex])))
            {
                nextLineIndex++;
            }

            if (nextLineIndex < sourceText.Lines.Count
                && FormattingTextAnalysisUtilities.IsBlankLine(sourceText, nextLineIndex))
            {
                var blankLine = sourceText.Lines[nextLineIndex];

                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(blankLine.Start, blankLine.EndIncludingLineBreak))));
            }

            lineIndex = nextLineIndex;
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeActionWithDocumentationModeCheck(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}