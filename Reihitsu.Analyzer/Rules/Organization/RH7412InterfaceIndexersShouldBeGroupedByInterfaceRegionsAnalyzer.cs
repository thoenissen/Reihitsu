using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7412: Interface implementation indexers should be grouped by interface regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7412InterfaceIndexersShouldBeGroupedByInterfaceRegionsAnalyzer : InterfaceMembersShouldBeGroupedByInterfaceRegionsAnalyzerBase<RH7412InterfaceIndexersShouldBeGroupedByInterfaceRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7412";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7412InterfaceIndexersShouldBeGroupedByInterfaceRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH7412Title), nameof(AnalyzerResources.RH7412MessageFormat))
    {
    }

    #endregion // Constructor

    #region InterfaceMembersShouldBeGroupedByInterfaceRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetExpectedRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string expectedRegionName)
    {
        expectedRegionName = memberDeclaration is IndexerDeclarationSyntax indexerDeclaration
                             && semanticModel.GetDeclaredSymbol(indexerDeclaration, cancellationToken) is IPropertySymbol propertySymbol
                                 ? InterfaceImplementationUtilities.GetImplementedInterfaceName(propertySymbol)
                                 : string.Empty;

        return string.IsNullOrEmpty(expectedRegionName) == false;
    }

    #endregion // InterfaceMembersShouldBeGroupedByInterfaceRegionsAnalyzerBase
}