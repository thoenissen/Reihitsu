using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Abstracts file system operations for testability.
/// </summary>
internal interface IFileSystem
{
    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><see langword="true"/> if the file exists; otherwise, <see langword="false"/>.</returns>
    bool FileExists(string path);

    /// <summary>
    /// Determines whether the specified directory exists.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><see langword="true"/> if the directory exists; otherwise, <see langword="false"/>.</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// Reads the entire content of a file as a string.
    /// </summary>
    /// <param name="path">The path of the file to read.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation, containing the file content.</returns>
    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken);

    /// <summary>
    /// Writes content to a file, creating or overwriting it.
    /// </summary>
    /// <param name="path">The path of the file to write.</param>
    /// <param name="content">The content to write to the file.</param>
    /// <param name="encoding">The encoding to use when writing the file.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteAllTextAsync(string path, string content, Encoding encoding, CancellationToken cancellationToken);

    /// <summary>
    /// Detects the encoding of the specified file.
    /// </summary>
    /// <param name="path">The path of the file to inspect.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous detection operation, containing the file encoding.</returns>
    Task<Encoding> DetectEncodingAsync(string path, CancellationToken cancellationToken);

    /// <summary>
    /// Enumerates files matching a pattern in a directory.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <param name="searchPattern">The search pattern to match against file names.</param>
    /// <param name="searchOption">Specifies whether to search the current directory or all subdirectories.</param>
    /// <returns>An enumerable collection of matching file paths.</returns>
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);

    /// <summary>
    /// Returns the full (absolute) path for the specified path string.
    /// </summary>
    /// <param name="path">The file or directory path.</param>
    /// <returns>The fully qualified path.</returns>
    string GetFullPath(string path);
}