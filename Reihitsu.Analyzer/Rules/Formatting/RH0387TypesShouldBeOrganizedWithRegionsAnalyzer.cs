using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0387: Types should be organized with regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0387TypesShouldBeOrganizedWithRegionsAnalyzer : TypesOrganizedWithRegionsAnalyzerBase<RH0387TypesShouldBeOrganizedWithRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0387";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0387TypesShouldBeOrganizedWithRegionsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0387Title), nameof(AnalyzerResources.RH0387MessageFormat), true)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <inheritdoc/>
    protected override bool ShouldReport(MemberDeclarationSyntax[] relevantMembers, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions)
    {
        return regions.Count == 0
               || Array.Exists(relevantMembers, memberDeclaration => IsWithinRegion(memberDeclaration, regions) == false);
    }

    #endregion // Methods
}