using System.IO;

namespace Reihitsu.Cli.Test.Helpers;

/// <summary>
/// Redirects and captures <see cref="Console.Out"/> and <see cref="Console.Error"/> for the duration of a test
/// </summary>
internal sealed class ConsoleCapture : IDisposable
{
    #region Fields

    /// <summary>
    /// Original standard output writer restored on disposal
    /// </summary>
    private readonly TextWriter _originalOut;

    /// <summary>
    /// Original error writer restored on disposal
    /// </summary>
    private readonly TextWriter _originalError;

    /// <summary>
    /// Captured standard output buffer
    /// </summary>
    private readonly StringWriter _capturedOut;

    /// <summary>
    /// Captured standard error buffer
    /// </summary>
    private readonly StringWriter _capturedError;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleCapture"/> class
    /// </summary>
    public ConsoleCapture()
    {
        _originalOut = Console.Out;
        _originalError = Console.Error;
        _capturedOut = new StringWriter();
        _capturedError = new StringWriter();

        Console.SetOut(_capturedOut);
        Console.SetError(_capturedError);
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Gets the captured standard output
    /// </summary>
    public string StandardOutput => _capturedOut.ToString();

    /// <summary>
    /// Gets the captured standard error output
    /// </summary>
    public string StandardError => _capturedError.ToString();

    #endregion // Properties

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        Console.SetOut(_originalOut);
        Console.SetError(_originalError);

        _capturedOut.Dispose();
        _capturedError.Dispose();
    }

    #endregion // IDisposable
}