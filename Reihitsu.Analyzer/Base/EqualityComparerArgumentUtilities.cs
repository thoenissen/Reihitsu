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
                && IsNullLikeExpression(semanticModel, argument.Expression) == false)
            {
                return true;
            }
        }

        return false;
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
        while (operation != null)
        {
            if (operation.ConstantValue.HasValue)
            {
                return operation.ConstantValue.Value == null;
            }

            switch (operation)
            {
                case IConversionOperation conversion when conversion.Conversion.IsUserDefined == false:
                    {
                        operation = conversion.Operand;
                    }
                    break;

                case IParenthesizedOperation parenthesized:
                    {
                        operation = parenthesized.Operand;
                    }
                    break;

                case IDefaultValueOperation defaultValue:
                    {
                        return IsNullLikeDefaultType(defaultValue.Type);
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        return false;
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