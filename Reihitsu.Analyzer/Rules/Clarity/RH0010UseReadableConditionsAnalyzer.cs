using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH0010: Use readable conditions.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0010UseReadableConditionsAnalyzer : DiagnosticAnalyzerBase<RH0010UseReadableConditionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0010";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0010UseReadableConditionsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH0010Title), nameof(AnalyzerResources.RH0010MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the expression is constant-like.
    /// </summary>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the expression is constant-like</returns>
    private static bool IsConstantLike(ExpressionSyntax expressionSyntax, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (expressionSyntax is DefaultExpressionSyntax
                             or LiteralExpressionSyntax)
        {
            return true;
        }

        if (semanticModel.GetConstantValue(expressionSyntax, cancellationToken).HasValue)
        {
            return true;
        }

        return semanticModel.GetSymbolInfo(expressionSyntax, cancellationToken).Symbol is IFieldSymbol { IsConst: true }
                                                                                       or IFieldSymbol { IsStatic: true, IsReadOnly: true };
    }

    /// <summary>
    /// Analyze comparison expressions.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BinaryExpressionSyntax binaryExpression)
        {
            return;
        }

        if (IsConstantLike(binaryExpression.Left, context.SemanticModel, context.CancellationToken)
            && IsConstantLike(binaryExpression.Right, context.SemanticModel, context.CancellationToken) == false)
        {
            context.ReportDiagnostic(CreateDiagnostic(binaryExpression.OperatorToken.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnBinaryExpression,
                                         SyntaxKind.EqualsExpression,
                                         SyntaxKind.NotEqualsExpression,
                                         SyntaxKind.LessThanExpression,
                                         SyntaxKind.GreaterThanExpression,
                                         SyntaxKind.LessThanOrEqualExpression,
                                         SyntaxKind.GreaterThanOrEqualExpression);
    }

    #endregion // DiagnosticAnalyzer
}