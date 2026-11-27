using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Diagnostics;
using Reihitsu.Cli.Test.Helpers;

namespace Reihitsu.Cli.Test.Unit;

/// <summary>
/// Tests for the file name in the <c>---</c> and <c>+++</c> lines of every phase diff that <see cref="PipelineTraceCommand"/> prints:
/// a file inside the current directory is named by its path relative to that directory behind <c>a/</c> and <c>b/</c>, any other
/// file by its full path without them, as the diff of <c>reihitsu-format --dry-run</c> names it. Every test sets the process's
/// current directory, so the class does not run in parallel with other tests
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class PipelineTraceCommandDiffHeaderTests
{
    #region Constants

    /// <summary>
    /// A file that one formatter phase changes in its first pass
    /// </summary>
    private const string UnformattedSource = "class A\n{\n    int  x;\n}\n";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a file inside the current directory given by its full path is named relative to the current directory
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task FullPathInsideTheCurrentDirectoryPrintsTheRelativeHeader()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile(Path.Combine("work", "src", "A.cs"), UnformattedSource);

            // Act
            var (exitCode, output, error, currentDirectory) = await RunIn(Path.Combine(fixture.Path, "work"), directory => [Path.Combine(directory, "src", "A.cs")]);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            AssertTrace(output, Path.Combine(currentDirectory, "src", "A.cs"), "--- a/src/A.cs", "+++ b/src/A.cs");
        }
    }

    /// <summary>
    /// Verifies that a file given relative to the current directory without dot segments keeps its relative header
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RelativePathInsideTheCurrentDirectoryPrintsTheRelativeHeader()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile(Path.Combine("work", "src", "A.cs"), UnformattedSource);

            // Act
            var (exitCode, output, error, currentDirectory) = await RunIn(Path.Combine(fixture.Path, "work"), _ => ["src/A.cs"]);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            AssertTrace(output, Path.Combine(currentDirectory, "src", "A.cs"), "--- a/src/A.cs", "+++ b/src/A.cs");
        }
    }

    /// <summary>
    /// Verifies that a relative path with a leading <c>./</c> segment is named without it
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RelativePathWithCurrentDirectorySegmentPrintsTheNormalizedHeader()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile(Path.Combine("work", "src", "A.cs"), UnformattedSource);

            // Act
            var (exitCode, output, error, currentDirectory) = await RunIn(Path.Combine(fixture.Path, "work"), _ => ["./src/A.cs"]);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            AssertTrace(output, Path.Combine(currentDirectory, "src", "A.cs"), "--- a/src/A.cs", "+++ b/src/A.cs");
        }
    }

    /// <summary>
    /// Verifies that a relative path with a <c>..</c> segment that lands inside the current directory is named relative to it
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RelativePathWithParentSegmentLandingInsidePrintsTheNormalizedHeader()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile(Path.Combine("work", "src", "A.cs"), UnformattedSource);

            // Act
            var (exitCode, output, error, currentDirectory) = await RunIn(Path.Combine(fixture.Path, "work", "src"), _ => ["../src/A.cs"]);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            AssertTrace(output, Path.Combine(currentDirectory, "A.cs"), "--- a/A.cs", "+++ b/A.cs");
        }
    }

    /// <summary>
    /// Verifies that a file outside the current directory given by a relative path is named by its full path without the prefixes
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RelativePathOutsideTheCurrentDirectoryPrintsTheFullPath()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            Directory.CreateDirectory(Path.Combine(fixture.Path, "work", "src"));
            fixture.CreateFile(Path.Combine("work", "other", "B.cs"), UnformattedSource);

            // Act
            var (exitCode, output, error, currentDirectory) = await RunIn(Path.Combine(fixture.Path, "work", "src"), _ => ["../other/B.cs"]);

            // Assert
            var fullPath = Path.Combine(Path.GetDirectoryName(currentDirectory), "other", "B.cs");

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            AssertTrace(output, fullPath, "--- " + fullPath, "+++ " + fullPath);
        }
    }

    /// <summary>
    /// Verifies that a file outside the current directory given by its full path is named by that full path without the prefixes
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task FullPathOutsideTheCurrentDirectoryPrintsTheFullPath()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            Directory.CreateDirectory(Path.Combine(fixture.Path, "work"));
            fixture.CreateFile(Path.Combine("other", "B.cs"), UnformattedSource);

            // Act
            var (exitCode, output, error, currentDirectory) = await RunIn(Path.Combine(fixture.Path, "work"), directory => [Path.Combine(Path.GetDirectoryName(directory), "other", "B.cs")]);

            // Assert
            var fullPath = Path.Combine(Path.GetDirectoryName(currentDirectory), "other", "B.cs");

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            AssertTrace(output, fullPath, "--- " + fullPath, "+++ " + fullPath);
        }
    }

    /// <summary>
    /// Asserts the unchanged <c>Tracing</c> line and that every phase diff of the trace carries the expected header lines
    /// </summary>
    /// <param name="output">The captured standard output</param>
    /// <param name="fullPath">The full path the <c>Tracing</c> line must name</param>
    /// <param name="oldHeader">The expected <c>---</c> line</param>
    /// <param name="newHeader">The expected <c>+++</c> line</param>
    private static void AssertTrace(string output, string fullPath, string oldHeader, string newHeader)
    {
        var lines = output.Split(Environment.NewLine);
        var oldHeaders = lines.Where(line => line.StartsWith("--- ", StringComparison.Ordinal)).ToArray();
        var newHeaders = lines.Where(line => line.StartsWith("+++ ", StringComparison.Ordinal)).ToArray();

        Assert.AreEqual($"Tracing {fullPath} (maximum passes: 3)", lines[0]);
        Assert.IsNotEmpty(oldHeaders, "The trace should print at least one phase diff.");
        Assert.AreSequenceEqual(Enumerable.Repeat(oldHeader, oldHeaders.Length).ToArray(), oldHeaders);
        Assert.AreSequenceEqual(Enumerable.Repeat(newHeader, oldHeaders.Length).ToArray(), newHeaders);
    }

    /// <summary>
    /// Runs the trace in the given current directory, restoring the previous one afterwards. The arguments are built from the
    /// current directory as the process reports it, so a full path in them is spelled like the process's own paths
    /// </summary>
    /// <param name="directory">The directory to make the current directory</param>
    /// <param name="createArguments">Builds the trace arguments from the current directory as the process reports it</param>
    /// <returns>The exit code, the standard output, the standard error, and the current directory as the process reports it</returns>
    private async Task<(int ExitCode, string Output, string Error, string CurrentDirectory)> RunIn(string directory, Func<string, string[]> createArguments)
    {
        var previousDirectory = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(directory);

            var currentDirectory = Directory.GetCurrentDirectory();
            var output = new StringWriter();
            var error = new StringWriter();
            var exitCode = await PipelineTraceCommand.ExecuteAsync(createArguments(currentDirectory), output, error, TestContext.CancellationToken);

            return (exitCode, output.ToString(), error.ToString(), currentDirectory);
        }
        finally
        {
            Directory.SetCurrentDirectory(previousDirectory);
        }
    }

    #endregion // Methods
}