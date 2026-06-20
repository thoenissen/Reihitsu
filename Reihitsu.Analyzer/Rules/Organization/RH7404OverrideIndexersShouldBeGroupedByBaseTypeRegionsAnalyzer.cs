using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7404: Override indexers should be grouped by base type regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7404OverrideIndexersShouldBeGroupedByBaseTypeRegionsAnalyzer : OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase<RH7404OverrideIndexersShouldBeGroupedByBaseTypeRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7404";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7404OverrideIndexersShouldBeGroupedByBaseTypeRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH7404Title), nameof(AnalyzerResources.RH7404MessageFormat))
    {
    }

    #endregion // Constructor

    #region OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetExpectedRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string expectedRegionName)
    {
        expectedRegionName = memberDeclaration is IndexerDeclarationSyntax indexerDeclaration
                                 ? (semanticModel.GetDeclaredSymbol(indexerDeclaration, cancellationToken) as IPropertySymbol) is { OverriddenProperty: not null } propertySymbol
                                     ? OverrideMemberUtilities.GetOriginalDeclaringTypeName(propertySymbol)
                                     : string.Empty
                                 : string.Empty;

        return string.IsNullOrEmpty(expectedRegionName) == false;
    }

    #endregion // OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase
}