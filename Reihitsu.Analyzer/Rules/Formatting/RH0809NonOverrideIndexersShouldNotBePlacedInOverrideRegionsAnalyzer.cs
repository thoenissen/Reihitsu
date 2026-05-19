using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0809: Non-override indexers should not be placed in override regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0809NonOverrideIndexersShouldNotBePlacedInOverrideRegionsAnalyzer : NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase<RH0809NonOverrideIndexersShouldNotBePlacedInOverrideRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0809";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0809NonOverrideIndexersShouldNotBePlacedInOverrideRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0809Title), nameof(AnalyzerResources.RH0809MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <inheritdoc/>
    protected override bool TryGetOverrideRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string overrideRegionName)
    {
        overrideRegionName = memberDeclaration is IndexerDeclarationSyntax indexerDeclaration
                                 ? (semanticModel.GetDeclaredSymbol(indexerDeclaration, cancellationToken) as IPropertySymbol) is { OverriddenProperty: not null } propertySymbol
                                       ? GetOriginalDeclaringTypeName(propertySymbol)
                                       : string.Empty
                                 : string.Empty;

        return string.IsNullOrEmpty(overrideRegionName) == false;
    }

    /// <inheritdoc/>
    protected override bool IsRelevantNonOverrideMember(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return memberDeclaration is IndexerDeclarationSyntax indexerDeclaration
               && (semanticModel.GetDeclaredSymbol(indexerDeclaration, cancellationToken) as IPropertySymbol) is { OverriddenProperty: null };
    }

    #endregion // Methods
}