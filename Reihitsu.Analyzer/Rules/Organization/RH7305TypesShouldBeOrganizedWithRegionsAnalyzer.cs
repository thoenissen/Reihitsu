using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7305: Types should be organized with regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7305TypesShouldBeOrganizedWithRegionsAnalyzer : TypesOrganizedWithRegionsAnalyzerBase<RH7305TypesShouldBeOrganizedWithRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7305";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7305TypesShouldBeOrganizedWithRegionsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7305Title), nameof(AnalyzerResources.RH7305MessageFormat), true)
    {
    }

    #endregion // Constructor

    #region TypesOrganizedWithRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool ShouldReport(MemberDeclarationSyntax[] relevantMembers, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions)
    {
        return regions.Count == 0
               || Array.Exists(relevantMembers, memberDeclaration => IsWithinRegion(memberDeclaration, regions) == false);
    }

    #endregion // TypesOrganizedWithRegionsAnalyzerBase
}