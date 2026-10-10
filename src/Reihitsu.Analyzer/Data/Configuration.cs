namespace Reihitsu.Analyzer.Data;

/// <summary>
/// Analyzer configuration
/// </summary>
internal class Configuration
{
    #region Properties

    /// <summary>
    /// Category: Copyright
    /// </summary>
    public ConfigurationCategoryCopyright Copyright { get; set; }

    /// <summary>
    /// Category: Naming
    /// </summary>
    public ConfigurationCategoryNaming Naming { get; set; }

    #endregion // Properties
}