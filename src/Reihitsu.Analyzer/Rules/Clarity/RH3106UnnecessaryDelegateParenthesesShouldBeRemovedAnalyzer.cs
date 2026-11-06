using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3106: Unnecessary delegate parentheses should be removed
/// </summary>
[Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer(Microsoft.CodeAnalysis.LanguageNames.CSharp)]
public class RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer : EmptyParenthesesAnalyzerBase<AnonymousMethodExpressionSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3106";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3106Title), nameof(AnalyzerResources.RH3106MessageFormat), SyntaxKind.AnonymousMethodExpression)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the expression takes its natural type: no delegate type is its target, and no overload
    /// resolution or type inference chooses one. Without a parameter list, an anonymous method has no natural type, so
    /// the list cannot be removed there. That is the case for the initializer of an implicitly typed local, for an
    /// element of an implicitly typed array creation, for the expression a lambda returns when that lambda takes its
    /// natural type itself, for a cast to a type that is not a delegate type, for a conversion to a type that is not a
    /// delegate type, such as <see langword="object"/> or <see cref="System.Delegate"/>, and for an argument of such a
    /// parameter type when no other overload is a candidate for the call and no type argument is inferred. An expression
    /// inside parentheses, in a branch of a conditional expression, or in an arm of a switch expression stands in the
    /// position of that expression. Where another branch, arm, or element gives the surrounding expression a delegate
    /// type, that delegate type is the expression's target in every position, also under <see langword="var"/>
    /// </summary>
    /// <param name="node">Anonymous method, or a lambda that returns one</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the expression takes its natural type</returns>
    private static bool TakesNaturalType(ExpressionSyntax node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (HasDelegateTypedNeighbor(node, semanticModel, cancellationToken))
        {
            return false;
        }

        var expression = GetPositionExpression(node);

        switch (expression.Parent)
        {
            case EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Type.IsVar: true } } }:
            case InitializerExpressionSyntax { Parent: ImplicitArrayCreationExpressionSyntax }:
                return true;

            case CastExpressionSyntax castExpression:
                return IsDelegateType(semanticModel.GetTypeInfo(castExpression.Type, cancellationToken).Type) == false;

            case LambdaExpressionSyntax lambda when lambda.ExpressionBody == expression:
                return TakesNaturalType(lambda, semanticModel, cancellationToken);

            case ReturnStatementSyntax returnStatement when returnStatement.Ancestors().FirstOrDefault(static ancestor => ancestor is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax or BaseMethodDeclarationSyntax or AccessorDeclarationSyntax) is LambdaExpressionSyntax lambda:
                return TakesNaturalType(lambda, semanticModel, cancellationToken);
        }

        if (IsDelegateType(semanticModel.GetTypeInfo(expression, cancellationToken).ConvertedType))
        {
            return false;
        }

        return expression.Parent is not ArgumentSyntax { Parent: BaseArgumentListSyntax { Parent: { } call } }
               || HasCompetingCandidates(call, semanticModel, cancellationToken) == false;
    }

    /// <summary>
    /// Gets the expression whose position the expression stands in: the outermost parenthesized expression, conditional
    /// expression whose branch it is, or switch expression whose arm it is
    /// </summary>
    /// <param name="expression">Expression</param>
    /// <returns>The expression whose position decides the target</returns>
    private static ExpressionSyntax GetPositionExpression(ExpressionSyntax expression)
    {
        while (true)
        {
            switch (expression.Parent)
            {
                case ParenthesizedExpressionSyntax parenthesizedExpression:
                    expression = parenthesizedExpression;
                    break;

                case ConditionalExpressionSyntax conditionalExpression when conditionalExpression.Condition != expression:
                    expression = conditionalExpression;
                    break;

                case SwitchExpressionArmSyntax { Parent: SwitchExpressionSyntax switchExpression } switchExpressionArm when switchExpressionArm.Expression == expression:
                    expression = switchExpression;
                    break;

                default:
                    return expression;
            }
        }
    }

    /// <summary>
    /// Determines whether an expression next to the expression gives it a delegate type as its target: another branch of
    /// a conditional expression, another arm of a switch expression, or another element of an implicitly typed array
    /// creation, which is not an anonymous function and whose type is a delegate type. The surrounding expression then
    /// takes that delegate type, and the expression converts to it. Parentheses, conditional branches, and switch arms
    /// are followed as in <see cref="GetPositionExpression"/>
    /// </summary>
    /// <param name="expression">Expression</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if a neighboring expression gives the expression a delegate type</returns>
    private static bool HasDelegateTypedNeighbor(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        while (true)
        {
            switch (expression.Parent)
            {
                case ParenthesizedExpressionSyntax parenthesizedExpression:
                    {
                        expression = parenthesizedExpression;
                    }
                    break;

                case ConditionalExpressionSyntax conditionalExpression when conditionalExpression.Condition != expression:
                    {
                        var otherBranch = conditionalExpression.WhenTrue == expression
                                              ? conditionalExpression.WhenFalse
                                              : conditionalExpression.WhenTrue;

                        if (IsDelegateTypedNonAnonymousFunction(otherBranch, semanticModel, cancellationToken))
                        {
                            return true;
                        }

                        expression = conditionalExpression;
                    }
                    break;

                case SwitchExpressionArmSyntax { Parent: SwitchExpressionSyntax switchExpression } switchExpressionArm when switchExpressionArm.Expression == expression:
                    {
                        if (switchExpression.Arms.Any(arm => arm != switchExpressionArm
                                                             && IsDelegateTypedNonAnonymousFunction(arm.Expression, semanticModel, cancellationToken)))
                        {
                            return true;
                        }

                        expression = switchExpression;
                    }
                    break;

                case InitializerExpressionSyntax { Parent: ImplicitArrayCreationExpressionSyntax } initializer:
                    {
                        var element = expression;

                        return initializer.Expressions.Any(otherElement => otherElement != element
                                                                           && IsDelegateTypedNonAnonymousFunction(otherElement, semanticModel, cancellationToken));
                    }

                default:
                    {
                        return false;
                    }
            }
        }
    }

    /// <summary>
    /// Determines whether the expression, inside any parentheses, is not an anonymous function and its type is a
    /// delegate type
    /// </summary>
    /// <param name="expression">Expression</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the expression gives a delegate type</returns>
    private static bool IsDelegateTypedNonAnonymousFunction(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            expression = parenthesizedExpression.Expression;
        }

        return expression is not AnonymousFunctionExpressionSyntax
               && IsDelegateType(semanticModel.GetTypeInfo(expression, cancellationToken).Type);
    }

    /// <summary>
    /// Determines whether the type is a delegate type
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns><see langword="true"/> for a delegate type</returns>
    private static bool IsDelegateType(ITypeSymbol type)
    {
        return type?.TypeKind == TypeKind.Delegate;
    }

    /// <summary>
    /// Determines whether overload resolution or type inference takes part in choosing the target of the call: another
    /// overload is a candidate for it, or the bound method is generic. A call that binds to nothing counts as such
    /// </summary>
    /// <param name="call">Invocation, object creation, constructor initializer, or element access</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the binding of the call can depend on the argument</returns>
    private static bool HasCompetingCandidates(SyntaxNode call, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var symbol = semanticModel.GetSymbolInfo(call, cancellationToken).Symbol;

        switch (symbol)
        {
            case IMethodSymbol { IsGenericMethod: true }:
                return true;

            case IMethodSymbol { MethodKind: MethodKind.Constructor } constructor:
                return constructor.ContainingType.InstanceConstructors.Length > 1;

            case IMethodSymbol when call is InvocationExpressionSyntax invocation:
                return semanticModel.GetMemberGroup(invocation.Expression, cancellationToken).Length > 1;

            case IPropertySymbol { IsIndexer: true } indexer:
                return indexer.ContainingType.GetMembers(indexer.Name).Length > 1;

            default:
                return true;
        }
    }

    #endregion // Methods

    #region EmptyParenthesesAnalyzerBase

    /// <inheritdoc/>
    protected override bool HasUnnecessaryParentheses(AnonymousMethodExpressionSyntax node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return node.ParameterList is { Parameters.Count: 0 }
               && TakesNaturalType(node, semanticModel, cancellationToken) == false;
    }

    /// <inheritdoc/>
    protected override Location GetDiagnosticLocation(AnonymousMethodExpressionSyntax node)
    {
        return node.ParameterList?.GetLocation() ?? node.GetLocation();
    }

    #endregion // EmptyParenthesesAnalyzerBase
}