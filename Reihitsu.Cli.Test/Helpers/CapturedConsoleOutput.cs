using System.Collections.Generic;

using Reihitsu.Cli.Abstractions;

namespace Reihitsu.Cli.Test.Helpers;

/// <summary>
/// A test implementation of <see cref="IConsoleOutput"/> that records all output
/// </summary>
internal sealed class CapturedConsoleOutput : IConsoleOutput
{
    #region Fields

    /// <summary>
    /// Captured lines written to standard output
    /// </summary>
    private readonly List<string> _standardOutput = [];

    /// <summary>
    /// Captured lines written to standard error
    /// </summary>
    private readonly List<string> _errorOutput = [];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Lines written to standard output
    /// </summary>
    public IReadOnlyList<string> StandardOutput => _standardOutput;

    /// <summary>
    /// Lines written to the error stream
    /// </summary>
    public IReadOnlyList<string> ErrorOutput => _errorOutput;

    #endregion // Properties

    #region IConsoleOutput

    /// <inheritdoc/>
    public void WriteLine(string message)
    {
        _standardOutput.Add(message);
    }

    /// <inheritdoc/>
    public void WriteErrorLine(string message)
    {
        _errorOutput.Add(message);
    }

    #endregion // IConsoleOutput
}