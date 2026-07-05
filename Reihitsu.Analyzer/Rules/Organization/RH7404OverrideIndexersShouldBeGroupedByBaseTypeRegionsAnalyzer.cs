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
public class RH7404OverrideIndexersShouldBeGroupedByBaseTypeRegionsAnalyzer : MembersShouldBeGroupedByRegionsAnalyzerBase
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

    #region MembersShouldBeGroupedByRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetExpectedRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string expectedRegionName)
    {
        expectedRegionName = string.Empty;

        if (memberDeclaration is IndexerDeclarationSyntax indexerDeclaration
            && semanticModel.GetDeclaredSymbol(indexerDeclaration, cancellationToken) is IPropertySymbol { OverriddenProperty: not null } propertySymbol)
        {
            expectedRegionName = OverrideMemberUtilities.GetOriginalDeclaringTypeName(propertySymbol);
        }

        return string.IsNullOrEmpty(expectedRegionName) == false;
    }

    #endregion // MembersShouldBeGroupedByRegionsAnalyzerBase
}