using System.Collections.Frozen;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Performance;

/// <summary>
/// RH0502: Types used for equality comparison must implement equality members
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0502TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer : StructEqualityPerformanceAnalyzerBase<RH0502TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0502";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0502TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Performance, nameof(AnalyzerResources.RH0502Title), nameof(AnalyzerResources.RH0502MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Check if the diagnostic should be reported
    /// </summary>
    /// <param name="compilation">Compilation</param>
    /// <param name="methodSymbol">Method symbol</param>
    /// <returns>Should the diagnostic by reported?</returns>
    private static bool CheckIfDiagnosticShouldBeReported(Compilation compilation, IMethodSymbol methodSymbol)
    {
        var keyIndex = GetKeyIndex(compilation, methodSymbol);

        return keyIndex != -1
               && methodSymbol.TypeParameters.Length > keyIndex
               && methodSymbol.TypeArguments[keyIndex] is { TypeKind: TypeKind.Struct } typeArgument
               && AreEqualityMembersImplemented(compilation, typeArgument) == false;
    }

    /// <summary>
    /// Get the index the key type parameter
    /// </summary>
    /// <param name="compilation">Compilation</param>
    /// <param name="methodSymbol">Method</param>
    /// <returns>Key index</returns>
    private static int GetKeyIndex(Compilation compilation, IMethodSymbol methodSymbol)
    {
        int keyIndex;

        if (SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, compilation.GetTypeByMetadataName("System.Linq.Enumerable")))
        {
            keyIndex = GetKeyIndexEnumerable(methodSymbol);
        }
        else if (SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, compilation.GetTypeByMetadataName("System.Collections.Frozen.FrozenDictionary")))
        {
            keyIndex = GetKeyIndexFrozenDictionary(methodSymbol);
        }
        else if (SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, compilation.GetTypeByMetadataName("System.Collections.Frozen.FrozenSet")))
        {
            keyIndex = GetKeyIndexFrozenSet(methodSymbol);
        }
        else if (SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableDictionary")))
        {
            keyIndex = GetKeyIndexImmutableDictionary(methodSymbol);
        }
        else if (SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableHashSet")))
        {
            keyIndex = GetKeyIndexImmutable(methodSymbol);
        }
        else
        {
            keyIndex = -1;
        }

        return keyIndex;
    }

    /// <summary>
    /// Get the index of the key type parameter for <see cref="Enumerable"/> methods
    /// </summary>
    /// <param name="methodSymbol">Method</param>
    /// <returns>Key index</returns>
    private static int GetKeyIndexEnumerable(IMethodSymbol methodSymbol)
    {
        var keyIndex = -1;

        switch (methodSymbol.Name)
        {
            case nameof(Enumerable.Distinct):
            case nameof(Enumerable.Union):
            case nameof(Enumerable.Intersect):
            case nameof(Enumerable.Except):
                {
                    keyIndex = 0;
                }
                break;

            case nameof(Enumerable.ToLookup):
            case nameof(Enumerable.ToDictionary):
            case nameof(Enumerable.GroupBy):
                {
                    keyIndex = 1;
                }
                break;

            case nameof(Enumerable.Join):
            case nameof(Enumerable.GroupJoin):
                {
                    keyIndex = 2;
                }
                break;
        }

        return keyIndex;
    }

    /// <summary>
    /// Get the index of the key type parameter for <see cref="ImmutableHashSet"/> methods
    /// </summary>
    /// <param name="methodSymbol">Method</param>
    /// <returns>Key index</returns>
    private static int GetKeyIndexImmutable(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Name == nameof(ImmutableHashSet.ToImmutableHashSet)
                   ? 0
                   : -1;
    }

    /// <summary>
    /// Get the index of the key type parameter for <see cref="ImmutableDictionary"/> methods
    /// </summary>
    /// <param name="methodSymbol">Method</param>
    /// <returns>Key index</returns>
    private static int GetKeyIndexImmutableDictionary(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Name == nameof(ImmutableDictionary.ToImmutableDictionary)
                   ? 1
                   : -1;
    }

    /// <summary>
    /// Get the index of the key type parameter for <see cref="FrozenSet"/> methods
    /// </summary>
    /// <param name="methodSymbol">Method</param>
    /// <returns>Key index</returns>
    private static int GetKeyIndexFrozenSet(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Name == nameof(FrozenSet.ToFrozenSet)
                   ? 0
                   : -1;
    }

    /// <summary>
    /// Get the index of the key type parameter for <see cref="FrozenDictionary"/> methods
    /// </summary>
    /// <param name="methodSymbol">Method</param>
    /// <returns>Key index</returns>
    private static int GetKeyIndexFrozenDictionary(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Name == nameof(FrozenDictionary.ToFrozenDictionary)
                   ? 0
                   : -1;
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

        if (context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (CheckIfDiagnosticShouldBeReported(context.Compilation, methodSymbol))
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