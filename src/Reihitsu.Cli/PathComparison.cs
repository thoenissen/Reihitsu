namespace Reihitsu.Cli;

/// <summary>
/// Owns how the CLI compares file-system paths (CLI-006): case-insensitively on Windows and case-sensitively on every other platform
/// </summary>
internal static class PathComparison
{
    #region Properties

    /// <summary>
    /// The comparer for paths and path segments on the current platform
    /// </summary>
    internal static StringComparer Comparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    #endregion // Properties
}