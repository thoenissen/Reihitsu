using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0364: Element documentation headers must not be followed by blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0364ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer : DiagnosticAnalyzerBase<RH0364ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0364";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0364ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0364Title), nameof(AnalyzerResources.RH0364MessageFormat))
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
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var rawStringLineIndices = FormattingTextAnalysisUtilities.GetRawStringLineIndices(root, sourceText);

        var lineIndex = 0;

        while (lineIndex < sourceText.Lines.Count - 1)
        {
            if (rawStringLineIndices.Contains(lineIndex))
            {
                lineIndex++;

                continue;
            }

            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[lineIndex]).TrimStart();

            if (lineText.StartsWith("///", StringComparison.Ordinal) == false)
            {
                lineIndex++;

                continue;
            }

            var nextLineIndex = lineIndex + 1;

            while (nextLineIndex < sourceText.Lines.Count
                   && rawStringLineIndices.Contains(nextLineIndex) == false
                   && FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[nextLineIndex]).TrimStart().StartsWith("///", StringComparison.Ordinal))
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

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}