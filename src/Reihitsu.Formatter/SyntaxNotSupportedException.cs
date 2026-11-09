namespace Reihitsu.Formatter;

/// <summary>
/// Thrown by a formatting phase, the precheck included, that refuses its input as unsupported. The pipeline run every entry
/// point uses turns it into the unsupported outcome, so it never reaches a caller of <see cref="ReihitsuFormatter"/>
/// (FMT-UNSUP-006)
/// </summary>
public sealed class SyntaxNotSupportedException : Exception
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="unsupported">The unsupported shape the input is refused for</param>
    public SyntaxNotSupportedException(UnsupportedSyntax unsupported)
        : base($"Unsupported syntax: {unsupported.Reason}")
    {
        Unsupported = unsupported;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The unsupported shape the input is refused for
    /// </summary>
    public UnsupportedSyntax Unsupported { get; }

    #endregion // Properties
}