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
    /// Determines whether the documentation header at the specified position directly follows an ordinary comment or a
    /// preprocessor directive within the same leading trivia block
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="documentationPosition">Position of the documentation header marker</param>
    /// <returns><see langword="true"/> if the header abuts a comment or a preprocessor directive</returns>
    private static bool IsPrecededByCommentOrDirective(SyntaxNode root, int documentationPosition)
    {
        var documentationTrivia = root.FindTrivia(documentationPosition);
        var leadingTrivia = documentationTrivia.Token.LeadingTrivia;
        var documentationIndex = leadingTrivia.IndexOf(documentationTrivia);

        if (documentationIndex <= 0)
        {
            return false;
        }

        for (var triviaIndex = documentationIndex - 1; triviaIndex >= 0; triviaIndex--)
        {
            var trivia = leadingTrivia[triviaIndex];

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia)
                || trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                continue;
            }

            return SyntaxTriviaUtilities.IsCommentTrivia(trivia)
                   || trivia.IsDirective;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a documentation trivia directly follows an ordinary comment or directive in its owning
    /// token's leading trivia
    /// </summary>
    /// <param name="documentationTrivia">Documentation trivia to inspect</param>
    /// <returns><see langword="true"/> when an adjacent comment or directive precedes it; otherwise, <see langword="false"/></returns>
    private static bool IsPrecededByCommentOrDirective(SyntaxTrivia documentationTrivia)
    {
        var leadingTrivia = documentationTrivia.Token.LeadingTrivia;
        var documentationIndex = leadingTrivia.IndexOf(documentationTrivia);

        for (var triviaIndex = documentationIndex - 1; triviaIndex >= 0; triviaIndex--)
        {
            var trivia = leadingTrivia[triviaIndex];

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia)
                || trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                continue;
            }

            return SyntaxTriviaUtilities.IsCommentTrivia(trivia)
                   || trivia.IsDirective;
        }

        return false;
    }

    /// <summary>
    /// Reports delimited XML documentation headers that are missing a preceding blank line
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="root">Syntax root</param>
    /// <param name="sourceText">Source text</param>
    private void AnalyzeDelimitedDocumentation(SyntaxTreeAnalysisContext context, SyntaxNode root, SourceText sourceText)
    {
        foreach (var trivia in root.DescendantTokens()
                                   .SelectMany(static token => token.LeadingTrivia)
                                   .Where(static trivia => trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)))
        {
            var documentationStart = trivia.FullSpan.Start;
            var line = sourceText.Lines.GetLineFromPosition(documentationStart);
            var lineIndex = line.LineNumber;

            if (lineIndex == 0
                || FormattingTextAnalysisUtilities.IsBlankLine(sourceText, lineIndex - 1))
            {
                continue;
            }

            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, line);
            var columnIndex = Math.Min(documentationStart - line.Start, lineText.Length);

            if (string.IsNullOrWhiteSpace(lineText.Substring(0, columnIndex)) == false)
            {
                continue;
            }

            var previousNonBlankLineIndex = FormattingTextAnalysisUtilities.FindPreviousNonBlankLineIndex(sourceText, lineIndex);

            if (previousNonBlankLineIndex < 0)
            {
                continue;
            }

            var previousLineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[previousNonBlankLineIndex]).Trim();

            if (previousLineText.EndsWith("{", StringComparison.Ordinal)
                || IsPrecededByCommentOrDirective(trivia))
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(documentationStart, documentationStart + 3))));
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

        AnalyzeDelimitedDocumentation(context, root, sourceText);

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

            if (previousNonBlankLineText.EndsWith("{", StringComparison.Ordinal))
            {
                continue;
            }

            var lineTextWithIndentation = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[lineIndex]);
            var documentationStart = lineTextWithIndentation.IndexOf("///", StringComparison.Ordinal);
            var diagnosticStart = sourceText.Lines[lineIndex].Start + documentationStart;

            // A documentation header that directly abuts an ordinary comment or a preprocessor directive is exempt,
            // mirroring RH5020: the formatter treats adjacent comment blocks as a unit and never inserts a blank line
            // between them, and it leaves directive-adjacent comments untouched, so flagging these would leave a
            // permanent, CLI-unfixable diagnostic. The preceding construct is read from trivia rather than line text so
            // that string content that only looks like a comment or a directive is not mistaken for one (issue #449)
            if (IsPrecededByCommentOrDirective(root, diagnosticStart))
            {
                continue;
            }

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