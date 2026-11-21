using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Test.Helpers;

namespace Reihitsu.Cli.Test.EndToEnd;

/// <summary>
/// End-to-end tests for the file name in the diff header that <c>--dry-run</c> prints: a file inside the current directory is
/// named by its path relative to that directory behind <c>a/</c> and <c>b/</c>, any other file by its full path without them.
/// Only a run through <see cref="Program.Main"/> sees the process's real current directory, so every test sets it
/// </summary>
[TestClass]
[DoNotParallelize]
[TestCategory("E2E")]
public class CliDryRunDiffHeaderEndToEndTests
{
    #region Constants

    /// <summary>
    /// The unformatted file of the specification examples
    /// </summary>
    private const string UnformattedSource = "class A\n{\n    int  x;\n}\n";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies the specification example of the dry-run mode: a file given relative to the current directory gets the relative
    /// header, while the <c>Would format:</c> line, the hunk, and the summary keep their form
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-013")]
    [Requirement("CLI-024")]
    public async Task SpecExampleRelativePathInsideTheCurrentDirectoryPrintsTheRelativeHeader()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile(Path.Combine("work", "src", "A.cs"), UnformattedSource);

            var (exitCode, output, error, currentDirectory) = await RunIn(Path.Combine(tempDir.Path, "work"), ["--dry-run", Path.Combine("src", "A.cs")]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreSequenceEqual(new[]
                                    {
                                        "Would format: " + Path.Combine(currentDirectory, "src", "A.cs"),
                                        "--- a/src/A.cs",
                                        "+++ b/src/A.cs",
                                        "@@ -1,4 +1,4 @@",
                                        " class A",
                                        " {",
                                        "-    int  x;",
                                        "-}",
                                        "+    int x;",
                                        "+}",
                                        "\\ No newline at end of file",
                                        string.Empty,
                                        string.Empty,
                                        "1 of 1 file(s) would be formatted."
                                    },
                                    Lines(output));
        }
    }

    /// <summary>
    /// Verifies that a file inside the current directory given by its full path gets the same relative header as one given
    /// relative to the current directory
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-024")]
    public async Task FullPathInsideTheCurrentDirectoryPrintsTheRelativeHeader()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var workDirectory = Path.Combine(tempDir.Path, "work");

            tempDir.CreateFile(Path.Combine("work", "src", "A.cs"), UnformattedSource);

            var (exitCode, output, _, currentDirectory) = await RunIn(workDirectory, directory => ["--dry-run", Path.Combine(directory, "src", "A.cs")]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            AssertHeader(output, Path.Combine(currentDirectory, "src", "A.cs"), "--- a/src/A.cs", "+++ b/src/A.cs");
        }
    }

    /// <summary>
    /// Verifies that a file inside the current directory selected through a directory gets the same relative header as one
    /// given relative to the current directory
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-024")]
    public async Task DirectoryInsideTheCurrentDirectoryPrintsTheRelativeHeader()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile(Path.Combine("work", "src", "A.cs"), UnformattedSource);

            var (exitCode, output, _, currentDirectory) = await RunIn(Path.Combine(tempDir.Path, "work"), ["--dry-run", "src" + Path.DirectorySeparatorChar]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            AssertHeader(output, Path.Combine(currentDirectory, "src", "A.cs"), "--- a/src/A.cs", "+++ b/src/A.cs");
        }
    }

    /// <summary>
    /// Verifies that a file directly in the current directory is named by its file name behind the prefixes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-024")]
    public async Task FileDirectlyInTheCurrentDirectoryPrintsItsFileName()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile(Path.Combine("work", "A.cs"), UnformattedSource);

            var (exitCode, output, _, currentDirectory) = await RunIn(Path.Combine(tempDir.Path, "work"), ["--dry-run", "A.cs"]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            AssertHeader(output, Path.Combine(currentDirectory, "A.cs"), "--- a/A.cs", "+++ b/A.cs");
        }
    }

    /// <summary>
    /// Verifies the specification example: a file whose name starts with two dots, directly in the current directory, is inside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-024")]
    public async Task FileNameStartingWithTwoDotsInTheCurrentDirectoryIsInside()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile(Path.Combine("work", "..A.cs"), UnformattedSource);

            var (exitCode, output, _, currentDirectory) = await RunIn(Path.Combine(tempDir.Path, "work"), ["--dry-run", "..A.cs"]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            AssertHeader(output, Path.Combine(currentDirectory, "..A.cs"), "--- a/..A.cs", "+++ b/..A.cs");
        }
    }

    /// <summary>
    /// Verifies the specification example: a file outside the current directory is named by its full path, exactly as the
    /// <c>Would format:</c> line prints it, without the prefixes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-024")]
    public async Task SpecExampleFileOutsideTheCurrentDirectoryPrintsItsFullPath()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            Directory.CreateDirectory(Path.Combine(tempDir.Path, "work", "src"));
            tempDir.CreateFile(Path.Combine("work", "other", "B.cs"), UnformattedSource);

            var (exitCode, output, _, currentDirectory) = await RunIn(Path.Combine(tempDir.Path, "work", "src"), ["--dry-run", Path.Combine("..", "other", "B.cs")]);

            var fullPath = Path.Combine(Path.GetDirectoryName(currentDirectory), "other", "B.cs");

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            AssertHeader(output, fullPath, "--- " + fullPath, "+++ " + fullPath);
        }
    }

    /// <summary>
    /// Verifies the specification example: a file in a sibling directory whose name only starts with the current directory's name
    /// is outside the current directory
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-024")]
    public async Task FileInSiblingDirectoryWithTheSameNamePrefixIsOutside()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            Directory.CreateDirectory(Path.Combine(tempDir.Path, "work"));
            tempDir.CreateFile(Path.Combine("work-old", "A.cs"), UnformattedSource);

            var (exitCode, output, _, currentDirectory) = await RunIn(Path.Combine(tempDir.Path, "work"), directory => ["--dry-run", $"{directory}-old{Path.DirectorySeparatorChar}A.cs"]);

            var fullPath = $"{currentDirectory}-old{Path.DirectorySeparatorChar}A.cs";

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            AssertHeader(output, fullPath, "--- " + fullPath, "+++ " + fullPath);
        }
    }

    /// <summary>
    /// Verifies that each file of one run gets its own header: the file inside the current directory the relative one, the file
    /// outside it the full path
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-024")]
    public async Task InsideAndOutsideFilesInOneRunGetTheirOwnHeaders()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile(Path.Combine("work", "src", "A.cs"), UnformattedSource);
            tempDir.CreateFile(Path.Combine("work", "other", "B.cs"), UnformattedSource);

            var (exitCode, output, _, currentDirectory) = await RunIn(Path.Combine(tempDir.Path, "work", "src"), ["--dry-run", "A.cs", Path.Combine("..", "other", "B.cs")]);

            var insidePath = Path.Combine(currentDirectory, "A.cs");
            var outsidePath = Path.Combine(Path.GetDirectoryName(currentDirectory), "other", "B.cs");
            var lines = Lines(output);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.AreSequenceEqual(new[]
                                    {
                                        "Would format: " + insidePath,
                                        "--- a/A.cs",
                                        "+++ b/A.cs",
                                        "Would format: " + outsidePath,
                                        "--- " + outsidePath,
                                        "+++ " + outsidePath
                                    },
                                    lines.Where(line => line.StartsWith("Would format: ", StringComparison.Ordinal)
                                                        || line.StartsWith("--- ", StringComparison.Ordinal)
                                                        || line.StartsWith("+++ ", StringComparison.Ordinal))
                                         .ToArray());
            Assert.AreEqual("2 of 2 file(s) would be formatted.", lines[^1]);
        }
    }

    /// <summary>
    /// Verifies that a file is inside the current directory when that directory is the root of the file system, so the relative
    /// path is the full path without the root
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-024")]
    public async Task FileBelowTheRootDirectoryIsInsideWhenTheRootIsTheCurrentDirectory()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile(Path.Combine("work", "src", "A.cs"), UnformattedSource);
            var root = Path.GetPathRoot(Path.GetFullPath(filePath));

            var (exitCode, output, _, currentDirectory) = await RunIn(root, ["--dry-run", filePath]);

            var relativePath = filePath.Substring(currentDirectory.Length).Replace(Path.DirectorySeparatorChar, '/');

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.IsFalse(relativePath.StartsWith('/'), $"Precondition: the root must end with a separator, got '{currentDirectory}'.");
            AssertHeader(output, filePath, "--- a/" + relativePath, "+++ b/" + relativePath);
        }
    }

    /// <summary>
    /// Verifies the specification example on Linux: directory names are compared case-sensitively, so a file below <c>work</c>
    /// is outside the current directory <c>Work</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [OSCondition(OperatingSystems.Linux)]
    [Requirement("CLI-024")]
    public async Task OnLinuxADirectoryDifferingOnlyInCaseIsAnotherDirectory()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile(Path.Combine("work", "src", "A.cs"), UnformattedSource);

            Directory.CreateDirectory(Path.Combine(tempDir.Path, "Work"));

            var (exitCode, output, _, _) = await RunIn(Path.Combine(tempDir.Path, "Work"), ["--dry-run", filePath]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            AssertHeader(output, filePath, "--- " + filePath, "+++ " + filePath);
        }
    }

    /// <summary>
    /// Verifies the specification example on Windows: directory names are compared case-insensitively, so a file below
    /// <c>work</c> is inside the current directory <c>Work</c>, and the relative path is written with <c>/</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [OSCondition(OperatingSystems.Windows)]
    [Requirement("CLI-024")]
    public async Task OnWindowsADirectoryDifferingOnlyInCaseIsTheSameDirectory()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile(Path.Combine("work", "src", "A.cs"), UnformattedSource);

            var (exitCode, output, _, _) = await RunIn(Path.Combine(tempDir.Path, "Work"), ["--dry-run", filePath]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            AssertHeader(output, filePath, "--- a/src/A.cs", "+++ b/src/A.cs");
        }
    }

    /// <summary>
    /// Verifies that on Linux, where a backslash is no directory separator, a backslash in a file name stays in the relative header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [OSCondition(OperatingSystems.Linux)]
    [Requirement("CLI-024")]
    public async Task OnLinuxABackslashInAFileNameIsKeptInTheRelativeHeader()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile(Path.Combine("work", "src", "x\\y.cs"), UnformattedSource);

            var (exitCode, output, _, currentDirectory) = await RunIn(Path.Combine(tempDir.Path, "work"), ["--dry-run", Path.Combine("src", "x\\y.cs")]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            AssertHeader(output, Path.Combine(currentDirectory, "src", "x\\y.cs"), "--- a/src/x\\y.cs", "+++ b/src/x\\y.cs");
        }
    }

    /// <summary>
    /// Verifies the specification example: when the current directory is reached through a symbolic link, the process reports the
    /// physical directory, so a full path through the link names the file outside it, while a relative path names it inside
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [OSCondition(OperatingSystems.Linux | OperatingSystems.OSX)]
    [Requirement("CLI-024")]
    public async Task FullPathThroughASymbolicLinkIntoTheCurrentDirectoryIsOutside()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile(Path.Combine("real", "work", "src", "A.cs"), UnformattedSource);

            var linkDirectory = Path.Combine(tempDir.Path, "link");

            Directory.CreateSymbolicLink(linkDirectory, Path.Combine(tempDir.Path, "real"));

            var linkedPath = Path.Combine(linkDirectory, "work", "src", "A.cs");

            var (exitCode, output, _, currentDirectory) = await RunIn(Path.Combine(linkDirectory, "work"), ["--dry-run", linkedPath]);

            Assert.IsFalse(currentDirectory.StartsWith(linkDirectory, StringComparison.Ordinal), $"Precondition: the process must report the physical directory, got '{currentDirectory}'.");
            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            AssertHeader(output, linkedPath, "--- " + linkedPath, "+++ " + linkedPath);
        }
    }

    /// <summary>
    /// Verifies the specification example: a relative path, given in a current directory reached through a symbolic link, names
    /// the file inside the current directory
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [OSCondition(OperatingSystems.Linux | OperatingSystems.OSX)]
    [Requirement("CLI-024")]
    public async Task RelativePathInACurrentDirectoryReachedThroughASymbolicLinkIsInside()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile(Path.Combine("real", "work", "src", "A.cs"), UnformattedSource);

            var linkDirectory = Path.Combine(tempDir.Path, "link");

            Directory.CreateSymbolicLink(linkDirectory, Path.Combine(tempDir.Path, "real"));

            var (exitCode, output, _, currentDirectory) = await RunIn(Path.Combine(linkDirectory, "work"), ["--dry-run", Path.Combine("src", "A.cs")]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            AssertHeader(output, Path.Combine(currentDirectory, "src", "A.cs"), "--- a/src/A.cs", "+++ b/src/A.cs");
        }
    }

    /// <summary>
    /// Verifies that a file given by its full path, while the current directory has been deleted, is outside the current
    /// directory: the dry run prints its diff with the full-path header and reports that formatting is needed, without an error
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [OSCondition(OperatingSystems.Linux)]
    [Requirement("CLI-013")]
    [Requirement("CLI-024")]
    public async Task FullPathToAFileWhileTheCurrentDirectoryIsDeletedPrintsTheFullPathHeader()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile(Path.Combine("work", "src", "A.cs"), UnformattedSource);

            var (exitCode, output, error) = await RunInDeletedDirectory(Path.Combine(tempDir.Path, "gone"), ["--dry-run", filePath]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.AreEqual(string.Empty, error);
            AssertWholeOutput(output, filePath);
        }
    }

    /// <summary>
    /// Verifies that a file selected through a directory given by its full path, while the current directory has been deleted, is
    /// outside the current directory: the dry run prints its diff with the full-path header and reports that formatting is needed,
    /// without an error
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [OSCondition(OperatingSystems.Linux)]
    [Requirement("CLI-013")]
    [Requirement("CLI-024")]
    public async Task FullPathToADirectoryWhileTheCurrentDirectoryIsDeletedPrintsTheFullPathHeader()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile(Path.Combine("work", "src", "A.cs"), UnformattedSource);

            var (exitCode, output, error) = await RunInDeletedDirectory(Path.Combine(tempDir.Path, "gone"), ["--dry-run", Path.Combine(tempDir.Path, "work", "src")]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.AreEqual(string.Empty, error);
            AssertWholeOutput(output, filePath);
        }
    }

    /// <summary>
    /// Asserts the whole standard output of a dry run over the unformatted file of the specification examples, named outside the
    /// current directory by its full path
    /// </summary>
    /// <param name="output">The captured standard output</param>
    /// <param name="filePath">The full path of the file</param>
    private static void AssertWholeOutput(string output, string filePath)
    {
        Assert.AreSequenceEqual(new[]
                                {
                                    "Would format: " + filePath,
                                    "--- " + filePath,
                                    "+++ " + filePath,
                                    "@@ -1,4 +1,4 @@",
                                    " class A",
                                    " {",
                                    "-    int  x;",
                                    "-}",
                                    "+    int x;",
                                    "+}",
                                    "\\ No newline at end of file",
                                    string.Empty,
                                    string.Empty,
                                    "1 of 1 file(s) would be formatted."
                                },
                                Lines(output));
    }

    /// <summary>
    /// Runs the CLI with a current directory that has been deleted, restoring the previous current directory afterwards
    /// </summary>
    /// <param name="directory">The directory to make the current directory and then delete</param>
    /// <param name="arguments">The command-line arguments</param>
    /// <returns>The exit code, the standard output, and the standard error</returns>
    private static async Task<(int ExitCode, string Output, string Error)> RunInDeletedDirectory(string directory, string[] arguments)
    {
        var previousDirectory = Directory.GetCurrentDirectory();

        try
        {
            Directory.CreateDirectory(directory);
            Directory.SetCurrentDirectory(directory);
            Directory.Delete(directory);

            Assert.Throws<IOException>(() => Directory.GetCurrentDirectory(), "Precondition: the process must be unable to read its deleted current directory.");

            using (var capture = new ConsoleCapture())
            {
                var exitCode = await Program.Main(arguments);

                return (exitCode, capture.StandardOutput, capture.StandardError);
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(previousDirectory);
        }
    }

    /// <summary>
    /// Asserts the <c>Would format:</c> line and the two header lines that follow it
    /// </summary>
    /// <param name="output">The captured standard output</param>
    /// <param name="wouldFormatPath">The path the <c>Would format:</c> line must name</param>
    /// <param name="oldHeader">The expected <c>---</c> line</param>
    /// <param name="newHeader">The expected <c>+++</c> line</param>
    private static void AssertHeader(string output, string wouldFormatPath, string oldHeader, string newHeader)
    {
        Assert.AreSequenceEqual(new[] { "Would format: " + wouldFormatPath, oldHeader, newHeader }, Lines(output).Take(3).ToArray());
    }

    /// <summary>
    /// Runs the CLI in the given current directory, restoring the previous one afterwards
    /// </summary>
    /// <param name="directory">The directory to make the current directory</param>
    /// <param name="arguments">The command-line arguments</param>
    /// <returns>The exit code, the standard output, the standard error, and the current directory as the process reports it</returns>
    private static Task<(int ExitCode, string Output, string Error, string CurrentDirectory)> RunIn(string directory, string[] arguments)
    {
        return RunIn(directory, _ => arguments);
    }

    /// <summary>
    /// Runs the CLI in the given current directory, restoring the previous one afterwards. The arguments are built from the
    /// current directory as the process reports it, so a full path in them is spelled like the process's own paths
    /// </summary>
    /// <param name="directory">The directory to make the current directory</param>
    /// <param name="createArguments">Builds the command-line arguments from the current directory as the process reports it</param>
    /// <returns>The exit code, the standard output, the standard error, and the current directory as the process reports it</returns>
    private static async Task<(int ExitCode, string Output, string Error, string CurrentDirectory)> RunIn(string directory, Func<string, string[]> createArguments)
    {
        var previousDirectory = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(directory);

            var currentDirectory = Directory.GetCurrentDirectory();

            using (var capture = new ConsoleCapture())
            {
                var exitCode = await Program.Main(createArguments(currentDirectory));

                return (exitCode, capture.StandardOutput, capture.StandardError, currentDirectory);
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(previousDirectory);
        }
    }

    /// <summary>
    /// Splits captured output into its lines, without the empty line behind the last line break
    /// </summary>
    /// <param name="text">The captured output</param>
    /// <returns>The lines</returns>
    private static string[] Lines(string text)
    {
        return text.Split(Environment.NewLine).SkipLast(1).ToArray();
    }

    #endregion // Methods
}