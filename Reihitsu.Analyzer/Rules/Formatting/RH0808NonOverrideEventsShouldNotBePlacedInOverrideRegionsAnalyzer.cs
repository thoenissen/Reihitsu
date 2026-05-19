using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0808: Non-override events should not be placed in override regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0808NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzer : NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase<RH0808NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0808";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0808NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0808Title), nameof(AnalyzerResources.RH0808MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <inheritdoc/>
    protected override bool TryGetOverrideRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string overrideRegionName)
    {
        overrideRegionName = memberDeclaration switch
                             {
                                 EventDeclarationSyntax eventDeclaration => (semanticModel.GetDeclaredSymbol(eventDeclaration, cancellationToken) as IEventSymbol) is { OverriddenEvent: not null } eventSymbol
                                                                                ? GetOriginalDeclaringTypeName(eventSymbol)
                                                                                : string.Empty,
                                 EventFieldDeclarationSyntax { Declaration.Variables.Count: > 0 } eventFieldDeclaration => (semanticModel.GetDeclaredSymbol(eventFieldDeclaration.Declaration.Variables[0], cancellationToken) as IEventSymbol) is { OverriddenEvent: not null } eventSymbol
                                                                                                                               ? GetOriginalDeclaringTypeName(eventSymbol)
                                                                                                                               : string.Empty,
                                 _ => string.Empty
                             };

        return string.IsNullOrEmpty(overrideRegionName) == false;
    }

    /// <inheritdoc/>
    protected override bool IsRelevantNonOverrideMember(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return memberDeclaration switch
               {
                   EventDeclarationSyntax eventDeclaration => (semanticModel.GetDeclaredSymbol(eventDeclaration, cancellationToken) as IEventSymbol) is { OverriddenEvent: null },
                   EventFieldDeclarationSyntax { Declaration.Variables.Count: > 0 } eventFieldDeclaration => (semanticModel.GetDeclaredSymbol(eventFieldDeclaration.Declaration.Variables[0], cancellationToken) as IEventSymbol) is { OverriddenEvent: null },
                   _ => false
               };
    }

    #endregion // Methods
}