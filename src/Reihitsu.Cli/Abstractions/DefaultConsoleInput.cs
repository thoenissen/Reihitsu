namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Default console input implementation that reads from <see cref="Console"/>
/// </summary>
internal sealed class DefaultConsoleInput : IConsoleInput
{
    #region IConsoleInput

    /// <inheritdoc/>
    public bool IsInteractive => Console.IsInputRedirected == false;

    /// <inheritdoc/>
    public string ReadLine()
    {
        return Console.ReadLine();
    }

    #endregion // IConsoleInput
}