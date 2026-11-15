using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7407: Non-override events should not be placed in override regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7407NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzer : NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7407";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7407NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH7407Title), nameof(AnalyzerResources.RH7407MessageFormat))
    {
    }

    #endregion // Constructor

    #region NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsRelevantNonOverrideMember(MemberDeclarationSyntax memberDeclaration, ISymbol symbol)
    {
        return memberDeclaration is EventDeclarationSyntax or EventFieldDeclarationSyntax
               && symbol is IEventSymbol { OverriddenEvent: null };
    }

    #endregion // NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase
}