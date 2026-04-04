using System.Collections.Generic;
using System.Threading.Tasks;

using Reihitsu.Cli.Abstractions;

namespace Reihitsu.Cli.Test.Helpers;

/// <summary>
/// A test implementation of <see cref="IConsoleOutput"/> that records all output.
/// </summary>
internal sealed class CapturedConsoleOutput : IConsoleOutput
{
    #region Fields

    private readonly List<string> _standardOutput = [];
    private readonly List<string> _errorOutput = [];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Gets the lines written to standard output.
    /// </summary>
    public IReadOnlyList<string> StandardOutput => _standardOutput;

    /// <summary>
    /// Gets the lines written to the error stream.
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
    public Task WriteErrorLineAsync(string message)
    {
        _errorOutput.Add(message);

        return Task.CompletedTask;
    }

    #endregion // IConsoleOutput
}