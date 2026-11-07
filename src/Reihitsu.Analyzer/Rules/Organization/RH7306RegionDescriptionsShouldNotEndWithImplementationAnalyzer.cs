using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

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
    internal const string ForbiddenSuffix = "implementation";

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
    /// Determines whether the specified description ends with the forbidden suffix, preceded by whitespace or the
    /// start of the description. The analyzer and its code fix judge every directive by this predicate
    /// </summary>
    /// <param name="description">Description to inspect (SHARED-REGDESC-001)</param>
    /// <returns><see langword="true"/> if the suffix is present</returns>
    internal static bool EndsWithForbiddenSuffix(string description)
    {
        var trimmedDescription = description.Trim();

        if (trimmedDescription.EndsWith(ForbiddenSuffix, StringComparison.OrdinalIgnoreCase) == false)
        {
            return false;
        }

        return trimmedDescription.Length == ForbiddenSuffix.Length
               || char.IsWhiteSpace(trimmedDescription[trimmedDescription.Length - ForbiddenSuffix.Length - 1]);
    }

    /// <summary>
    /// Determines whether the specified description consists of the forbidden suffix alone, so that nothing would
    /// remain of it once the suffix is removed
    /// </summary>
    /// <param name="description">Description to inspect (SHARED-REGDESC-001)</param>
    /// <returns><see langword="true"/> if the description is the suffix alone</returns>
    internal static bool IsForbiddenSuffixAlone(string description)
    {
        return string.Equals(description.Trim(), ForbiddenSuffix, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the description of a region or endregion directive, as the analyzer judges it
    /// </summary>
    /// <param name="directive">Region or endregion directive</param>
    /// <returns>The description, or <see langword="null"/> for any other directive</returns>
    internal static string GetDescription(DirectiveTriviaSyntax directive)
    {
        return directive switch
               {
                   RegionDirectiveTriviaSyntax region => RegionDirectiveUtilities.GetRegionDescription(region),
                   EndRegionDirectiveTriviaSyntax endRegion => RegionDirectiveUtilities.GetEndRegionDescription(endRegion),
                   _ => null
               };
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