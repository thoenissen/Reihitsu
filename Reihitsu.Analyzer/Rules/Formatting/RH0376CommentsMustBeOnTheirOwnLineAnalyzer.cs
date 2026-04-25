using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0376: Comments must be on their own line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0376CommentsMustBeOnTheirOwnLineAnalyzer : DiagnosticAnalyzerBase<RH0376CommentsMustBeOnTheirOwnLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0376";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0376CommentsMustBeOnTheirOwnLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0376Title), nameof(AnalyzerResources.RH0376MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a comment shares any of its occupied lines with code.
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
                && FormattingTextAnalysisUtilities.ContainsNonWhitespace(sourceText.ToString(TextSpan.FromBounds(line.Start, prefixEnd))))
            {
                return true;
            }

            if (suffixStart < line.End
                && FormattingTextAnalysisUtilities.ContainsNonWhitespace(sourceText.ToString(TextSpan.FromBounds(suffixStart, line.End))))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Analyzes the syntax tree.
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