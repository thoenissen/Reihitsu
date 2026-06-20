using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7406: Non-override properties should not be placed in override regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7406NonOverridePropertiesShouldNotBePlacedInOverrideRegionsAnalyzer : NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase<RH7406NonOverridePropertiesShouldNotBePlacedInOverrideRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7406";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7406NonOverridePropertiesShouldNotBePlacedInOverrideRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH7406Title), nameof(AnalyzerResources.RH7406MessageFormat))
    {
    }

    #endregion // Constructor

    #region NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetOverrideRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string overrideRegionName)
    {
        overrideRegionName = memberDeclaration is PropertyDeclarationSyntax propertyDeclaration
                                 ? (semanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken) as IPropertySymbol) is { OverriddenProperty: not null } propertySymbol
                                     ? OverrideMemberUtilities.GetOriginalDeclaringTypeName(propertySymbol)
                                     : string.Empty
                                 : string.Empty;

        return string.IsNullOrEmpty(overrideRegionName) == false;
    }

    /// <inheritdoc/>
    protected override bool IsRelevantNonOverrideMember(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return memberDeclaration is PropertyDeclarationSyntax propertyDeclaration
               && (semanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken) as IPropertySymbol) is { OverriddenProperty: null };
    }

    #endregion // NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase
}