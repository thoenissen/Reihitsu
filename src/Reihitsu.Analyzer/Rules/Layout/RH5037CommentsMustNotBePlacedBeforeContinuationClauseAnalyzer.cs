using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5037: Comments must not be placed before a continuation clause
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5037CommentsMustNotBePlacedBeforeContinuationClauseAnalyzer : CommentPositionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5037";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5037CommentsMustNotBePlacedBeforeContinuationClauseAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5037Title), nameof(AnalyzerResources.RH5037MessageFormat))
    {
    }

    #endregion // Constructor

    #region CommentPositionAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsMisplaced(SyntaxTrivia comment, SyntaxToken previousToken, SyntaxToken nextToken)
    {
        return nextToken.Parent switch
               {
                   ElseClauseSyntax elseClause => elseClause.ElseKeyword == nextToken,
                   CatchClauseSyntax catchClause => catchClause.CatchKeyword == nextToken,
                   FinallyClauseSyntax finallyClause => finallyClause.FinallyKeyword == nextToken,
                   DoStatementSyntax doStatement => doStatement.WhileKeyword == nextToken,
                   _ => false
               };
    }

    #endregion // CommentPositionAnalyzerBase
}