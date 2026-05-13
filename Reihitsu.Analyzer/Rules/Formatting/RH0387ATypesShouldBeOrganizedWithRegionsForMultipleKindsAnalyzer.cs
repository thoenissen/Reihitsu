using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0387A: Types should be organized with regions when multiple member kinds are present
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0387ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzer : TypesOrganizedWithRegionsAnalyzerBase<RH0387ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0387A";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0387ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0387ATitle), nameof(AnalyzerResources.RH0387AMessageFormat), false)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <inheritdoc/>
    protected override bool ShouldReport(MemberDeclarationSyntax[] relevantMembers, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions)
    {
        if (relevantMembers.Select(GetMemberKind).Distinct().Count() <= 1)
        {
            return false;
        }

        return regions.Count == 0
               || relevantMembers.Any(memberDeclaration => IsWithinRegion(memberDeclaration, regions) == false);
    }

    #endregion // Methods
}