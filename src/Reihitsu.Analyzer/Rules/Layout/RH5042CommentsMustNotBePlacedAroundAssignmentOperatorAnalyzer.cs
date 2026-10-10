using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5042: Comments must not be placed around an assignment operator
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer : CommentPositionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5042";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5042Title), nameof(AnalyzerResources.RH5042MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the token is the operator of an assignment or of an initializing <c>=</c>
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token is an assignment operator</returns>
    private static bool IsAssignmentOperator(SyntaxToken token)
    {
        return token.Parent switch
               {
                   EqualsValueClauseSyntax equalsValueClause => equalsValueClause.EqualsToken == token,
                   AssignmentExpressionSyntax assignmentExpression => assignmentExpression.OperatorToken == token,
                   _ => false
               };
    }

    #endregion // Methods

    #region CommentPositionAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsMisplaced(SyntaxTrivia comment, SyntaxToken previousToken, SyntaxToken nextToken)
    {
        return IsAssignmentOperator(previousToken)
               || IsAssignmentOperator(nextToken);
    }

    #endregion // CommentPositionAnalyzerBase
}