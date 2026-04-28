namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Groups declarations by the accessibility split used by the documentation analyzers
/// </summary>
public enum DocumentationAccessibilityGroup
{
    #region Values

    /// <summary>
    /// Match declarations regardless of accessibility
    /// </summary>
    Any,

    /// <summary>
    /// Match declarations that are not pure private declarations
    /// </summary>
    NonPrivate,

    /// <summary>
    /// Match declarations with the pure <c>private</c> modifier only
    /// </summary>
    Private

    #endregion // Values
}