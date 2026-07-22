using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    /// Determines whether the specified position falls inside comment trivia or preprocessor-disabled text.
    /// The formatter never rewrites the interior of such trivia, so trailing whitespace there can never be
    /// removed and must not be flagged
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="position">Position to inspect</param>
    /// <returns><see langword="true"/> if the position is inside a comment or disabled-text interior; otherwise, <see langword="false"/></returns>
    private static bool IsInsideNonFormattableTrivia(SyntaxNode root, int position)
    {
        var trivia = root.FindTrivia(position);

        return SyntaxTriviaUtilities.IsCommentTrivia(trivia)
               || trivia.IsKind(SyntaxKind.DisabledTextTrivia);
    }

    /// <summary>
    /// Analyzes the syntax tree for trailing whitespace
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

            var diagnosticStart = line.Start + trailingWhitespaceStart;

            if (IsInsideNonFormattableTrivia(root, diagnosticStart))
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