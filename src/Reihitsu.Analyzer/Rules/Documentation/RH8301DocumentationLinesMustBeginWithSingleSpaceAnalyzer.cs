using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
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
    /// Gets the indices of the lines of every <c>///</c> comment that the formatter leaves as written, so their
    /// separators are not judged
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The line indices of the comments left as written</returns>
    private static HashSet<int> GetLineIndicesOfCommentsLeftAsWritten(SyntaxNode root, SourceText sourceText, CancellationToken cancellationToken)
    {
        var lineIndices = new HashSet<int>();

        foreach (var trivia in root.DescendantTrivia())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (DocumentationCommentUtilities.IsLeftAsWritten(trivia, sourceText) == false)
            {
                continue;
            }

            var firstLine = sourceText.Lines.GetLineFromPosition(trivia.Span.Start).LineNumber;
            var lastLine = sourceText.Lines.GetLineFromPosition(Math.Max(trivia.Span.Start, trivia.Span.End - 1)).LineNumber;

            for (var lineIndex = firstLine; lineIndex <= lastLine; lineIndex++)
            {
                lineIndices.Add(lineIndex);
            }
        }

        return lineIndices;
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
        var lineIndicesLeftAsWritten = GetLineIndicesOfCommentsLeftAsWritten(root, sourceText, context.CancellationToken);

        for (var lineIndex = 0; lineIndex < sourceText.Lines.Count; lineIndex++)
        {
            if (nonFormattableLineIndices.Contains(lineIndex)
                || lineIndicesLeftAsWritten.Contains(lineIndex))
            {
                continue;
            }

            var line = sourceText.Lines[lineIndex];
            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, line);
            var trimmed = lineText.TrimStart();

            if (DocumentationAnalysisUtilities.IsDocumentationLine(lineText) == false)
            {
                continue;
            }

            AnalyzeExterior(context, sourceText, line.Start + lineText.Length - trimmed.Length);
        }

        // The first line of a comment written behind code is judged as if the comment started its own line (SHARED-DOC-004);
        // its other lines start with their own exterior and are judged above
        foreach (var exteriorStart in DocumentationAnalysisUtilities.GetDocumentationCommentsBehindCode(root, sourceText, context.CancellationToken)
                                                                    .Select(documentationComment => documentationComment.FullSpan.Start)
                                                                    .Where(exteriorStart => nonFormattableLineIndices.Contains(sourceText.Lines.GetLineFromPosition(exteriorStart).LineNumber) == false
                                                                                            && lineIndicesLeftAsWritten.Contains(sourceText.Lines.GetLineFromPosition(exteriorStart).LineNumber) == false))
        {
            AnalyzeExterior(context, sourceText, exteriorStart);
        }
    }

    /// <summary>
    /// Reports the <c>///</c> at the given position when the text behind it on its line does not start with exactly one
    /// space
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="exteriorStart">Position of the <c>///</c></param>
    private void AnalyzeExterior(SyntaxTreeAnalysisContext context, SourceText sourceText, int exteriorStart)
    {
        var line = sourceText.Lines.GetLineFromPosition(exteriorStart);
        var suffix = sourceText.ToString(TextSpan.FromBounds(exteriorStart + 3, line.End));

        if (string.Equals(suffix, DocumentationCommentUtilities.NormalizeExteriorSuffix(suffix), StringComparison.Ordinal) == false)
        {
            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(exteriorStart, exteriorStart + 3))));
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