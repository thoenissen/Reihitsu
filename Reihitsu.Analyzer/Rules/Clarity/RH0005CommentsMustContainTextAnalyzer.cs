using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH0005: Comments must contain text.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0005CommentsMustContainTextAnalyzer : DiagnosticAnalyzerBase<RH0005CommentsMustContainTextAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0005";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0005CommentsMustContainTextAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH0005Title), nameof(AnalyzerResources.RH0005MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the comment trivia is empty.
    /// </summary>
    /// <param name="commentTrivia">Comment trivia</param>
    /// <returns><see langword="true"/> if the comment is empty</returns>
    private static bool IsEmptyComment(SyntaxTrivia commentTrivia)
    {
        var commentText = commentTrivia.Kind() switch
                          {
                              SyntaxKind.SingleLineCommentTrivia => commentTrivia.ToString().Substring(2),
                              SyntaxKind.MultiLineCommentTrivia => commentTrivia.ToString().Substring(2, commentTrivia.ToString().Length - 4),
                              _ => string.Empty
                          };

        return string.IsNullOrWhiteSpace(commentText);
    }

    /// <summary>
    /// Determine whether the specified line is a regular single-line comment.
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="lineIndex">Line index</param>
    /// <returns><see langword="true"/> if the line contains a regular single-line comment</returns>
    private static bool IsSingleLineCommentLine(SourceText sourceText, int lineIndex)
    {
        var line = sourceText.Lines[lineIndex];
        var lineText = sourceText.ToString(TextSpan.FromBounds(line.Start, line.End)).TrimStart();

        return lineText.StartsWith("//", StringComparison.Ordinal)
               && lineText.StartsWith("///", StringComparison.Ordinal) == false;
    }

    /// <summary>
    /// Determine whether an empty single-line comment represents a separator inside a comment block.
    /// </summary>
    /// <param name="commentTrivia">Comment trivia</param>
    /// <param name="sourceText">Source text</param>
    /// <returns><see langword="true"/> if the comment is an empty separator inside a comment block</returns>
    private static bool IsBlankLineInsideSingleLineCommentBlock(SyntaxTrivia commentTrivia, SourceText sourceText)
    {
        if (commentTrivia.IsKind(SyntaxKind.SingleLineCommentTrivia) == false)
        {
            return false;
        }

        var lineIndex = sourceText.Lines.GetLineFromPosition(commentTrivia.SpanStart).LineNumber;

        if (lineIndex == 0
            || lineIndex >= sourceText.Lines.Count - 1)
        {
            return false;
        }

        return IsSingleLineCommentLine(sourceText, lineIndex - 1)
               && IsSingleLineCommentLine(sourceText, lineIndex + 1);
    }

    /// <summary>
    /// Analyze the syntax tree.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var triviaList = root.DescendantTrivia(descendIntoTrivia: true);

        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) == false
                && trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) == false)
            {
                continue;
            }

            if (IsEmptyComment(trivia))
            {
                if (IsBlankLineInsideSingleLineCommentBlock(trivia, sourceText))
                {
                    continue;
                }

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