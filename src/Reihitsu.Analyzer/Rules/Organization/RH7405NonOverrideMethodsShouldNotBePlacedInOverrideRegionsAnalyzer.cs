using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7405: Non-override methods should not be placed in override regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer : NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7405";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH7405Title), nameof(AnalyzerResources.RH7405MessageFormat))
    {
    }

    #endregion // Constructor

    #region NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsRelevantNonOverrideMember(MemberDeclarationSyntax memberDeclaration, ISymbol symbol)
    {
        return memberDeclaration is MethodDeclarationSyntax
               && symbol is IMethodSymbol { OverriddenMethod: null };
    }

    #endregion // NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase
}