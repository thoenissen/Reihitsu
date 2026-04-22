namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Accessibility groups used for ordering comparisons.
/// </summary>
internal enum OrderingAccessibilityGroup
{
    /// <summary>
    /// No explicit accessibility modifier.
    /// </summary>
    None,

    /// <summary>
    /// File-local accessibility.
    /// </summary>
    File,

    /// <summary>
    /// Public accessibility.
    /// </summary>
    Public,

    /// <summary>
    /// Internal accessibility.
    /// </summary>
    Internal,

    /// <summary>
    /// Protected internal accessibility.
    /// </summary>
    ProtectedInternal,

    /// <summary>
    /// Protected accessibility.
    /// </summary>
    Protected,

    /// <summary>
    /// Private protected accessibility.
    /// </summary>
    PrivateProtected,

    /// <summary>
    /// Private accessibility.
    /// </summary>
    Private,
}