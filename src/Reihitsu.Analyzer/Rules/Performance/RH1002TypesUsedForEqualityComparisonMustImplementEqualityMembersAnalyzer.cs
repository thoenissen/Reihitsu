using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

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
    /// Simple method names of the operations that compare a type for equality, used as a cheap syntactic
    /// pre-filter before performing the more expensive semantic binding.
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
                                                                         nameof(Enumerable.Contains),
                                                                         nameof(Enumerable.SequenceEqual),
                                                                         nameof(ImmutableHashSet.ToImmutableHashSet),
                                                                         nameof(ImmutableDictionary.ToImmutableDictionary),
                                                                         nameof(FrozenSet.ToFrozenSet),
                                                                         nameof(FrozenDictionary.ToFrozenDictionary)
                                                                     }.ToFrozenSet();

    /// <summary>
    /// Simple method names that are only relevant when declared on <see cref="Enumerable"/>. Another relevant
    /// static class can declare a method of the same name with a different meaning, such as
    /// <c>ImmutableDictionary.Contains(map, key, value)</c>, which is a key lookup through the map's own comparer
    /// </summary>
    private static readonly FrozenSet<string> _enumerableOnlyMethodNames = new[]
                                                                           {
                                                                               nameof(Enumerable.Contains),
                                                                               nameof(Enumerable.SequenceEqual)
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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Should the diagnostic by reported?</returns>
    private static bool CheckIfDiagnosticShouldBeReported(SemanticModel semanticModel, InvocationExpressionSyntax invocationExpression, IMethodSymbol methodSymbol, CancellationToken cancellationToken)
    {
        if (IsRelevantContainingType(semanticModel.Compilation, methodSymbol.ContainingType) == false)
        {
            return false;
        }

        if (_enumerableOnlyMethodNames.Contains(methodSymbol.Name)
            && SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, semanticModel.Compilation.GetTypeByMetadataName("System.Linq.Enumerable")) == false)
        {
            return false;
        }

        if (HasExplicitEqualityComparerArgument(semanticModel, invocationExpression, methodSymbol))
        {
            return false;
        }

        return GetEqualityComparisonType(methodSymbol) is { TypeKind: TypeKind.Struct } comparisonType
               && AreEqualityMembersImplemented(semanticModel.Compilation, comparisonType) == false
               && IsDelegatedToDictionaryKeyLookup(semanticModel, invocationExpression, methodSymbol, comparisonType, cancellationToken) == false;
    }

    /// <summary>
    /// Determines whether the invocation is the comparer-less <c>Enumerable.Contains(source, value)</c> overload
    /// on a source whose static type is a dictionary of the compared <see cref="KeyValuePair{TKey, TValue}"/>.
    /// That overload delegates to the source's own <c>ICollection&lt;T&gt;.Contains</c>, which a dictionary answers
    /// with a key lookup, so the <see cref="KeyValuePair{TKey, TValue}"/> equality members are never used.
    /// A source that converts to <see cref="IDictionary{TKey, TValue}"/> is always such a collection. A source that
    /// only converts to <see cref="IReadOnlyDictionary{TKey, TValue}"/> is exempt only when that conversion comes
    /// from an interface — the static type itself, or an interface constraint of a type parameter — whose runtime
    /// implementations are dictionaries that delegate; a concrete type implementing only the read-only interface,
    /// directly or as a type parameter's class constraint, is not a collection and is scanned with the default comparer.
    /// The overload that takes a comparer does not delegate, and a source whose static type is only a key/value
    /// pair sequence carries no dictionary semantics, so both stay checked
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="invocationExpression">Invocation expression</param>
    /// <param name="methodSymbol">Method symbol</param>
    /// <param name="comparisonType">Type used for equality comparison</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the invocation is answered by the dictionary's key lookup</returns>
    private static bool IsDelegatedToDictionaryKeyLookup(SemanticModel semanticModel, InvocationExpressionSyntax invocationExpression, IMethodSymbol methodSymbol, ITypeSymbol comparisonType, CancellationToken cancellationToken)
    {
        var compilation = semanticModel.Compilation;
        var unreducedMethod = methodSymbol.ReducedFrom ?? methodSymbol;

        if (unreducedMethod.Name != nameof(Enumerable.Contains)
            || unreducedMethod.Parameters.Length != 2
            || comparisonType is not INamedTypeSymbol { TypeArguments.Length: 2 } keyValuePairType
            || SymbolEqualityComparer.Default.Equals(keyValuePairType.OriginalDefinition, compilation.GetTypeByMetadataName("System.Collections.Generic.KeyValuePair`2")) == false)
        {
            return false;
        }

        if (semanticModel.GetOperation(invocationExpression, cancellationToken) is not IInvocationOperation invocationOperation)
        {
            return false;
        }

        var sourceArgument = invocationOperation.Arguments.FirstOrDefault(static argument => argument.Parameter?.Ordinal == 0);
        var sourceType = UnwrapImplicitConversions(sourceArgument?.Value)?.Type;

        if (sourceType == null)
        {
            return false;
        }

        if (IsImplicitlyConvertibleToDictionary(compilation, sourceType, "System.Collections.Generic.IDictionary`2", keyValuePairType.TypeArguments))
        {
            return true;
        }

        return IsReadOnlyDictionaryInterface(compilation, sourceType, keyValuePairType.TypeArguments);
    }

    /// <summary>
    /// Determines whether the type is an interface that converts to <see cref="IReadOnlyDictionary{TKey, TValue}"/>,
    /// or a type parameter with such an interface among its constraints, followed through nested type parameters.
    /// A class constraint is not followed, because a concrete type implementing only the read-only interface does
    /// not delegate the lookup
    /// </summary>
    /// <param name="compilation">Compilation</param>
    /// <param name="type">Type to check</param>
    /// <param name="keyAndValueTypes">Key and value type arguments</param>
    /// <returns><see langword="true"/> if the type is, or is constrained to, a read-only dictionary interface</returns>
    private static bool IsReadOnlyDictionaryInterface(Compilation compilation, ITypeSymbol type, ImmutableArray<ITypeSymbol> keyAndValueTypes)
    {
        return type switch
               {
                   ITypeParameterSymbol typeParameter => typeParameter.ConstraintTypes.Any(constraintType => IsReadOnlyDictionaryInterface(compilation, constraintType, keyAndValueTypes)),
                   { TypeKind: TypeKind.Interface } => IsImplicitlyConvertibleToDictionary(compilation, type, "System.Collections.Generic.IReadOnlyDictionary`2", keyAndValueTypes),
                   _ => false
               };
    }

    /// <summary>
    /// Removes the implicit conversions the compiler inserted around an operation, so that the operand's own
    /// static type is visible. An explicit conversion written in source is kept
    /// </summary>
    /// <param name="operation">Operation</param>
    /// <returns>The operation without its implicit conversions</returns>
    private static IOperation UnwrapImplicitConversions(IOperation operation)
    {
        while (operation is IConversionOperation { IsImplicit: true } conversionOperation)
        {
            operation = conversionOperation.Operand;
        }

        return operation;
    }

    /// <summary>
    /// Determines whether the type converts implicitly, without a user-defined conversion, to the given dictionary
    /// interface constructed with the key and value types. A conversion rather than symbol identity is checked, so
    /// that type parameters constrained to the interface and tuple types differing only in element names match
    /// </summary>
    /// <param name="compilation">Compilation</param>
    /// <param name="type">Type to check</param>
    /// <param name="dictionaryInterfaceName">Fully qualified metadata name of the generic dictionary interface</param>
    /// <param name="keyAndValueTypes">Key and value type arguments</param>
    /// <returns><see langword="true"/> if the type converts to the constructed dictionary interface</returns>
    private static bool IsImplicitlyConvertibleToDictionary(Compilation compilation, ITypeSymbol type, string dictionaryInterfaceName, ImmutableArray<ITypeSymbol> keyAndValueTypes)
    {
        if (compilation.GetTypeByMetadataName(dictionaryInterfaceName) is not { } dictionaryInterface)
        {
            return false;
        }

        var conversion = compilation.ClassifyCommonConversion(type, dictionaryInterface.Construct(keyAndValueTypes[0], keyAndValueTypes[1]));

        return conversion.Exists
               && conversion.IsImplicit
               && conversion.IsUserDefined == false;
    }

    /// <summary>
    /// Is the method declared on one of the static classes this rule inspects?
    /// </summary>
    /// <param name="compilation">Compilation</param>
    /// <param name="containingType">Containing type of the invoked method</param>
    /// <returns><see langword="true"/> if the containing type is relevant to this rule</returns>
    private static bool IsRelevantContainingType(Compilation compilation, INamedTypeSymbol containingType)
    {
        return Array.Exists(_relevantContainingTypes, typeName => SymbolEqualityComparer.Default.Equals(containingType, compilation.GetTypeByMetadataName(typeName)));
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
        return methodSymbol.Parameters.FirstOrDefault(static parameter => parameter.Name is "keySelector" or "outerKeySelector");
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

        if (context.SemanticModel.GetSymbolInfo(invocationExpression, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (CheckIfDiagnosticShouldBeReported(context.SemanticModel, invocationExpression, methodSymbol, context.CancellationToken))
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