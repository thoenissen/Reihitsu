using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5602: Code must not contain trailing whitespace
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5602CodeMustNotContainTrailingWhitespaceAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5602";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5602CodeMustNotContainTrailingWhitespaceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5602Title), nameof(AnalyzerResources.RH5602MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree for trailing whitespace. A run of whitespace is reported unless it is part of the text of
    /// a literal: the whitespace behind a literal's end and the whitespace inside an interpolation hole are code
    /// (SHARED-DIAG-003), whether or not a literal touches the line
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var line in sourceText.Lines)
        {
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

            var diagnosticStart = line.Start + trailingWhitespaceStart;

            if (FormattingTextAnalysisUtilities.IsInsideLiteralText(root.FindToken(diagnosticStart), diagnosticStart)
                || SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, diagnosticStart))
            {
                continue;
            }

            var diagnosticSpan = TextSpan.FromBounds(diagnosticStart, line.End);

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