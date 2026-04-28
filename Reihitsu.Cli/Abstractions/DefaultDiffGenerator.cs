namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Default diff generator implementation that delegates to <see cref="DiffGenerator"/>
/// </summary>
internal sealed class DefaultDiffGenerator : IDiffGenerator
{
    #region IDiffGenerator

    /// <inheritdoc/>
    public string Generate(string filePath, string originalContent, string formattedContent)
    {
        return DiffGenerator.Generate(filePath, originalContent, formattedContent);
    }

    #endregion // IDiffGenerator
}