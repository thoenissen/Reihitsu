using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Performance;

/// <summary>
/// RH0503: Use string interpolation instead of string concatenation
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0503UseStringInterpolationInsteadOfStringConcatenationAnalyzer : DiagnosticAnalyzerBase<RH0503UseStringInterpolationInsteadOfStringConcatenationAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0503";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0503UseStringInterpolationInsteadOfStringConcatenationAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Performance, nameof(AnalyzerResources.RH0503Title), nameof(AnalyzerResources.RH0503MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Count the individual segments that make up a concatenation chain
    /// </summary>
    /// <param name="addExpression">Add expression</param>
    /// <returns>Number of concatenation chain segments</returns>
    private static int CountSegments(BinaryExpressionSyntax addExpression)
    {
        return EnumerateChainSegments(addExpression).Count();
    }

    /// <summary>
    /// Determine whether the chain contains a non-constant segment
    /// </summary>
    /// <param name="addExpression">Add expression</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the chain contains a non-constant segment</returns>
    private static bool ContainsNonConstantSegment(BinaryExpressionSyntax addExpression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        foreach (var segment in EnumerateChainSegments(addExpression))
        {
            if (semanticModel.GetConstantValue(segment, cancellationToken).HasValue == false)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Enumerate the individual segments that make up a concatenation chain
    /// </summary>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <returns>Concatenation chain segments</returns>
    private static IEnumerable<ExpressionSyntax> EnumerateChainSegments(ExpressionSyntax expressionSyntax)
    {
        if (expressionSyntax is ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            foreach (var segment in EnumerateChainSegments(parenthesizedExpression.Expression))
            {
                yield return segment;
            }

            yield break;
        }

        if (expressionSyntax is BinaryExpressionSyntax binaryExpression
            && binaryExpression.IsKind(SyntaxKind.AddExpression))
        {
            foreach (var segment in EnumerateChainSegments(binaryExpression.Left))
            {
                yield return segment;
            }

            foreach (var segment in EnumerateChainSegments(binaryExpression.Right))
            {
                yield return segment;
            }

            yield break;
        }

        yield return expressionSyntax;
    }

    /// <summary>
    /// Determine whether the expression is nested inside a larger string concatenation chain
    /// </summary>
    /// <param name="addExpression">Add expression</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the expression is nested inside a larger string concatenation chain</returns>
    private static bool IsNestedInsideLargerStringConcatenationChain(BinaryExpressionSyntax addExpression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        ExpressionSyntax currentExpression = addExpression;

        while (currentExpression.Parent is ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            currentExpression = parenthesizedExpression;
        }

        return currentExpression.Parent is BinaryExpressionSyntax parentBinaryExpression
               && parentBinaryExpression.IsKind(SyntaxKind.AddExpression)
               && IsStringExpression(parentBinaryExpression, semanticModel, cancellationToken);
    }

    /// <summary>
    /// Determine whether the expression resolves to <see cref="string"/>
    /// </summary>
    /// <param name="expressionSyntax">Expression syntax</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the expression resolves to <see cref="string"/></returns>
    private static bool IsStringExpression(ExpressionSyntax expressionSyntax, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var typeInfo = semanticModel.GetTypeInfo(expressionSyntax, cancellationToken);
        var expressionType = typeInfo.ConvertedType ?? typeInfo.Type;

        return expressionType?.SpecialType == SpecialType.System_String;
    }

    /// <summary>
    /// Analyze add expressions
    /// </summary>
    /// <param name="context">Context</param>
    private void OnAddExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BinaryExpressionSyntax addExpression)
        {
            return;
        }

        if (IsNestedInsideLargerStringConcatenationChain(addExpression, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        if (IsStringExpression(addExpression, context.SemanticModel, context.CancellationToken) == false)
        {
            return;
        }

        if (CountSegments(addExpression) < 3)
        {
            return;
        }

        if (context.SemanticModel.GetConstantValue(addExpression, context.CancellationToken).HasValue)
        {
            return;
        }

        if (ContainsNonConstantSegment(addExpression, context.SemanticModel, context.CancellationToken))
        {
            context.ReportDiagnostic(CreateDiagnostic(addExpression.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnAddExpression, SyntaxKind.AddExpression);
    }

    #endregion // DiagnosticAnalyzer
}