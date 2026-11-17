using System;
using System.IO;
using System.Text;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// A temporary repository-like directory holding the fixture build files of one
/// <see cref="RoslynPackageVersionTests"/> test
/// </summary>
internal sealed class RoslynVersionFixtureDirectory : IDisposable
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="RoslynVersionFixtureDirectory"/> class
    /// </summary>
    public RoslynVersionFixtureDirectory()
    {
        Root = Path.Combine(Path.GetTempPath(), "ReihitsuRoslynVersionTest", Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(Root);
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Full path of the fixture root
    /// </summary>
    public string Root { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Writes a fixture file
    /// </summary>
    /// <param name="relativePath">Path relative to the fixture root, with forward slashes</param>
    /// <param name="content">File content</param>
    public void Write(string relativePath, string content)
    {
        var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    #endregion // Methods

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Directory.Exists(Root))
        {
            Directory.Delete(Root, recursive: true);
        }
    }

    #endregion // IDisposable
}