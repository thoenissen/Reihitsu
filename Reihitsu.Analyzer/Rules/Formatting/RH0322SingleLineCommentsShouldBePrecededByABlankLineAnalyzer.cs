using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0322: Single-line comments should be preceded by a blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzer : DiagnosticAnalyzerBase<RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0322";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0322Title), nameof(AnalyzerResources.RH0322MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes single-line comments for missing separating blank lines
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var syntaxRoot = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var trivia in syntaxRoot.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) == false)
            {
                continue;
            }

            var commentText = trivia.ToString();

            if (commentText.StartsWith("////", StringComparison.Ordinal))
            {
                continue;
            }

            var commentLineSpan = trivia.GetLocation().GetLineSpan();
            var commentLineIndex = commentLineSpan.StartLinePosition.Line;

            if (commentLineIndex == 0)
            {
                continue;
            }

            var commentLine = sourceText.Lines[commentLineIndex];
            var commentLineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, commentLine);
            var commentColumnIndex = Math.Min(commentLineSpan.StartLinePosition.Character, commentLineText.Length);

            if (FormattingTextAnalysisUtilities.ContainsNonWhitespace(commentLineText.Substring(0, commentColumnIndex)))
            {
                continue;
            }

            var previousNonBlankLineIndex = FormattingTextAnalysisUtilities.FindPreviousNonBlankLineIndex(sourceText, commentLineIndex);

            if (previousNonBlankLineIndex < 0
                || commentLineIndex - previousNonBlankLineIndex > 1)
            {
                continue;
            }

            var previousLineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[previousNonBlankLineIndex]).TrimStart();

            if (previousLineText.StartsWith("//", StringComparison.Ordinal)
                || previousLineText.EndsWith("{", StringComparison.Ordinal))
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(trivia.GetLocation()));
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