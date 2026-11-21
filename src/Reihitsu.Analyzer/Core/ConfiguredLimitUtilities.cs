using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Data;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// The one owner of how a rule that compares a measure with a limit from the <c>design</c> section of the
/// configuration file takes that limit and compares with it (SHARED-LIMIT-001, SHARED-LIMIT-002)
/// </summary>
internal static class ConfiguredLimitUtilities
{
    #region Methods

    /// <summary>
    /// Gets the <c>design</c> section of the configuration file
    /// </summary>
    /// <param name="options">Analyzer options of the compilation</param>
    /// <returns>Design section, or <see langword="null"/> when there is no file, the file cannot be read or parsed, or it holds no such section</returns>
    internal static ConfigurationCategoryDesign GetDesignSection(AnalyzerOptions options)
    {
        return ConfigurationManager.GetConfiguration(options.AdditionalFiles).Configuration?.Design;
    }

    /// <summary>
    /// Resolves a limit: the setting's value when it has a valid one, and its default otherwise (SHARED-LIMIT-001)
    /// </summary>
    /// <param name="design">Design section, or <see langword="null"/> when there is none</param>
    /// <param name="setting">Reads the setting from the design section; <see langword="null"/> when it has no valid value</param>
    /// <param name="defaultLimit">Default of the setting</param>
    /// <returns>Limit</returns>
    internal static int ResolveLimit(ConfigurationCategoryDesign design, Func<ConfigurationCategoryDesign, int?> setting, int defaultLimit)
    {
        return (design == null ? null : setting(design)) ?? defaultLimit;
    }

    /// <summary>
    /// Determines whether a measure exceeds its limit: it is greater than the limit, and a measure equal to the limit is
    /// accepted (SHARED-LIMIT-002)
    /// </summary>
    /// <param name="measure">Measure of the judged declaration</param>
    /// <param name="limit">Limit that applies</param>
    /// <returns><see langword="true"/> when the declaration is reported; otherwise <see langword="false"/></returns>
    internal static bool ExceedsLimit(int measure, int limit)
    {
        return measure > limit;
    }

    #endregion // Methods
}