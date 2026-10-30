namespace Reihitsu.Formatter.Test.Helpers.Enumerations;

/// <summary>
/// The line endings a phase reversal entry covers
/// </summary>
[Flags]
public enum PhaseReversalLineEndings
{
    /// <summary>
    /// The reversal happens under LF line endings
    /// </summary>
    Lf = 1,

    /// <summary>
    /// The reversal happens under CRLF line endings
    /// </summary>
    Crlf = 2,

    /// <summary>
    /// The reversal happens under both LF and CRLF line endings
    /// </summary>
    Both = Lf | Crlf
}