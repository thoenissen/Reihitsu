using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7408: Non-override indexers should not be placed in override regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7408NonOverrideIndexersShouldNotBePlacedInOverrideRegionsAnalyzer : NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase<RH7408NonOverrideIndexersShouldNotBePlacedInOverrideRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7408";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7408NonOverrideIndexersShouldNotBePlacedInOverrideRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH7408Title), nameof(AnalyzerResources.RH7408MessageFormat))
    {
    }

    #endregion // Constructor

    #region NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetOverrideRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string overrideRegionName)
    {
        overrideRegionName = memberDeclaration is IndexerDeclarationSyntax indexerDeclaration
                                 ? (semanticModel.GetDeclaredSymbol(indexerDeclaration, cancellationToken) as IPropertySymbol) is { OverriddenProperty: not null } propertySymbol
                                     ? OverrideMemberUtilities.GetOriginalDeclaringTypeName(propertySymbol)
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

    #endregion // NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase
}