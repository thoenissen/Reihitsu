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
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var nonFormattableLineIndices = FormattingTextAnalysisUtilities.GetNonFormattableLineIndices(root, sourceText);

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