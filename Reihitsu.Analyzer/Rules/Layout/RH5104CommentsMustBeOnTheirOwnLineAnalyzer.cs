using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5104: Comments must be on their own line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5104CommentsMustBeOnTheirOwnLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5104";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5104CommentsMustBeOnTheirOwnLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5104Title), nameof(AnalyzerResources.RH5104MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the comment sits inside an interpolation hole of a multi-line interpolated
    /// string (verbatim or raw). Relocating such a comment to its own line would insert text into the
    /// string's literal content and silently change its runtime value, so it is exempt. This checks every
    /// enclosing interpolated string, not just the innermost one, so a comment inside a single-line
    /// interpolated string that is itself nested in a hole of an outer multi-line string is still exempt
    /// </summary>
    /// <param name="commentTrivia">Comment trivia</param>
    /// <returns><see langword="true"/> if the comment is inside such an interpolation hole</returns>
    private static bool IsInsideMultiLineInterpolatedStringHole(SyntaxTrivia commentTrivia)
    {
        return commentTrivia.Token.Parent?.AncestorsAndSelf()
                            .OfType<InterpolatedStringExpressionSyntax>()
                            .Any(interpolatedString => SyntaxNodeUtilities.IsSingleLine(interpolatedString) == false) == true;
    }

    /// <summary>
    /// Determines whether a comment shares any of its occupied lines with code
    /// </summary>
    /// <param name="commentTrivia">Comment trivia</param>
    /// <param name="sourceText">Source text</param>
    /// <returns><see langword="true"/> if the comment is not on its own line</returns>
    private static bool IsInlineComment(SyntaxTrivia commentTrivia, SourceText sourceText)
    {
        var commentSpan = commentTrivia.Span;
        var startLineIndex = sourceText.Lines.GetLineFromPosition(commentSpan.Start).LineNumber;
        var endPosition = commentSpan.Length == 0 ? commentSpan.End : commentSpan.End - 1;
        var endLineIndex = sourceText.Lines.GetLineFromPosition(endPosition).LineNumber;

        for (var lineIndex = startLineIndex; lineIndex <= endLineIndex; lineIndex++)
        {
            var line = sourceText.Lines[lineIndex];
            var prefixEnd = Math.Min(commentSpan.Start, line.End);
            var suffixStart = Math.Max(commentSpan.End, line.Start);

            if (prefixEnd > line.Start
                && string.IsNullOrWhiteSpace(sourceText.ToString(TextSpan.FromBounds(line.Start, prefixEnd))) == false)
            {
                return true;
            }

            if (suffixStart < line.End
                && string.IsNullOrWhiteSpace(sourceText.ToString(TextSpan.FromBounds(suffixStart, line.End))) == false)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var syntaxRoot = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var trivia in syntaxRoot.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) == false
                && trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) == false)
            {
                continue;
            }

            if (trivia.Token.Parent?.AncestorsAndSelf().OfType<DirectiveTriviaSyntax>().Any() == true)
            {
                continue;
            }

            if (IsInsideMultiLineInterpolatedStringHole(trivia))
            {
                continue;
            }

            if (IsInlineComment(trivia, sourceText))
            {
                context.ReportDiagnostic(CreateDiagnostic(trivia.GetLocation()));
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