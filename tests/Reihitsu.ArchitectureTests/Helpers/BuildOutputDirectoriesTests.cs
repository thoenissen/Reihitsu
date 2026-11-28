using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.ArchitectureTests.Helpers;

/// <summary>
/// Tests for <see cref="BuildOutputDirectories"/>, the one decision of which scanned files are build output
/// </summary>
[TestClass]
public sealed class BuildOutputDirectoriesTests
{
    #region Fields

    /// <summary>
    /// Root directory the checked paths lie below; only its path is used, nothing is read or written
    /// </summary>
    private static readonly string _root = Path.Combine(Path.GetTempPath(), "ReihitsuBuildOutputTest");

    #endregion // Fields

    #region Tests

    /// <summary>
    /// Verifies that a file below a <c>bin</c> or an <c>obj</c> directory is build output
    /// </summary>
    /// <param name="directoryName">Name of the build output directory</param>
    [TestMethod]
    [DataRow("bin")]
    [DataRow("obj")]
    public void FileBelowBuildOutputDirectoryIsBuildOutput(string directoryName)
    {
        // Act
        var contains = BuildOutputDirectories.Contains(_root, Path.Combine(_root, "Project", directoryName, "Release", "Sample.cs"));

        // Assert
        Assert.IsTrue(contains);
    }

    /// <summary>
    /// Verifies that a file directly in a <c>bin</c> or an <c>obj</c> directory below the root is build output
    /// </summary>
    /// <param name="directoryName">Name of the build output directory</param>
    [TestMethod]
    [DataRow("bin")]
    [DataRow("obj")]
    public void FileDirectlyInBuildOutputDirectoryIsBuildOutput(string directoryName)
    {
        // Act
        var contains = BuildOutputDirectories.Contains(_root, Path.Combine(_root, directoryName, "Sample.cs"));

        // Assert
        Assert.IsTrue(contains);
    }

    /// <summary>
    /// Verifies that a file outside any build output directory is not build output
    /// </summary>
    [TestMethod]
    public void FileOutsideBuildOutputDirectoryIsNotBuildOutput()
    {
        // Act
        var contains = BuildOutputDirectories.Contains(_root, Path.Combine(_root, "Project", "Sample.cs"));

        // Assert
        Assert.IsFalse(contains);
    }

    /// <summary>
    /// Verifies that the directory names are compared case-insensitively
    /// </summary>
    /// <param name="directoryName">Name of the build output directory in another casing</param>
    [TestMethod]
    [DataRow("Bin")]
    [DataRow("BIN")]
    [DataRow("Obj")]
    [DataRow("OBJ")]
    public void BuildOutputDirectoryInOtherCasingIsBuildOutput(string directoryName)
    {
        // Act
        var contains = BuildOutputDirectories.Contains(_root, Path.Combine(_root, "Project", directoryName, "Sample.cs"));

        // Assert
        Assert.IsTrue(contains);
    }

    /// <summary>
    /// Verifies that a directory whose name only starts or ends with <c>bin</c> or <c>obj</c> is not build output
    /// </summary>
    /// <param name="directoryName">Name of a directory that is not build output</param>
    [TestMethod]
    [DataRow("binaries")]
    [DataRow("objects")]
    [DataRow("cabin")]
    [DataRow("_obj")]
    public void DirectoryNamedLikeBuildOutputIsNotBuildOutput(string directoryName)
    {
        // Act
        var contains = BuildOutputDirectories.Contains(_root, Path.Combine(_root, directoryName, "Sample.cs"));

        // Assert
        Assert.IsFalse(contains);
    }

    /// <summary>
    /// Verifies that a file named <c>bin</c> or <c>obj</c> is not build output, since only directories count
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    [TestMethod]
    [DataRow("bin")]
    [DataRow("obj")]
    public void FileNamedLikeBuildOutputIsNotBuildOutput(string fileName)
    {
        // Act
        var contains = BuildOutputDirectories.Contains(_root, Path.Combine(_root, "Project", fileName));

        // Assert
        Assert.IsFalse(contains);
    }

    /// <summary>
    /// Verifies that a <c>bin</c> or <c>obj</c> directory above the scanned root does not make the files below
    /// the root build output
    /// </summary>
    /// <param name="directoryName">Name of the build output directory above the root</param>
    [TestMethod]
    [DataRow("bin")]
    [DataRow("obj")]
    public void BuildOutputDirectoryAboveRootIsIgnored(string directoryName)
    {
        // Arrange
        var root = Path.Combine(_root, directoryName, "Checkout");

        // Act
        var contains = BuildOutputDirectories.Contains(root, Path.Combine(root, "Project", "Sample.cs"));

        // Assert
        Assert.IsFalse(contains);
    }

    /// <summary>
    /// Verifies that a file path written with forward slashes is split into its directories as well
    /// </summary>
    [TestMethod]
    public void ForwardSlashPathIsSplitIntoDirectories()
    {
        // Act
        var contains = BuildOutputDirectories.Contains(_root, _root + "/Project/obj/Sample.cs");

        // Assert
        Assert.IsTrue(contains);
    }

    /// <summary>
    /// Verifies which single directory names are build output directory names
    /// </summary>
    /// <param name="directoryName">Directory name to check</param>
    /// <param name="expected">Whether the name is a build output directory name</param>
    [TestMethod]
    [DataRow("bin", true)]
    [DataRow("obj", true)]
    [DataRow("Bin", true)]
    [DataRow("OBJ", true)]
    [DataRow(".git", false)]
    [DataRow("binaries", false)]
    [DataRow("src", false)]
    public void IsBuildOutputDirectoryNameDecidesSingleNames(string directoryName, bool expected)
    {
        // Act
        var isBuildOutput = BuildOutputDirectories.IsBuildOutputDirectoryName(directoryName);

        // Assert
        Assert.AreEqual(expected, isBuildOutput);
    }

    #endregion // Tests
}