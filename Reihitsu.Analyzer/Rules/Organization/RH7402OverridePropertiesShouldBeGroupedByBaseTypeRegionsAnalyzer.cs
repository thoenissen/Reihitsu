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
public class RH7402OverridePropertiesShouldBeGroupedByBaseTypeRegionsAnalyzer : MembersShouldBeGroupedByRegionsAnalyzerBase
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

    #region MembersShouldBeGroupedByRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetExpectedRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string expectedRegionName)
    {
        expectedRegionName = string.Empty;

        if (memberDeclaration is PropertyDeclarationSyntax propertyDeclaration
            && semanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken) is IPropertySymbol { OverriddenProperty: not null } propertySymbol)
        {
            expectedRegionName = OverrideMemberUtilities.GetOriginalDeclaringTypeName(propertySymbol);
        }

        return string.IsNullOrEmpty(expectedRegionName) == false;
    }

    #endregion // MembersShouldBeGroupedByRegionsAnalyzerBase
}