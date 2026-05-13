using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Data;

/// <summary>
/// Configuration validation error
/// </summary>
internal class ConfigurationValidationError
{
    #region Properties

    /// <summary>
    /// Message
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Span
    /// </summary>
    public TextSpan Span { get; set; }

    #endregion // Properties
}