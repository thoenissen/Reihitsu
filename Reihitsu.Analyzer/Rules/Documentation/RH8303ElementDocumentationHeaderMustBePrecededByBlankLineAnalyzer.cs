using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8303: Element documentation header must be preceded by blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8303";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8303Title), nameof(AnalyzerResources.RH8303MessageFormat))
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

        for (var lineIndex = 1; lineIndex < sourceText.Lines.Count; lineIndex++)
        {
            if (nonFormattableLineIndices.Contains(lineIndex))
            {
                continue;
            }

            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[lineIndex]);

            if (DocumentationAnalysisUtilities.IsDocumentationLine(lineText) == false)
            {
                continue;
            }

            var previousLineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[lineIndex - 1]);

            if (DocumentationAnalysisUtilities.IsDocumentationLine(previousLineText)
                || FormattingTextAnalysisUtilities.IsBlankLine(sourceText, lineIndex - 1))
            {
                continue;
            }

            var previousNonBlankLineIndex = FormattingTextAnalysisUtilities.FindPreviousNonBlankLineIndex(sourceText, lineIndex);

            if (previousNonBlankLineIndex < 0)
            {
                continue;
            }

            var previousNonBlankLineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[previousNonBlankLineIndex]).Trim();

            // A documentation header that abuts an ordinary comment or a preprocessor directive is exempt, mirroring
            // RH5020: the formatter treats adjacent comment blocks as a unit and never inserts a blank line between
            // them, and it leaves directive-adjacent comments untouched, so flagging these would be unfixable (issue #449)
            if (previousNonBlankLineText.EndsWith("{", StringComparison.Ordinal)
                || previousNonBlankLineText.StartsWith("//", StringComparison.Ordinal)
                || previousNonBlankLineText.StartsWith("#", StringComparison.Ordinal))
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

        context.RegisterSyntaxTreeActionWithDocumentationModeCheck(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}