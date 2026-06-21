using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Analyzer base class for checking that a <c>#region</c> or <c>#endregion</c> directive is surrounded by blank lines
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
public abstract class RegionDirectiveBlankLineAnalyzerBase<TAnalyzer> : DiagnosticAnalyzerBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer
{
    #region Fields

    /// <summary>
    /// Kind of the directive that is analyzed
    /// </summary>
    private readonly SyntaxKind _directiveKind;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">The diagnostic ID</param>
    /// <param name="titleResourceName">The resource name for the title of the diagnostic</param>
    /// <param name="messageFormatResourceName">The resource name for the message format of the diagnostic</param>
    /// <param name="directiveKind">Kind of the directive that is analyzed</param>
    private protected RegionDirectiveBlankLineAnalyzerBase(string diagnosticId, string titleResourceName, string messageFormatResourceName, SyntaxKind directiveKind)
        : base(diagnosticId, DiagnosticCategory.Layout, titleResourceName, messageFormatResourceName)
    {
        _directiveKind = directiveKind;
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
        var openBraceEndLineIndices = FormattingTextAnalysisUtilities.GetLineIndicesEndingWithToken(root,
                                                                                                    sourceText,
                                                                                                    static token => token.IsKind(SyntaxKind.OpenBraceToken));
        var closeBraceStartLineIndices = FormattingTextAnalysisUtilities.GetLineIndicesBeginningWithToken(root,
                                                                                                          sourceText,
                                                                                                          static token => token.IsKind(SyntaxKind.CloseBraceToken));
        var nonFormattableLineIndices = FormattingTextAnalysisUtilities.GetNonFormattableLineIndices(root, sourceText);

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsKind(_directiveKind) == false)
            {
                continue;
            }

            var directiveLine = sourceText.Lines.GetLineFromPosition(trivia.SpanStart);
            var directiveLineIndex = directiveLine.LineNumber;

            if (RegionDirectiveBlankLineUtilities.IsAdjacentToNonFormattableLine(directiveLineIndex, sourceText.Lines.Count, nonFormattableLineIndices))
            {
                continue;
            }

            if (RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore(sourceText, directiveLineIndex, openBraceEndLineIndices)
                || RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineAfter(sourceText, directiveLineIndex, closeBraceStartLineIndices))
            {
                var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, directiveLine);
                var contentStart = directiveLine.Start + FormattingTextAnalysisUtilities.GetLeadingWhitespace(lineText).Length;
                var contentEnd = directiveLine.Start + FormattingTextAnalysisUtilities.GetTrailingWhitespaceStart(lineText);

                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(contentStart, contentEnd))));
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