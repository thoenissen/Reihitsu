using System;
using System.IO;
using System.Text;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Creates and cleans up a temporary directory holding disposable fixture files for one
/// <see cref="ApplyFixWrapperPositionalArgumentTests"/> test
/// </summary>
internal sealed class ApplyFixWrapperFixtureDirectoryScope : IDisposable
{
    #region Properties

    /// <summary>
    /// Full path of the temporary directory
    /// </summary>
    public string Path { get; }

    #endregion // Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplyFixWrapperFixtureDirectoryScope"/> class
    /// </summary>
    public ApplyFixWrapperFixtureDirectoryScope()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ReihitsuApplyFixWrapperTest", Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(Path);
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Creates a fixture subdirectory containing one trivial C# file
    /// </summary>
    /// <param name="relativeDirectory">Directory name relative to the scope, which may itself look like an option</param>
    /// <param name="fileName">Fixture file name</param>
    /// <returns>The full path of the created subdirectory</returns>
    public string CreateFixture(string relativeDirectory, string fileName)
    {
        var directory = System.IO.Path.Combine(Path, relativeDirectory);

        Directory.CreateDirectory(directory);
        File.WriteAllText(System.IO.Path.Combine(directory, fileName), "internal class Sample\n{\n}\n", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        return directory;
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