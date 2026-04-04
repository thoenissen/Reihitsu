using System.Threading.Tasks;

namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Default console output implementation that writes to <see cref="Console"/>.
/// </summary>
internal sealed class DefaultConsoleOutput : IConsoleOutput
{
    #region IConsoleOutput

    /// <inheritdoc/>
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    /// <inheritdoc/>
    public Task WriteErrorLineAsync(string message)
    {
        return Console.Error.WriteLineAsync(message);
    }

    #endregion // IConsoleOutput
}