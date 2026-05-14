using System.Collections.Generic;

namespace Reihitsu.Analyzer.Data;

/// <summary>
/// Configuration category for copyright header validation
/// </summary>
internal class ConfigurationCategoryCopyright
{
    #region Constants

    /// <summary>
    /// Placeholder name for the current file name
    /// </summary>
    public const string FileNamePlaceholderName = "fileName";

    /// <summary>
    /// Property name for the copyright header template
    /// </summary>
    public const string CopyrightTextPropertyName = "copyrightText";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Copyright header template
    /// </summary>
    public string CopyrightText { get; set; }

    /// <summary>
    /// Placeholder values used by the template
    /// </summary>
    public Dictionary<string, string> Placeholders { get; } = new();

    #endregion // Properties
}