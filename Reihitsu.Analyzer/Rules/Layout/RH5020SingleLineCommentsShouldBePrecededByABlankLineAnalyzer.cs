using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5020: Single-line comments should be preceded by a blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5020SingleLineCommentsShouldBePrecededByABlankLineAnalyzer : DiagnosticAnalyzerBase<RH5020SingleLineCommentsShouldBePrecededByABlankLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5020";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5020SingleLineCommentsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5020Title), nameof(AnalyzerResources.RH5020MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the comment starts a block or switch section
    /// </summary>
    /// <param name="commentTrivia">The comment trivia</param>
    /// <returns><see langword="true"/> if the comment starts a block or switch section</returns>
    private static bool IsFirstCommentInBlock(SyntaxTrivia commentTrivia)
    {
        var previousToken = commentTrivia.Token.GetPreviousToken();

        return previousToken.RawKind == 0
               || previousToken.IsKind(SyntaxKind.OpenBraceToken)
               || (previousToken.IsKind(SyntaxKind.ColonToken)
                   && previousToken.Parent?.Kind() is SyntaxKind.CaseSwitchLabel
                                                   or SyntaxKind.CasePatternSwitchLabel
                                                   or SyntaxKind.DefaultSwitchLabel);
    }

    /// <summary>
    /// Determines whether the line immediately preceding the comment is a preprocessor directive
    /// </summary>
    /// <param name="commentTrivia">The comment trivia</param>
    /// <returns><see langword="true"/> if the comment is immediately preceded by a preprocessor directive</returns>
    private static bool IsPrecededByDirective(SyntaxTrivia commentTrivia)
    {
        var leadingTrivia = commentTrivia.Token.LeadingTrivia;
        var commentIndex = leadingTrivia.IndexOf(commentTrivia);

        for (var index = commentIndex - 1; index >= 0; index--)
        {
            var trivia = leadingTrivia[index];

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia)
                || trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                continue;
            }

            return trivia.IsDirective;
        }

        return false;
    }

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

            if (string.IsNullOrWhiteSpace(commentLineText.Substring(0, commentColumnIndex)) == false)
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
                || IsPrecededByDirective(trivia)
                || IsFirstCommentInBlock(trivia))
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