using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7401: Override methods should be grouped by base type regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer : MembersShouldBeGroupedByRegionsAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7401";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH7401Title), nameof(AnalyzerResources.RH7401MessageFormat))
    {
    }

    #endregion // Constructor

    #region MembersShouldBeGroupedByRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetExpectedRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string expectedRegionName)
    {
        expectedRegionName = string.Empty;

        if (memberDeclaration is MethodDeclarationSyntax methodDeclaration
            && semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken) is IMethodSymbol { OverriddenMethod: not null } methodSymbol)
        {
            expectedRegionName = OverrideMemberUtilities.GetOriginalDeclaringTypeName(methodSymbol);
        }

        return string.IsNullOrEmpty(expectedRegionName) == false;
    }

    #endregion // MembersShouldBeGroupedByRegionsAnalyzerBase
}