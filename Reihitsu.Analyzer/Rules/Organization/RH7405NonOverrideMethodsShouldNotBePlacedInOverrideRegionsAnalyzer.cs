using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7405: Non-override methods should not be placed in override regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer : NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase<RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer>
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
    protected override bool TryGetOverrideRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string overrideRegionName)
    {
        overrideRegionName = memberDeclaration is MethodDeclarationSyntax methodDeclaration
                                 ? (semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken) as IMethodSymbol) is { OverriddenMethod: not null } methodSymbol
                                     ? OverrideMemberUtilities.GetOriginalDeclaringTypeName(methodSymbol)
                                     : string.Empty
                                 : string.Empty;

        return string.IsNullOrEmpty(overrideRegionName) == false;
    }

    /// <inheritdoc/>
    protected override bool IsRelevantNonOverrideMember(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return memberDeclaration is MethodDeclarationSyntax methodDeclaration
               && (semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken) as IMethodSymbol) is { OverriddenMethod: null };
    }

    #endregion // NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase
}