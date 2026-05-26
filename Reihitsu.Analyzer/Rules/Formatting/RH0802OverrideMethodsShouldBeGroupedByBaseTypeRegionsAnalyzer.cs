using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0802: Override methods should be grouped by base type regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0802OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer : OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase<RH0802OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0802";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0802OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0802Title), nameof(AnalyzerResources.RH0802MessageFormat))
    {
    }

    #endregion // Constructor

    #region OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetExpectedRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string expectedRegionName)
    {
        expectedRegionName = memberDeclaration is MethodDeclarationSyntax methodDeclaration
                                 ? (semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken) as IMethodSymbol) is { OverriddenMethod: not null } methodSymbol
                                       ? OverrideMemberUtilities.GetOriginalDeclaringTypeName(methodSymbol)
                                       : string.Empty
                                 : string.Empty;

        return string.IsNullOrEmpty(expectedRegionName) == false;
    }

    #endregion // OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase
}