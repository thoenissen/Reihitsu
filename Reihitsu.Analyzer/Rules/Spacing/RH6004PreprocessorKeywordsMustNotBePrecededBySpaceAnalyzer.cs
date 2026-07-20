using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6004: Preprocessor keywords must not be preceded by space
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6004";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6004Title), nameof(AnalyzerResources.RH6004MessageFormat))
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

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.GetStructure() is DirectiveTriviaSyntax { IsActive: false })
            {
                nonFormattableLineIndices.Add(sourceText.Lines.GetLineFromPosition(trivia.SpanStart).LineNumber);
            }
        }

        foreach (var line in sourceText.Lines)
        {
            if (nonFormattableLineIndices.Contains(line.LineNumber))
            {
                continue;
            }

            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, line);
            var trimmed = lineText.TrimStart();

            if (trimmed.StartsWith("#", StringComparison.Ordinal) == false)
            {
                continue;
            }

            if (trimmed.StartsWith("#region", StringComparison.Ordinal)
                || trimmed.StartsWith("#endregion", StringComparison.Ordinal))
            {
                continue;
            }

            var indentLength = lineText.Length - trimmed.Length;

            if (indentLength > 0)
            {
                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(line.Start, line.Start + indentLength))));
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