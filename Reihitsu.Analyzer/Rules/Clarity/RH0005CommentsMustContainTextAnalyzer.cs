using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

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
    /// Analyze the syntax tree.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
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