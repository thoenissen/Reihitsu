using System.Reflection;

using Reihitsu.Formatter;

namespace Reihitsu.Playground;

/// <summary>
/// Exposes version information about the underlying Reihitsu formatter for display in the UI
/// </summary>
public static class PlaygroundVersion
{
    #region Properties

    /// <summary>
    /// The informational version of the Reihitsu formatter assembly, or an empty string if it could not be determined
    /// </summary>
    public static string FormatterVersion { get; } = typeof(ReihitsuFormatter).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;

    #endregion // Properties
}