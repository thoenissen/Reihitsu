using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6023: Code must not contain alignment padding
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6023CodeMustNotContainAlignmentPaddingAnalyzer : DiagnosticAnalyzerBase<RH6023CodeMustNotContainAlignmentPaddingAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6023";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6023CodeMustNotContainAlignmentPaddingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6023Title), nameof(AnalyzerResources.RH6023MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the gap between two tokens is a run of spaces or tabs. Because leading
    /// indentation, trailing whitespace, and line breaks all contain an end-of-line character, and
    /// string, comment, and documentation content is either part of a single token or introduces
    /// non-whitespace characters, a gap that consists solely of spaces and tabs is inter-token
    /// alignment padding
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="start">Inclusive start of the gap</param>
    /// <param name="end">Exclusive end of the gap</param>
    /// <returns><see langword="true"/> if the gap is alignment padding</returns>
    private static bool IsAlignmentPadding(SourceText sourceText, int start, int end)
    {
        for (var index = start; index < end; index++)
        {
            if (sourceText[index] != ' '
                && sourceText[index] != '\t')
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Analyzes the syntax tree for alignment padding between tokens
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var token in root.DescendantTokens())
        {
            var nextToken = token.GetNextToken();

            if (nextToken.RawKind == 0
                || nextToken.IsKind(SyntaxKind.EndOfFileToken))
            {
                continue;
            }

            var start = token.Span.End;
            var end = nextToken.SpanStart;

            if (end - start <= 1
                || IsAlignmentPadding(sourceText, start, end) == false)
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(start, end))));
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