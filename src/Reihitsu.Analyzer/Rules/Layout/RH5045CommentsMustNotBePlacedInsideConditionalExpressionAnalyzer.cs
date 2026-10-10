using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5045: Comments must not be placed inside a conditional expression
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5045CommentsMustNotBePlacedInsideConditionalExpressionAnalyzer : CommentPositionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5045";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5045CommentsMustNotBePlacedInsideConditionalExpressionAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5045Title), nameof(AnalyzerResources.RH5045MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the token is the <c>?</c> or the <c>:</c> of a conditional expression
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token is a conditional operator</returns>
    private static bool IsConditionalOperator(SyntaxToken token)
    {
        return token.Parent is ConditionalExpressionSyntax conditionalExpression
               && (conditionalExpression.QuestionToken == token
                   || conditionalExpression.ColonToken == token);
    }

    #endregion // Methods

    #region CommentPositionAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsMisplaced(SyntaxTrivia comment, SyntaxToken previousToken, SyntaxToken nextToken)
    {
        return IsConditionalOperator(previousToken)
               || IsConditionalOperator(nextToken);
    }

    #endregion // CommentPositionAnalyzerBase
}