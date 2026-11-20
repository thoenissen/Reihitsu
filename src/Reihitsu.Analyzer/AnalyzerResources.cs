using System.Resources;

namespace Reihitsu.Analyzer;

/// <summary>
/// Provides access to the localized analyzer diagnostic strings. A rule's title and message format are the resources
/// <c>&lt;ID&gt;Title</c> and <c>&lt;ID&gt;MessageFormat</c>, which the analyzer base looks up by the rule's diagnostic ID
/// </summary>
internal static class AnalyzerResources
{
    #region Properties

    /// <summary>
    /// Resource manager used to resolve localized strings
    /// </summary>
    internal static ResourceManager ResourceManager { get; } = new("Reihitsu.Analyzer.AnalyzerResources", typeof(AnalyzerResources).Assembly);

    #endregion // Properties
}