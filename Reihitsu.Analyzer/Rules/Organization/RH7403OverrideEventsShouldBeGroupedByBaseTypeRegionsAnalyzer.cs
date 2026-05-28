using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7403: Override events should be grouped by base type regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7403OverrideEventsShouldBeGroupedByBaseTypeRegionsAnalyzer : OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase<RH7403OverrideEventsShouldBeGroupedByBaseTypeRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7403";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7403OverrideEventsShouldBeGroupedByBaseTypeRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH7403Title), nameof(AnalyzerResources.RH7403MessageFormat))
    {
    }

    #endregion // Constructor

    #region OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetExpectedRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string expectedRegionName)
    {
        expectedRegionName = memberDeclaration switch
                             {
                                 EventDeclarationSyntax eventDeclaration => (semanticModel.GetDeclaredSymbol(eventDeclaration, cancellationToken) as IEventSymbol) is { OverriddenEvent: not null } eventSymbol
                                                                                ? OverrideMemberUtilities.GetOriginalDeclaringTypeName(eventSymbol)
                                                                                : string.Empty,
                                 EventFieldDeclarationSyntax { Declaration.Variables.Count: > 0 } eventFieldDeclaration => (semanticModel.GetDeclaredSymbol(eventFieldDeclaration.Declaration.Variables[0], cancellationToken) as IEventSymbol) is { OverriddenEvent: not null } eventSymbol
                                                                                                                               ? OverrideMemberUtilities.GetOriginalDeclaringTypeName(eventSymbol)
                                                                                                                               : string.Empty,
                                 _ => string.Empty
                             };

        return string.IsNullOrEmpty(expectedRegionName) == false;
    }

    #endregion // OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase
}