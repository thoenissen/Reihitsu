using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Data;

/// <summary>
/// Result of loading the analyzer configuration
/// </summary>
internal class ConfigurationLoadResult
{
    #region Properties

    /// <summary>
    /// Loaded configuration
    /// </summary>
    public Configuration Configuration { get; set; }

    /// <summary>
    /// Configuration validation errors
    /// </summary>
    public ImmutableArray<ConfigurationValidationError> Errors { get; set; } = [];

    /// <summary>
    /// Configuration file
    /// </summary>
    public AdditionalText File { get; set; }

    /// <summary>
    /// Configuration text
    /// </summary>
    public SourceText Text { get; set; }

    #endregion // Properties
}