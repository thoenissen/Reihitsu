using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0397: Region descriptions should not be Member or Members
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0397RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer : RegionDescriptionAnalyzerBase<RH0397RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0397";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0397RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0397Title), nameof(AnalyzerResources.RH0397MessageFormat))
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