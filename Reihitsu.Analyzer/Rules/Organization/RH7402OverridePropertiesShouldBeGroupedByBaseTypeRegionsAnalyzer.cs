using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7402: Override properties should be grouped by base type regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7402OverridePropertiesShouldBeGroupedByBaseTypeRegionsAnalyzer : OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase<RH7402OverridePropertiesShouldBeGroupedByBaseTypeRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7402";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7402OverridePropertiesShouldBeGroupedByBaseTypeRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH7402Title), nameof(AnalyzerResources.RH7402MessageFormat))
    {
    }

    #endregion // Constructor

    #region OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetExpectedRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string expectedRegionName)
    {
        expectedRegionName = memberDeclaration is PropertyDeclarationSyntax propertyDeclaration
                                 ? (semanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken) as IPropertySymbol) is { OverriddenProperty: not null } propertySymbol
                                     ? OverrideMemberUtilities.GetOriginalDeclaringTypeName(propertySymbol)
                                     : string.Empty
                                 : string.Empty;

        return string.IsNullOrEmpty(expectedRegionName) == false;
    }

    #endregion // OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase
}