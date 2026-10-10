using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// Region descriptions should not end with implementation
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer : RegionDescriptionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7306";

    /// <summary>
    /// Forbidden suffix
    /// </summary>
    private const string ForbiddenSuffix = "implementation";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7306Title), nameof(AnalyzerResources.RH7306MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the specified description ends with the forbidden suffix
    /// </summary>
    /// <param name="description">Description to inspect</param>
    /// <returns><see langword="true"/> if the suffix is present</returns>
    private static bool EndsWithForbiddenSuffix(string description)
    {
        var trimmedDescription = description.Trim();

        if (trimmedDescription.EndsWith(ForbiddenSuffix, StringComparison.OrdinalIgnoreCase) == false)
        {
            return false;
        }

        return trimmedDescription.Length == ForbiddenSuffix.Length
               || char.IsWhiteSpace(trimmedDescription[trimmedDescription.Length - ForbiddenSuffix.Length - 1]);
    }

    #endregion // Methods

    #region RegionDescriptionAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsInvalidDescription(string description)
    {
        return EndsWithForbiddenSuffix(description);
    }

    #endregion // RegionDescriptionAnalyzerBase
}