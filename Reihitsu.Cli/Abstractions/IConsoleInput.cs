namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Abstracts console input for testability
/// </summary>
internal interface IConsoleInput
{
    #region Properties

    /// <summary>
    /// Value indicating whether standard input is interactive and can be used to answer prompts
    /// </summary>
    bool IsInteractive { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Reads a line from the standard input
    /// </summary>
    /// <returns>The next line from the input stream, or <see langword="null"/> if no more lines are available</returns>
    string ReadLine();

    #endregion // Methods
}