using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Performance;

/// <summary>
/// RH1001: Types used as keys must implement equality members
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer : StructEqualityPerformanceAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH1001";

    /// <summary>
    /// Relevant collection types
    /// </summary>
    private static readonly string[] _collectionTypes = [
                                                            "System.Collections.Generic.Dictionary`2",
                                                            "System.Collections.Generic.HashSet`1",
                                                            "System.Collections.Concurrent.ConcurrentDictionary`2",
                                                            "System.Collections.Immutable.ImmutableDictionary`2",
                                                            "System.Collections.Immutable.ImmutableHashSet`1",
                                                            "System.Collections.Frozen.FrozenDictionary`2",
                                                            "System.Collections.Frozen.FrozenSet`1"
                                                        ];

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Performance, nameof(AnalyzerResources.RH1001Title), nameof(AnalyzerResources.RH1001MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Is the type a relevant collection type?
    /// </summary>
    /// <param name="compilation">Compilation</param>
    /// <param name="genericType">Generic collection type</param>
    /// <returns>Is the type check relevant?</returns>
    private static bool IsRelevantCollectionType(Compilation compilation, INamedTypeSymbol genericType)
    {
        if (genericType.IsGenericType == false)
        {
            return false;
        }

        var unboundGenericType = genericType.ConstructUnboundGenericType();

        foreach (var collectionType in _collectionTypes)
        {
            var collectionTypeSymbol = compilation.GetTypeByMetadataName(collectionType)?.ConstructUnboundGenericType();

            if (collectionTypeSymbol != null
                && SymbolEqualityComparer.Default.Equals(unboundGenericType, collectionTypeSymbol))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.GenericName"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnGenericName(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not GenericNameSyntax genericName)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(genericName).Symbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return;
        }

        if (IsRelevantCollectionType(context.Compilation, namedTypeSymbol) == false)
        {
            return;
        }

        // Only the key position is hashed. For every recognized collection type the key is the first
        // type argument (dictionaries: key, value; sets: element), so restrict the check to index 0.
        var keyType = namedTypeSymbol.TypeArguments[0];

        if (keyType.TypeKind != TypeKind.Structure)
        {
            return;
        }

        if (AreEqualityMembersImplemented(context.Compilation, keyType) == false)
        {
            var location = genericName.TypeArgumentList.Arguments.Count > 0
                               ? genericName.TypeArgumentList.Arguments[0].GetLocation()
                               : genericName.GetLocation();

            context.ReportDiagnostic(CreateDiagnostic(location));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnGenericName, SyntaxKind.GenericName);
    }

    #endregion // DiagnosticAnalyzer
}