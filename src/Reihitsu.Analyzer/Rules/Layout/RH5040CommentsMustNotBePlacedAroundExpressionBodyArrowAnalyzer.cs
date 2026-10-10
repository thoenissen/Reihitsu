using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5040: Comments must not be placed around the arrow of an expression-bodied member
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5040CommentsMustNotBePlacedAroundExpressionBodyArrowAnalyzer : CommentPositionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5040";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5040CommentsMustNotBePlacedAroundExpressionBodyArrowAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5040Title), nameof(AnalyzerResources.RH5040MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the token is the arrow of an expression-bodied member or accessor
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token is such an arrow</returns>
    private static bool IsMemberArrow(SyntaxToken token)
    {
        return token.Parent is ArrowExpressionClauseSyntax arrowExpressionClause
               && arrowExpressionClause.ArrowToken == token
               && arrowExpressionClause.Parent is MemberDeclarationSyntax or AccessorDeclarationSyntax;
    }

    #endregion // Methods

    #region CommentPositionAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsMisplaced(SyntaxTrivia comment, SyntaxToken previousToken, SyntaxToken nextToken)
    {
        return IsMemberArrow(previousToken)
               || IsMemberArrow(nextToken);
    }

    #endregion // CommentPositionAnalyzerBase
}