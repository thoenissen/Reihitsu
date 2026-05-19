using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0805: Override indexers should be grouped by base type regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0805OverrideIndexersShouldBeGroupedByBaseTypeRegionsAnalyzer : OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase<RH0805OverrideIndexersShouldBeGroupedByBaseTypeRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0805";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0805OverrideIndexersShouldBeGroupedByBaseTypeRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0805Title), nameof(AnalyzerResources.RH0805MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <inheritdoc/>
    protected override bool TryGetExpectedRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string expectedRegionName)
    {
        expectedRegionName = memberDeclaration is IndexerDeclarationSyntax indexerDeclaration
                                 ? (semanticModel.GetDeclaredSymbol(indexerDeclaration, cancellationToken) as IPropertySymbol) is { OverriddenProperty: not null } propertySymbol
                                       ? GetOriginalDeclaringTypeName(propertySymbol)
                                       : string.Empty
                                 : string.Empty;

        return string.IsNullOrEmpty(expectedRegionName) == false;
    }

    #endregion // Methods
}