using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0337: Documentation lines must begin with single space.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0337DocumentationLinesMustBeginWithSingleSpaceAnalyzer : DiagnosticAnalyzerBase<RH0337DocumentationLinesMustBeginWithSingleSpaceAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0337";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0337DocumentationLinesMustBeginWithSingleSpaceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0337Title), nameof(AnalyzerResources.RH0337MessageFormat))
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

        for (var lineIndex = 0; lineIndex < sourceText.Lines.Count; lineIndex++)
        {
            if (rawStringLineIndices.Contains(lineIndex))
            {
                continue;
            }

            var line = sourceText.Lines[lineIndex];
            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, line);
            var trimmed = lineText.TrimStart();

            if (trimmed.StartsWith("///", StringComparison.Ordinal) == false
                || trimmed.StartsWith("////", StringComparison.Ordinal))
            {
                continue;
            }

            var indentLength = lineText.Length - trimmed.Length;
            var suffix = trimmed.Substring(3);

            if (suffix.Length == 0
                || (suffix.StartsWith(" ", StringComparison.Ordinal) && (suffix.Length == 1 || suffix[1] != ' ')))
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(line.Start + indentLength, line.Start + indentLength + 3))));
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