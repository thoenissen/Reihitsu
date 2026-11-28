using System;
using System.IO;
using System.Text;

namespace Reihitsu.ArchitectureTests.Helpers;

/// <summary>
/// Creates and cleans up a temporary directory holding the disposable fixture files of one self-test of a scan
/// </summary>
internal sealed class FixtureDirectory : IDisposable
{
    #region Properties

    /// <summary>
    /// Full path of the temporary directory
    /// </summary>
    public string Path { get; } = Directory.CreateTempSubdirectory("ReihitsuArchitectureTest").FullName;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Writes a fixture file as UTF-8 without a byte order mark, creating any intermediate directories it needs
    /// </summary>
    /// <param name="relativePath">File path relative to the temporary directory, with forward slashes or the platform separator</param>
    /// <param name="content">File content</param>
    /// <returns>The full path of the written file</returns>
    public string WriteFile(string relativePath, string content)
    {
        var fullPath = System.IO.Path.Combine(Path, relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
        var directory = System.IO.Path.GetDirectoryName(fullPath);

        if (string.IsNullOrEmpty(directory) == false)
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        return fullPath;
    }

    #endregion // Methods

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }

    #endregion // IDisposable
}