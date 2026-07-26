using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    /// <param name="compilation">Compilation</param>
    /// <param name="argumentList">Argument list</param>
    /// <param name="parameters">Parameters of the bound method or constructor</param>
    /// <returns><see langword="true"/> if a custom equality comparer is explicitly supplied</returns>
    internal static bool HasExplicitEqualityComparerArgument(Compilation compilation, ArgumentListSyntax argumentList, ImmutableArray<IParameterSymbol> parameters)
    {
        var comparerType = compilation.GetTypeByMetadataName("System.Collections.Generic.IEqualityComparer`1")?.ConstructUnboundGenericType();

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
                && IsNullLikeExpression(argument.Expression) == false)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether an expression is <see langword="null"/> or functionally equivalent to it for a
    /// reference-typed parameter: the <see langword="null"/> literal, target-typed <see langword="default"/>, or
    /// an explicit <c>default(T)</c>
    /// </summary>
    /// <param name="expression">Expression</param>
    /// <returns><see langword="true"/> if the expression is null-like</returns>
    private static bool IsNullLikeExpression(ExpressionSyntax expression)
    {
        return expression.IsKind(SyntaxKind.NullLiteralExpression)
               || expression.IsKind(SyntaxKind.DefaultLiteralExpression)
               || expression.IsKind(SyntaxKind.DefaultExpression);
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