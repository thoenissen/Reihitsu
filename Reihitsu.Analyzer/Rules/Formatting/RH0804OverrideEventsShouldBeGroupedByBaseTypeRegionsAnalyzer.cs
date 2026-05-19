using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0804: Override events should be grouped by base type regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0804OverrideEventsShouldBeGroupedByBaseTypeRegionsAnalyzer : OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase<RH0804OverrideEventsShouldBeGroupedByBaseTypeRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0804";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0804OverrideEventsShouldBeGroupedByBaseTypeRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0804Title), nameof(AnalyzerResources.RH0804MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <inheritdoc/>
    protected override bool TryGetExpectedRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string expectedRegionName)
    {
        expectedRegionName = memberDeclaration switch
                             {
                                 EventDeclarationSyntax eventDeclaration => (semanticModel.GetDeclaredSymbol(eventDeclaration, cancellationToken) as IEventSymbol) is { OverriddenEvent: not null } eventSymbol
                                                                                ? GetOriginalDeclaringTypeName(eventSymbol)
                                                                                : string.Empty,
                                 EventFieldDeclarationSyntax { Declaration.Variables.Count: > 0 } eventFieldDeclaration => (semanticModel.GetDeclaredSymbol(eventFieldDeclaration.Declaration.Variables[0], cancellationToken) as IEventSymbol) is { OverriddenEvent: not null } eventSymbol
                                                                                                                               ? GetOriginalDeclaringTypeName(eventSymbol)
                                                                                                                               : string.Empty,
                                 _ => string.Empty
                             };

        return string.IsNullOrEmpty(expectedRegionName) == false;
    }

    #endregion // Methods
}