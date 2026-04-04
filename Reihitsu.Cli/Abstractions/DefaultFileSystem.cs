using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Default file system implementation that delegates to <see cref="File"/>, <see cref="Directory"/>, and <see cref="Path"/>.
/// </summary>
internal sealed class DefaultFileSystem : IFileSystem
{
    #region IFileSystem

    /// <inheritdoc/>
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <inheritdoc/>
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    /// <inheritdoc/>
    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken)
    {
        return File.ReadAllTextAsync(path, cancellationToken);
    }

    /// <inheritdoc/>
    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken)
    {
        return File.WriteAllTextAsync(path, content, cancellationToken);
    }

    /// <inheritdoc/>
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.EnumerateFiles(path, searchPattern, searchOption);
    }

    /// <inheritdoc/>
    public string GetFullPath(string path)
    {
        return Path.GetFullPath(path);
    }

    #endregion // IFileSystem
}