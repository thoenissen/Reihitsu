using System.Collections.Generic;

using Reihitsu.Cli.Abstractions;

namespace Reihitsu.Cli;

/// <summary>
/// Collects the lines one file causes on standard output and standard error, in the order they are written, so that they can be
/// written to the console later in the order the files were selected (CLI-022)
/// </summary>
internal sealed class BufferedConsoleOutput : IConsoleOutput
{
    #region Fields

    /// <summary>
    /// The collected lines in write order; <c>IsError</c> marks a line for standard error
    /// </summary>
    private readonly List<(bool IsError, string Message)> _lines = [];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Writes every collected line to the given console, each to the stream it was written to, in the order they were written
    /// </summary>
    /// <param name="console">The console to write to</param>
    public void FlushTo(IConsoleOutput console)
    {
        foreach (var (isError, message) in _lines)
        {
            if (isError)
            {
                console.WriteErrorLine(message);
            }
            else
            {
                console.WriteLine(message);
            }
        }

        _lines.Clear();
    }

    #endregion // Methods

    #region IConsoleOutput

    /// <inheritdoc/>
    public void WriteLine(string message)
    {
        _lines.Add((false, message));
    }

    /// <inheritdoc/>
    public void WriteErrorLine(string message)
    {
        _lines.Add((true, message));
    }

    #endregion // IConsoleOutput
}