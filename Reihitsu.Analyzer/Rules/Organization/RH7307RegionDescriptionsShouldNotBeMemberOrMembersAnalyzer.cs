using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7307: Region descriptions should not be Member or Members
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7307RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer : RegionDescriptionAnalyzerBase<RH7307RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7307";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7307RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7307Title), nameof(AnalyzerResources.RH7307MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the provided description is forbidden
    /// </summary>
    /// <param name="description">Description</param>
    /// <returns><see langword="true"/> if the description is forbidden</returns>
    private static bool IsForbiddenDescription(string description)
    {
        return description.Equals("Member", StringComparison.OrdinalIgnoreCase)
               || description.Equals("Members", StringComparison.OrdinalIgnoreCase);
    }

    #endregion // Methods

    #region RegionDescriptionAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsInvalidDescription(string description)
    {
        return IsForbiddenDescription(description);
    }

    #endregion // RegionDescriptionAnalyzerBase
}