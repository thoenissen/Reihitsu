using System;
using System.IO;
using System.Text;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Creates and cleans up a temporary directory holding disposable fixture files for one
/// <see cref="SelfReferentialTrackerReferenceTests"/> test
/// </summary>
internal sealed class TrackerReferenceFixtureDirectory : IDisposable
{
    #region Properties

    /// <summary>
    /// Full path of the temporary directory
    /// </summary>
    public string Path { get; } = Directory.CreateTempSubdirectory("ReihitsuTrackerReferenceTest").FullName;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Writes a fixture file, creating any intermediate directories it needs
    /// </summary>
    /// <param name="relativePath">File path relative to the temporary directory</param>
    /// <param name="content">File content</param>
    /// <returns>The full path of the written file</returns>
    public string WriteFile(string relativePath, string content)
    {
        var fullPath = System.IO.Path.Combine(Path, relativePath);
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