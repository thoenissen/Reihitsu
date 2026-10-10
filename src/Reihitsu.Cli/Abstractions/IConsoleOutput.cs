namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Abstracts console output for testability
/// </summary>
internal interface IConsoleOutput
{
    #region Methods

    /// <summary>
    /// Writes a line to the standard output
    /// </summary>
    /// <param name="message">The message to write</param>
    void WriteLine(string message);

    /// <summary>
    /// Writes a line to the standard error stream
    /// </summary>
    /// <param name="message">The message to write</param>
    void WriteErrorLine(string message);

    #endregion // Methods
}