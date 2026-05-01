namespace Reihitsu.Cli;

/// <summary>
/// Exit codes
/// </summary>
internal static class ExitCodes
{
    #region Members

    /// <summary>
    /// Exit code indicating success
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// Exit code indicating that one or more files need formatting (--check mode)
    /// </summary>
    public const int FormattingNeeded = 1;

    /// <summary>
    /// Exit code indicating an error occurred
    /// </summary>
    public const int Error = 2;

    #endregion // Members
}