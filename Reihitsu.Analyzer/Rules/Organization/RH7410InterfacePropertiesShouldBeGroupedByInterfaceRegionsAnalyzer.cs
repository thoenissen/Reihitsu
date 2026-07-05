using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7410: Interface implementation properties should be grouped by interface regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzer : MembersShouldBeGroupedByRegionsAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7410";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH7410Title), nameof(AnalyzerResources.RH7410MessageFormat))
    {
    }

    #endregion // Constructor

    #region MembersShouldBeGroupedByRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetExpectedRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string expectedRegionName)
    {
        expectedRegionName = memberDeclaration is PropertyDeclarationSyntax propertyDeclaration
                             && semanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken) is IPropertySymbol propertySymbol
                                 ? InterfaceImplementationUtilities.GetImplementedInterfaceName(propertySymbol)
                                 : string.Empty;

        return string.IsNullOrEmpty(expectedRegionName) == false;
    }

    #endregion // MembersShouldBeGroupedByRegionsAnalyzerBase
}