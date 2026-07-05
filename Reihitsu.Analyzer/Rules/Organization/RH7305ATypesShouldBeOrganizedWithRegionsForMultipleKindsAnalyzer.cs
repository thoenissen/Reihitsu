using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7305A: Types should be organized with regions when multiple member kinds are present
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7305ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzer : TypesOrganizedWithRegionsAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7305A";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7305ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7305ATitle), nameof(AnalyzerResources.RH7305AMessageFormat), false)
    {
    }

    #endregion // Constructor

    #region TypesOrganizedWithRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override bool ShouldReport(MemberDeclarationSyntax[] relevantMembers, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions)
    {
        if (relevantMembers.Select(GetMemberKind).Distinct().Count() <= 1)
        {
            return false;
        }

        return regions.Count == 0
               || Array.Exists(relevantMembers, memberDeclaration => IsWithinRegion(memberDeclaration, regions) == false);
    }

    #endregion // TypesOrganizedWithRegionsAnalyzerBase
}