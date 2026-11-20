using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base class for analyzers that report ordinary comments placed in a specific position inside a declaration or a
/// statement. Each comment is reported at its own location, whether or not it shares a line with code, and each line of
/// a <c>///</c> comment that documents nothing is reported as a comment of its own
/// </summary>
public abstract class CommentPositionAnalyzerBase : DiagnosticAnalyzerBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    protected CommentPositionAnalyzerBase(string diagnosticId)
        : base(diagnosticId, DiagnosticCategory.Layout)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a comment between the two tokens is in the position this rule reports
    /// </summary>
    /// <param name="comment">Comment trivia</param>
    /// <param name="previousToken">Token before the comment, or a <see cref="SyntaxKind.None"/> token when the comment precedes the first token of the file</param>
    /// <param name="nextToken">Token after the comment</param>
    /// <returns><see langword="true"/> if the comment is misplaced</returns>
    protected abstract bool IsMisplaced(SyntaxTrivia comment, SyntaxToken previousToken, SyntaxToken nextToken);

    /// <summary>
    /// Analyzes the syntax tree. Comments inside preprocessor directives belong to structured trivia and are not
    /// visited
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var syntaxRoot = context.Tree.GetRoot(context.CancellationToken);

        foreach (var trivia in syntaxRoot.DescendantTrivia())
        {
            if (CommentPositionUtilities.IsOrdinaryComment(trivia)
                && CommentPositionUtilities.TryGetSurroundingTokens(trivia, out var previousToken, out var nextToken)
                && IsMisplaced(trivia, previousToken, nextToken))
            {
                foreach (var location in CommentPositionUtilities.GetCommentLocations(trivia))
                {
                    context.ReportDiagnostic(CreateDiagnostic(location));
                }
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