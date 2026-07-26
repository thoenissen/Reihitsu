using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Performance;

/// <summary>
/// RH1002: Types used for equality comparison must implement equality members
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer : StructEqualityPerformanceAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH1002";

    /// <summary>
    /// Simple method names that can carry a key type parameter relevant to this rule, used as a cheap
    /// syntactic pre-filter before performing the more expensive semantic binding.
    /// <c>DistinctBy</c>, <c>UnionBy</c>, <c>IntersectBy</c>, <c>ExceptBy</c>, and <c>ToHashSet</c> are not
    /// available on <see cref="Enumerable"/> in this project's netstandard2.0 target, so they are listed as
    /// string literals rather than via <see langword="nameof"/>
    /// </summary>
    private static readonly FrozenSet<string> _relevantMethodNames = new[]
                                                                     {
                                                                         nameof(Enumerable.Distinct),
                                                                         "DistinctBy",
                                                                         nameof(Enumerable.Union),
                                                                         "UnionBy",
                                                                         nameof(Enumerable.Intersect),
                                                                         "IntersectBy",
                                                                         nameof(Enumerable.Except),
                                                                         "ExceptBy",
                                                                         "ToHashSet",
                                                                         nameof(Enumerable.ToLookup),
                                                                         nameof(Enumerable.ToDictionary),
                                                                         nameof(Enumerable.GroupBy),
                                                                         nameof(Enumerable.Join),
                                                                         nameof(Enumerable.GroupJoin),
                                                                         nameof(ImmutableHashSet.ToImmutableHashSet),
                                                                         nameof(ImmutableDictionary.ToImmutableDictionary),
                                                                         nameof(FrozenSet.ToFrozenSet),
                                                                         nameof(FrozenDictionary.ToFrozenDictionary)
                                                                     }.ToFrozenSet();

    /// <summary>
    /// Fully qualified metadata names of the static classes whose methods this rule inspects
    /// </summary>
    private static readonly string[] _relevantContainingTypes = [
                                                                    "System.Linq.Enumerable",
                                                                    "System.Collections.Frozen.FrozenDictionary",
                                                                    "System.Collections.Frozen.FrozenSet",
                                                                    "System.Collections.Immutable.ImmutableDictionary",
                                                                    "System.Collections.Immutable.ImmutableHashSet"
                                                                ];

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Performance, nameof(AnalyzerResources.RH1002Title), nameof(AnalyzerResources.RH1002MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the invocation syntactically calls one of the relevant methods
    /// </summary>
    /// <param name="invocationExpression">Invocation expression</param>
    /// <returns><see langword="true"/> if the called method name is relevant to this rule</returns>
    private static bool IsRelevantMethodName(InvocationExpressionSyntax invocationExpression)
    {
        var name = invocationExpression.Expression switch
                   {
                       MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
                       MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Identifier.ValueText,
                       IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
                       GenericNameSyntax genericName => genericName.Identifier.ValueText,
                       _ => null
                   };

        return name != null && _relevantMethodNames.Contains(name);
    }

    /// <summary>
    /// Check if the diagnostic should be reported
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="invocationExpression">Invocation expression</param>
    /// <param name="methodSymbol">Method symbol</param>
    /// <returns>Should the diagnostic by reported?</returns>
    private static bool CheckIfDiagnosticShouldBeReported(SemanticModel semanticModel, InvocationExpressionSyntax invocationExpression, IMethodSymbol methodSymbol)
    {
        if (IsRelevantContainingType(semanticModel.Compilation, methodSymbol.ContainingType) == false)
        {
            return false;
        }

        if (HasExplicitEqualityComparerArgument(semanticModel, invocationExpression, methodSymbol))
        {
            return false;
        }

        return GetEqualityComparisonType(methodSymbol) is { TypeKind: TypeKind.Struct } comparisonType
               && AreEqualityMembersImplemented(semanticModel.Compilation, comparisonType) == false;
    }

    /// <summary>
    /// Is the method declared on one of the static classes this rule inspects?
    /// </summary>
    /// <param name="compilation">Compilation</param>
    /// <param name="containingType">Containing type of the invoked method</param>
    /// <returns><see langword="true"/> if the containing type is relevant to this rule</returns>
    private static bool IsRelevantContainingType(Compilation compilation, INamedTypeSymbol containingType)
    {
        foreach (var typeName in _relevantContainingTypes)
        {
            if (SymbolEqualityComparer.Default.Equals(containingType, compilation.GetTypeByMetadataName(typeName)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Resolves the type that the bound overload actually uses for equality comparison: the key-selector's
    /// return type when the overload has one, otherwise the method's first type argument (the source element
    /// type for plain overloads, or the key type itself for the <see cref="KeyValuePair{TKey, TValue}"/>-sourced
    /// overloads, whose type parameter list starts with the key rather than the source)
    /// </summary>
    /// <param name="methodSymbol">Method</param>
    /// <returns>The type used for equality comparison, or <see langword="null"/> if it could not be determined</returns>
    private static ITypeSymbol GetEqualityComparisonType(IMethodSymbol methodSymbol)
    {
        if (FindKeySelectorParameter(methodSymbol) is { Type: INamedTypeSymbol { TypeArguments.Length: 2 } selectorType })
        {
            return selectorType.TypeArguments[1];
        }

        return methodSymbol.TypeArguments.Length > 0
                   ? methodSymbol.TypeArguments[0]
                   : null;
    }

    /// <summary>
    /// Finds the key-selector parameter of the bound overload, if it has one. The BCL uses the parameter name
    /// <c>keySelector</c> for every relevant method, except <c>Enumerable.Join</c>/<c>Enumerable.GroupJoin</c>
    /// which name it <c>outerKeySelector</c>
    /// </summary>
    /// <param name="methodSymbol">Method</param>
    /// <returns>The key-selector parameter, or <see langword="null"/> if the overload does not have one</returns>
    private static IParameterSymbol FindKeySelectorParameter(IMethodSymbol methodSymbol)
    {
        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.Name is "keySelector" or "outerKeySelector")
            {
                return parameter;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether the invocation passes an explicit, non-<see langword="null"/> <see cref="IEqualityComparer{T}"/>
    /// argument. Such an argument bypasses the compared type's own equality members, so the overload is exempt
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="invocationExpression">Invocation expression</param>
    /// <param name="methodSymbol">Method symbol</param>
    /// <returns><see langword="true"/> if a custom equality comparer is explicitly supplied</returns>
    private static bool HasExplicitEqualityComparerArgument(SemanticModel semanticModel, InvocationExpressionSyntax invocationExpression, IMethodSymbol methodSymbol)
    {
        return EqualityComparerArgumentUtilities.HasExplicitEqualityComparerArgument(semanticModel, invocationExpression.ArgumentList, methodSymbol.Parameters);
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.InvocationExpression"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocationExpression)
        {
            return;
        }

        // Cheap syntactic pre-filter before the semantic binding below
        if (IsRelevantMethodName(invocationExpression) == false)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (CheckIfDiagnosticShouldBeReported(context.SemanticModel, invocationExpression, methodSymbol))
        {
            var location = invocationExpression.Expression switch
                           {
                               MemberAccessExpressionSyntax memberAccess => memberAccess.Name.GetLocation(),
                               IdentifierNameSyntax identifierName => identifierName.GetLocation(),
                               _ => invocationExpression.GetLocation()
                           };

            context.ReportDiagnostic(CreateDiagnostic(location));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnInvocationExpression, SyntaxKind.InvocationExpression);
    }

    #endregion // DiagnosticAnalyzer
}