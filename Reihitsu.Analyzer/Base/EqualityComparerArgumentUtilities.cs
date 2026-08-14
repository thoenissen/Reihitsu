using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Utilities for detecting explicitly supplied equality comparers
/// </summary>
internal static class EqualityComparerArgumentUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether an argument list passes an explicit, non-<see langword="null"/>
    /// <see cref="System.Collections.Generic.IEqualityComparer{T}"/> argument
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="argumentList">Argument list</param>
    /// <param name="parameters">Parameters of the bound method or constructor</param>
    /// <returns><see langword="true"/> if a custom equality comparer is explicitly supplied</returns>
    internal static bool HasExplicitEqualityComparerArgument(SemanticModel semanticModel, ArgumentListSyntax argumentList, ImmutableArray<IParameterSymbol> parameters)
    {
        var comparerType = semanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.IEqualityComparer`1")?.ConstructUnboundGenericType();

        if (comparerType == null)
        {
            return false;
        }

        var positionalIndex = 0;

        foreach (var argument in argumentList.Arguments)
        {
            IParameterSymbol parameter;

            if (argument.NameColon != null)
            {
                parameter = FindParameterByName(parameters, argument.NameColon.Name.Identifier.ValueText);
            }
            else
            {
                parameter = positionalIndex < parameters.Length
                                ? parameters[positionalIndex]
                                : null;
            }

            // A named argument in its natural position still occupies that ordinal slot (C# 7.2+ non-trailing
            // named arguments), so every argument advances the positional count, named or not
            positionalIndex++;

            if (parameter is { Type: INamedTypeSymbol { IsGenericType: true } parameterType }
                && SymbolEqualityComparer.Default.Equals(parameterType.ConstructUnboundGenericType(), comparerType)
                && IsDefinitelyNonCustomEqualityComparerExpression(semanticModel, argument.Expression) == false)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether an expression can only produce <see langword="null"/> or the framework's
    /// <c>EqualityComparer&lt;T&gt;.Default</c>, neither of which bypasses the compared type's equality behavior
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="expression">Expression</param>
    /// <returns><see langword="true"/> if every represented comparer value is non-custom</returns>
    private static bool IsDefinitelyNonCustomEqualityComparerExpression(SemanticModel semanticModel, ExpressionSyntax expression)
    {
        var equalityComparerType = semanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.EqualityComparer`1")?.ConstructUnboundGenericType();

        return IsNullLikeExpression(semanticModel, expression)
               || (equalityComparerType != null
                   && (IsFrameworkDefaultEqualityComparerExpression(semanticModel, expression)
                       || IsDefinitelyNonCustomEqualityComparerOperation(semanticModel.GetOperation(expression), equalityComparerType)));
    }

    /// <summary>
    /// Determines whether an operation can only produce <see langword="null"/> or the framework's default equality
    /// comparer, including when different branches produce different non-custom values
    /// </summary>
    /// <param name="operation">Operation</param>
    /// <param name="equalityComparerType">Unbound <c>EqualityComparer&lt;T&gt;</c> type</param>
    /// <returns><see langword="true"/> if every represented comparer value is non-custom</returns>
    private static bool IsDefinitelyNonCustomEqualityComparerOperation(IOperation operation, INamedTypeSymbol equalityComparerType)
    {
        if (IsNullLikeOperation(operation)
            || IsFrameworkDefaultEqualityComparerOperation(operation, equalityComparerType))
        {
            return true;
        }

        operation = UnwrapOperation(operation);

        return operation switch
               {
                   ICoalesceOperation coalesce => IsDefinitelyNonCustomEqualityComparerOperation(coalesce.Value, equalityComparerType)
                                                  && IsDefinitelyNonCustomEqualityComparerOperation(coalesce.WhenNull, equalityComparerType),
                   IConditionalOperation conditional => IsDefinitelyNonCustomConditional(conditional, equalityComparerType),
                   ISwitchExpressionOperation { Arms.Length: > 0 } switchExpression => switchExpression.Arms.All(arm => IsDefinitelyNonCustomEqualityComparerOperation(arm.Value, equalityComparerType)),
                   IConditionalAccessOperation conditionalAccess => IsNullLikeOperation(conditionalAccess.Operation)
                                                                    || IsDefinitelyNonCustomEqualityComparerOperation(conditionalAccess.WhenNotNull, equalityComparerType),
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether every reachable branch of a conditional operation is non-custom
    /// </summary>
    /// <param name="conditional">Conditional operation</param>
    /// <param name="equalityComparerType">Unbound <c>EqualityComparer&lt;T&gt;</c> type</param>
    /// <returns><see langword="true"/> when every reachable branch is non-custom; otherwise, <see langword="false"/></returns>
    private static bool IsDefinitelyNonCustomConditional(IConditionalOperation conditional, INamedTypeSymbol equalityComparerType)
    {
        if (conditional.Condition.ConstantValue is { HasValue: true, Value: bool conditionValue })
        {
            var reachableBranch = conditionValue ? conditional.WhenTrue : conditional.WhenFalse;

            return IsDefinitelyNonCustomEqualityComparerOperation(reachableBranch, equalityComparerType);
        }

        return IsDefinitelyNonCustomEqualityComparerOperation(conditional.WhenTrue, equalityComparerType)
               && IsDefinitelyNonCustomEqualityComparerOperation(conditional.WhenFalse, equalityComparerType);
    }

    /// <summary>
    /// Determines whether an expression evaluates to <see langword="null"/>, including through parentheses,
    /// built-in conversions, null-forgiving syntax, or a default expression
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="expression">Expression</param>
    /// <returns><see langword="true"/> if the expression is null-like</returns>
    private static bool IsNullLikeExpression(SemanticModel semanticModel, ExpressionSyntax expression)
    {
        var constantValue = semanticModel.GetConstantValue(expression);

        if (constantValue.HasValue)
        {
            return constantValue.Value == null;
        }

        return IsNullLikeOperation(semanticModel.GetOperation(expression))
               || expression.IsKind(SyntaxKind.NullLiteralExpression)
               || IsNullLikeDefaultExpression(semanticModel, expression);
    }

    /// <summary>
    /// Determines whether an operation evaluates to <see langword="null"/> after unwrapping semantics-preserving
    /// parentheses and built-in conversions
    /// </summary>
    /// <param name="operation">Operation</param>
    /// <returns><see langword="true"/> if the operation is null-like</returns>
    private static bool IsNullLikeOperation(IOperation operation)
    {
        operation = UnwrapOperation(operation);

        if (operation == null)
        {
            return false;
        }

        if (operation.ConstantValue.HasValue)
        {
            return operation.ConstantValue.Value == null;
        }

        return operation switch
               {
                   IDefaultValueOperation defaultValue => IsNullLikeDefaultType(defaultValue.Type),
                   ICoalesceOperation coalesce => IsNullLikeOperation(coalesce.Value) && IsNullLikeOperation(coalesce.WhenNull),
                   IConditionalOperation conditional => IsNullLikeConditional(conditional),
                   ISwitchExpressionOperation { Arms.Length: > 0 } switchExpression => switchExpression.Arms.All(static arm => IsNullLikeOperation(arm.Value)),
                   IConditionalAccessOperation conditionalAccess => IsNullLikeOperation(conditionalAccess.Operation)
                                                                    || IsNullLikeOperation(conditionalAccess.WhenNotNull),
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether every reachable branch of a conditional operation is null-like
    /// </summary>
    /// <param name="conditional">Conditional operation</param>
    /// <returns><see langword="true"/> when every reachable branch is null-like; otherwise, <see langword="false"/></returns>
    private static bool IsNullLikeConditional(IConditionalOperation conditional)
    {
        if (conditional.Condition.ConstantValue is { HasValue: true, Value: bool conditionValue })
        {
            var reachableBranch = conditionValue ? conditional.WhenTrue : conditional.WhenFalse;

            return IsNullLikeOperation(reachableBranch);
        }

        return IsNullLikeOperation(conditional.WhenTrue) && IsNullLikeOperation(conditional.WhenFalse);
    }

    /// <summary>
    /// Determines whether an expression resolves to the framework's <c>EqualityComparer&lt;T&gt;.Default</c>,
    /// which uses the compared type's own equality behavior and therefore is not a custom comparer
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="expression">Expression</param>
    /// <returns><see langword="true"/> if the expression resolves to the framework default comparer</returns>
    private static bool IsFrameworkDefaultEqualityComparerExpression(SemanticModel semanticModel, ExpressionSyntax expression)
    {
        var equalityComparerType = semanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.EqualityComparer`1")?.ConstructUnboundGenericType();

        if (equalityComparerType == null)
        {
            return false;
        }

        while (true)
        {
            switch (expression)
            {
                case ParenthesizedExpressionSyntax parenthesized:
                    {
                        expression = parenthesized.Expression;
                    }
                    break;

                case PostfixUnaryExpressionSyntax postfixUnary when postfixUnary.IsKind(SyntaxKind.SuppressNullableWarningExpression):
                    {
                        expression = postfixUnary.Operand;
                    }
                    break;

                case CastExpressionSyntax castExpression when semanticModel.GetOperation(castExpression) is IConversionOperation { Conversion.IsUserDefined: false }:
                    {
                        expression = castExpression.Expression;
                    }
                    break;

                default:
                    {
                        return IsFrameworkDefaultEqualityComparerProperty(semanticModel, expression, equalityComparerType)
                               || IsFrameworkDefaultEqualityComparerOperation(semanticModel.GetOperation(expression), equalityComparerType);
                    }
            }
        }
    }

    /// <summary>
    /// Determines whether an expression directly references <c>EqualityComparer&lt;T&gt;.Default</c>
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="expression">Expression</param>
    /// <param name="equalityComparerType">Unbound <c>EqualityComparer&lt;T&gt;</c> type</param>
    /// <returns><see langword="true"/> if the expression is the framework default comparer property</returns>
    private static bool IsFrameworkDefaultEqualityComparerProperty(SemanticModel semanticModel, ExpressionSyntax expression, INamedTypeSymbol equalityComparerType)
    {
        return semanticModel.GetSymbolInfo(expression).Symbol is IPropertySymbol
                                                              {
                                                                  IsStatic: true,
                                                                  Name: "Default",
                                                                  ContainingType: { IsGenericType: true } containingType
                                                              }
               && SymbolEqualityComparer.Default.Equals(containingType.ConstructUnboundGenericType(), equalityComparerType);
    }

    /// <summary>
    /// Determines whether an operation necessarily produces the framework's default equality comparer
    /// </summary>
    /// <param name="operation">Operation</param>
    /// <param name="equalityComparerType">Unbound <c>EqualityComparer&lt;T&gt;</c> type</param>
    /// <returns><see langword="true"/> if the operation produces the framework default comparer</returns>
    private static bool IsFrameworkDefaultEqualityComparerOperation(IOperation operation, INamedTypeSymbol equalityComparerType)
    {
        operation = UnwrapOperation(operation);

        return operation switch
               {
                   IPropertyReferenceOperation
                   {
                       Property:
                       {
                           IsStatic: true,
                           Name: "Default",
                           ContainingType: { IsGenericType: true } containingType
                       }
                   } => SymbolEqualityComparer.Default.Equals(containingType.ConstructUnboundGenericType(), equalityComparerType),
                   ICoalesceOperation coalesce => IsFrameworkDefaultEqualityComparerOperation(coalesce.Value, equalityComparerType)
                                                  || (IsNullLikeOperation(coalesce.Value)
                                                      && IsFrameworkDefaultEqualityComparerOperation(coalesce.WhenNull, equalityComparerType)),
                   IConditionalOperation conditional => IsFrameworkDefaultConditional(conditional, equalityComparerType),
                   ISwitchExpressionOperation { Arms.Length: > 0 } switchExpression => switchExpression.Arms.All(arm => IsFrameworkDefaultEqualityComparerOperation(arm.Value, equalityComparerType)),
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether every reachable branch of a conditional operation is the framework default comparer
    /// </summary>
    /// <param name="conditional">Conditional operation</param>
    /// <param name="equalityComparerType">Unbound <c>EqualityComparer&lt;T&gt;</c> type</param>
    /// <returns><see langword="true"/> when every reachable branch is the framework default; otherwise, <see langword="false"/></returns>
    private static bool IsFrameworkDefaultConditional(IConditionalOperation conditional, INamedTypeSymbol equalityComparerType)
    {
        if (conditional.Condition.ConstantValue is { HasValue: true, Value: bool conditionValue })
        {
            var reachableBranch = conditionValue ? conditional.WhenTrue : conditional.WhenFalse;

            return IsFrameworkDefaultEqualityComparerOperation(reachableBranch, equalityComparerType);
        }

        return IsFrameworkDefaultEqualityComparerOperation(conditional.WhenTrue, equalityComparerType)
               && IsFrameworkDefaultEqualityComparerOperation(conditional.WhenFalse, equalityComparerType);
    }

    /// <summary>
    /// Removes parentheses and built-in conversions that do not change comparer semantics
    /// </summary>
    /// <param name="operation">Operation to unwrap</param>
    /// <returns>The first operation whose wrapper can affect comparer semantics</returns>
    private static IOperation UnwrapOperation(IOperation operation)
    {
        while (operation is IConversionOperation { Conversion.IsUserDefined: false }
               || operation is IParenthesizedOperation)
        {
            operation = operation is IConversionOperation conversionOperation
                            ? conversionOperation.Operand
                            : ((IParenthesizedOperation)operation).Operand;
        }

        return operation;
    }

    /// <summary>
    /// Determines whether a default expression produces <see langword="null"/> when converted to the comparer
    /// parameter. A non-nullable value-type comparer is boxed as a real comparer, while reference types,
    /// nullable value types, and types that cannot be resolved conservatively remain null-like
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="expression">Expression</param>
    /// <returns><see langword="true"/> if the default expression is null-like</returns>
    private static bool IsNullLikeDefaultExpression(SemanticModel semanticModel, ExpressionSyntax expression)
    {
        if (expression.IsKind(SyntaxKind.DefaultLiteralExpression) == false
            && expression.IsKind(SyntaxKind.DefaultExpression) == false)
        {
            return false;
        }

        var expressionType = semanticModel.GetTypeInfo(expression).Type;

        return IsNullLikeDefaultType(expressionType);
    }

    /// <summary>
    /// Determines whether the default value of a type produces <see langword="null"/> when converted to the
    /// comparer parameter
    /// </summary>
    /// <param name="type">Type of the default value</param>
    /// <returns><see langword="true"/> if the default value is null-like</returns>
    private static bool IsNullLikeDefaultType(ITypeSymbol type)
    {
        return type == null
               || type.IsValueType == false
               || type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };
    }

    /// <summary>
    /// Finds a parameter by name
    /// </summary>
    /// <param name="parameters">Parameters</param>
    /// <param name="name">Parameter name</param>
    /// <returns>The matching parameter, or <see langword="null"/> if none is found</returns>
    private static IParameterSymbol FindParameterByName(ImmutableArray<IParameterSymbol> parameters, string name)
    {
        foreach (var parameter in parameters)
        {
            if (parameter.Name == name)
            {
                return parameter;
            }
        }

        return null;
    }

    #endregion // Methods
}