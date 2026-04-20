using System.IO;
using System.Text;

namespace Reihitsu.Cli.Test.Helpers;

/// <summary>
/// Creates and cleans up temporary directories for test isolation.
/// </summary>
internal sealed class TemporaryDirectoryFixture : IDisposable
{
    #region Properties

    /// <summary>
    /// Gets the full path of the temporary directory.
    /// </summary>
    public string Path { get; }

    #endregion // Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TemporaryDirectoryFixture"/> class.
    /// </summary>
    public TemporaryDirectoryFixture()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ReihitsuCliTest", Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(Path);
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Creates a file with the specified content in the temporary directory.
    /// </summary>
    /// <param name="relativePath">The relative path within the temporary directory.</param>
    /// <param name="content">The file content.</param>
    /// <returns>The full path of the created file.</returns>
    public string CreateFile(string relativePath, string content)
    {
        return CreateFile(relativePath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    /// <summary>
    /// Creates a file with the specified content and encoding in the temporary directory.
    /// </summary>
    /// <param name="relativePath">The relative path within the temporary directory.</param>
    /// <param name="content">The file content.</param>
    /// <param name="encoding">The file encoding.</param>
    /// <returns>The full path of the created file.</returns>
    public string CreateFile(string relativePath, string content, Encoding encoding)
    {
        var fullPath = System.IO.Path.Combine(Path, relativePath);
        var directory = System.IO.Path.GetDirectoryName(fullPath)!;

        Directory.CreateDirectory(directory);
        File.WriteAllText(fullPath, content, encoding);

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