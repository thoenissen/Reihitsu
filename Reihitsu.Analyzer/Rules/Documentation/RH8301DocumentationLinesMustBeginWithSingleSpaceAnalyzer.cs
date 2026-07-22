using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8301: Documentation lines must begin with single space
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8301";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8301Title), nameof(AnalyzerResources.RH8301MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether a line is a single XML documentation line
    /// </summary>
    /// <param name="lineText">Line text</param>
    /// <returns><see langword="true"/> if the line is XML documentation</returns>
    private static bool IsDocumentationLine(string lineText)
    {
        var trimmed = lineText.TrimStart();

        return trimmed.StartsWith("///", StringComparison.Ordinal)
               && trimmed.StartsWith("////", StringComparison.Ordinal) == false;
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var nonFormattableLineIndices = FormattingTextAnalysisUtilities.GetNonFormattableLineIndices(root, sourceText);

        for (var lineIndex = 0; lineIndex < sourceText.Lines.Count; lineIndex++)
        {
            if (nonFormattableLineIndices.Contains(lineIndex))
            {
                continue;
            }

            var line = sourceText.Lines[lineIndex];
            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, line);
            var trimmed = lineText.TrimStart();

            if (IsDocumentationLine(lineText) == false)
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

        context.RegisterSyntaxTreeActionWithDocumentationModeCheck(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}