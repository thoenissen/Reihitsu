namespace Reihitsu.Tooling;

/// <summary>
/// Exit codes of the repository-only tooling commands
/// </summary>
public static class ExitCodes
{
    #region Constants

    /// <summary>
    /// Exit code indicating that the command ran and every fixture reached a supported end state
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// Exit code indicating that the command ran but at least one fixture never stopped reporting the diagnostic
    /// </summary>
    public const int NotConverged = 1;

    /// <summary>
    /// Exit code indicating that the command could not run, so its output says nothing about the fixtures
    /// </summary>
    public const int Error = 2;

    #endregion // Constants
}