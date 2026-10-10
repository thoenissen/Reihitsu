using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8302: Element documentation headers must not be followed by blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8302";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8302Title), nameof(AnalyzerResources.RH8302MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Tries to get the complete contiguous run of blank lines beginning at the specified line
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="firstLineIndex">Index of the first possible blank line</param>
    /// <param name="blankLineRun">Span of the complete blank-line run</param>
    /// <returns><see langword="true"/> if a blank-line run was found; otherwise, <see langword="false"/></returns>
    private static bool TryGetBlankLineRun(SourceText sourceText,
                                           int firstLineIndex,
                                           out TextSpan blankLineRun)
    {
        blankLineRun = default;

        if (firstLineIndex >= sourceText.Lines.Count
            || FormattingTextAnalysisUtilities.IsBlankLine(sourceText, firstLineIndex) == false)
        {
            return false;
        }

        var firstLine = sourceText.Lines[firstLineIndex];

        if (firstLine.EndIncludingLineBreak == firstLine.End)
        {
            return false;
        }

        var lastLineIndex = firstLineIndex;

        while (lastLineIndex + 1 < sourceText.Lines.Count
               && FormattingTextAnalysisUtilities.IsBlankLine(sourceText, lastLineIndex + 1))
        {
            lastLineIndex++;
        }

        blankLineRun = TextSpan.FromBounds(firstLine.Start,
                                           sourceText.Lines[lastLineIndex].EndIncludingLineBreak);

        return true;
    }

    /// <summary>
    /// Reports a diagnostic for the complete blank-line run that begins at the specified line
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="firstLineIndex">Index of the first possible blank line</param>
    private void ReportFollowingBlankLineRun(SyntaxTreeAnalysisContext context,
                                             SourceText sourceText,
                                             int firstLineIndex)
    {
        if (TryGetBlankLineRun(sourceText, firstLineIndex, out var blankLineRun))
        {
            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, blankLineRun)));
        }
    }

    /// <summary>
    /// Reports blank lines that immediately follow multi-line documentation comments
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="root">Syntax root</param>
    /// <param name="sourceText">Source text</param>
    private void AnalyzeMultiLineDocumentationComments(SyntaxTreeAnalysisContext context,
                                                       SyntaxNode root,
                                                       SourceText sourceText)
    {
        foreach (var token in root.DescendantTokens())
        {
            foreach (var documentationComment in token.LeadingTrivia)
            {
                if (documentationComment.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) == false)
                {
                    continue;
                }

                var commentEnd = documentationComment.FullSpan.End;
                var commentEndLine = sourceText.Lines.GetLineFromPosition(commentEnd - 1);

                if (string.IsNullOrWhiteSpace(sourceText.ToString(TextSpan.FromBounds(commentEnd, commentEndLine.End))) == false)
                {
                    continue;
                }

                var nextLineIndex = commentEndLine.LineNumber + 1;

                ReportFollowingBlankLineRun(context, sourceText, nextLineIndex);
            }
        }
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

        var lineIndex = 0;

        while (lineIndex < sourceText.Lines.Count - 1)
        {
            if (nonFormattableLineIndices.Contains(lineIndex))
            {
                lineIndex++;

                continue;
            }

            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[lineIndex]);

            if (DocumentationAnalysisUtilities.IsDocumentationLine(lineText) == false)
            {
                lineIndex++;

                continue;
            }

            var nextLineIndex = lineIndex + 1;

            while (nextLineIndex < sourceText.Lines.Count
                   && nonFormattableLineIndices.Contains(nextLineIndex) == false
                   && DocumentationAnalysisUtilities.IsDocumentationLine(FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[nextLineIndex])))
            {
                nextLineIndex++;
            }

            ReportFollowingBlankLineRun(context, sourceText, nextLineIndex);

            lineIndex = nextLineIndex;
        }

        AnalyzeMultiLineDocumentationComments(context, root, sourceText);
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