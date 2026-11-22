using System.Collections.Generic;
using System.Threading;

using Reihitsu.Cli.Abstractions;

namespace Reihitsu.Cli.Test.Helpers;

/// <summary>
/// A test implementation of <see cref="IConsoleOutput"/> that records all output, per stream and as one sequence across both
/// streams; it may be written from several threads
/// </summary>
internal sealed class CapturedConsoleOutput : IConsoleOutput
{
    #region Constants

    /// <summary>
    /// Prefix of a standard output line in <see cref="InterleavedOutput"/>
    /// </summary>
    public const string StandardOutputPrefix = "stdout: ";

    /// <summary>
    /// Prefix of a standard error line in <see cref="InterleavedOutput"/>
    /// </summary>
    public const string ErrorOutputPrefix = "stderr: ";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Guards the captured lists against concurrent writers
    /// </summary>
    private readonly Lock _lock = new();

    /// <summary>
    /// Captured lines written to standard output
    /// </summary>
    private readonly List<string> _standardOutput = [];

    /// <summary>
    /// Captured lines written to standard error
    /// </summary>
    private readonly List<string> _errorOutput = [];

    /// <summary>
    /// Captured lines of both streams, in the order they were written, each prefixed with its stream
    /// </summary>
    private readonly List<string> _interleavedOutput = [];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Lines written to standard output
    /// </summary>
    public IReadOnlyList<string> StandardOutput
    {
        get
        {
            lock (_lock)
            {
                return _standardOutput.ToArray();
            }
        }
    }

    /// <summary>
    /// Lines written to the error stream
    /// </summary>
    public IReadOnlyList<string> ErrorOutput
    {
        get
        {
            lock (_lock)
            {
                return _errorOutput.ToArray();
            }
        }
    }

    /// <summary>
    /// Lines of both streams in the order they were written, each prefixed with <see cref="StandardOutputPrefix"/> or
    /// <see cref="ErrorOutputPrefix"/>
    /// </summary>
    public IReadOnlyList<string> InterleavedOutput
    {
        get
        {
            lock (_lock)
            {
                return _interleavedOutput.ToArray();
            }
        }
    }

    #endregion // Properties

    #region IConsoleOutput

    /// <inheritdoc/>
    public void WriteLine(string message)
    {
        lock (_lock)
        {
            _standardOutput.Add(message);
            _interleavedOutput.Add(StandardOutputPrefix + message);
        }
    }

    /// <inheritdoc/>
    public void WriteErrorLine(string message)
    {
        lock (_lock)
        {
            _errorOutput.Add(message);
            _interleavedOutput.Add(ErrorOutputPrefix + message);
        }
    }

    #endregion // IConsoleOutput
}