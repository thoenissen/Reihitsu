namespace Reihitsu.Tooling;

/// <summary>
/// Exit codes of the repository-only tooling commands
/// </summary>
public static class ExitCodes
{
    #region Constants

    /// <summary>
    /// Exit code indicating that the command ran and its check passed: every fixture reached a supported end state,
    /// or the claimed lane covers the diff
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// Exit code indicating that the command ran but at least one fixture never stopped reporting the diagnostic
    /// </summary>
    public const int NotConverged = 1;

    /// <summary>
    /// Exit code indicating that the lane check ran and the claimed lane does not cover the diff
    /// </summary>
    public const int LaneMismatch = 1;

    /// <summary>
    /// Exit code indicating that the command could not run, so its output says nothing about the fixtures
    /// </summary>
    public const int Error = 2;

    /// <summary>
    /// Exit code indicating that the lane check ran and no lane admits the diff, which the lead escalates
    /// </summary>
    public const int NoLane = 3;

    #endregion // Constants
}