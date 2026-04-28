using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Test.Helpers;

namespace Reihitsu.Cli.Test.EndToEnd;

/// <summary>
/// End-to-end tests for the CLI tool
/// </summary>
[TestClass]
[DoNotParallelize]
[TestCategory("E2E")]
public class CliEndToEndTests
{
    #region Constants

    /// <summary>
    /// Source code that the formatter will change (missing blank line before return statement)
    /// </summary>
    private const string NeedsFormattingSource = "namespace TestProject;\r\n\r\npublic class NeedsFormatting\r\n{\r\n    public int Method()\r\n    {\r\n        var x = 1;\r\n        return x;\r\n    }\r\n}\r\n";

    /// <summary>
    /// Source code for a properly formatted file
    /// </summary>
    private const string FormattedFileTestData = """
                                                 using System;

                                                 namespace TestProject;

                                                 /// <summary>
                                                 /// A formatted class.
                                                 /// </summary>
                                                 public class FormattedClass
                                                 {
                                                     #region Methods

                                                     /// <summary>
                                                     /// A method.
                                                     /// </summary>
                                                     public void Method()
                                                     {
                                                         var value = 42;

                                                         Console.WriteLine(value);
                                                     }

                                                     #endregion // Methods
                                                 }
                                                 """;

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Text context
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the --help flag prints usage information and returns success
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainHelpFlagPrintsUsageAndReturnsSuccess()
    {
        // Act
        int exitCode;
        string output;

        using (var capture = new ConsoleCapture())
        {
            exitCode = await Program.Main(["--help"]);
            output = capture.StandardOutput;
        }

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("reihitsu-format", output);
    }

    /// <summary>
    /// Verifies that the -h short help flag prints usage information and returns success
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainShortHelpFlagPrintsUsageAndReturnsSuccess()
    {
        // Act
        int exitCode;
        string output;

        using (var capture = new ConsoleCapture())
        {
            exitCode = await Program.Main(["-h"]);
            output = capture.StandardOutput;
        }

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("reihitsu-format", output);
    }

    /// <summary>
    /// Verifies that an unknown option prints an error message and returns an error exit code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainUnknownOptionPrintsErrorAndReturnsError()
    {
        // Act
        int exitCode;
        string errorOutput;

        using (var capture = new ConsoleCapture())
        {
            exitCode = await Program.Main(["--unknown"]);
            errorOutput = capture.StandardError;
        }

        // Assert
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.Contains("Unknown option", errorOutput);
    }

    /// <summary>
    /// Verifies that a non-existent path returns an error exit code with an appropriate error message
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainNonExistentPathReturnsError()
    {
        // Act
        int exitCode;
        string errorOutput;

        using (var capture = new ConsoleCapture())
        {
            exitCode = await Program.Main(["/nonexistent/path"]);
            errorOutput = capture.StandardError;
        }

        // Assert
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.Contains("Path not found", errorOutput);
    }

    /// <summary>
    /// Verifies that check mode on already formatted files returns success
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainCheckModeOnFormattedFilesReturnsSuccess()
    {
        // Arrange

        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile("Formatted.cs", FormattedFileTestData);

            // Act
            int exitCode;

            using (new ConsoleCapture())
            {
                exitCode = await Program.Main(["--check", tempDir.Path]);
            }

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
        }
    }

    /// <summary>
    /// Verifies that check mode on an unformatted file returns the formatting needed exit code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainCheckModeOnUnformattedFileReturnsFormattingNeeded()
    {
        // Arrange

        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile("Unformatted.cs", NeedsFormattingSource);

            // Act
            int exitCode;

            using (new ConsoleCapture())
            {
                exitCode = await Program.Main(["--check", tempDir.Path]);
            }

            // Assert
            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
        }
    }

    /// <summary>
    /// Verifies that format mode formats a file and returns success
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainFormatModeFormatsFileAndReturnsSuccess()
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Unformatted.cs", NeedsFormattingSource);

            // Act
            int exitCode;

            using (new ConsoleCapture())
            {
                exitCode = await Program.Main([tempDir.Path]);
            }

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);

            var updatedContent = await File.ReadAllTextAsync(filePath, TestContext.CancellationTokenSource.Token)
                                           .ConfigureAwait(false);

            Assert.AreNotEqual(NeedsFormattingSource, updatedContent);
        }
    }

    /// <summary>
    /// Verifies that dry-run mode shows diff markers and returns the formatting needed exit code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainDryRunShowsDiffAndReturnsFormattingNeeded()
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile("Unformatted.cs", NeedsFormattingSource);

            // Act
            int exitCode;
            string output;

            using (var capture = new ConsoleCapture())
            {
                exitCode = await Program.Main(["--dry-run", tempDir.Path]);
                output = capture.StandardOutput;
            }

            // Assert
            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.Contains("@@", output);
        }
    }

    /// <summary>
    /// Verifies that dry-run mode does not modify the file on disk
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainDryRunDoesNotModifyFile()
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Unformatted.cs", NeedsFormattingSource);

            // Act
            using (new ConsoleCapture())
            {
                await Program.Main(["--dry-run", tempDir.Path]);
            }

            // Assert
            var contentAfterDryRun = await File.ReadAllTextAsync(filePath, TestContext.CancellationTokenSource.Token).ConfigureAwait(false);

            Assert.AreEqual(NeedsFormattingSource, contentAfterDryRun);
        }
    }

    /// <summary>
    /// Verifies that verbose mode shows detailed output for each processed file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainVerboseModeShowsDetailedOutput()
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile("Formatted.cs", FormattedFileTestData);
            tempDir.CreateFile("Unformatted.cs", NeedsFormattingSource);

            // Act
            string output;

            using (var capture = new ConsoleCapture())
            {
                await Program.Main(["--verbose", tempDir.Path]);
                output = capture.StandardOutput;
            }

            // Assert
            Assert.IsTrue(output.Contains("Unchanged:") && output.Contains("Formatted:"));
        }
    }

    /// <summary>
    /// Verifies that when no path argument is provided, the current directory is used
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainDefaultPathUsesCurrentDirectory()
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile("Formatted.cs", FormattedFileTestData);

            var previousDirectory = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(tempDir.Path);

                // Act
                int exitCode;

                using (new ConsoleCapture())
                {
                    exitCode = await Program.Main([]);
                }

                // Assert
                Assert.AreEqual(ExitCodes.Success, exitCode);
            }
            finally
            {
                Directory.SetCurrentDirectory(previousDirectory);
            }
        }
    }

    /// <summary>
    /// Verifies that multiple paths are all processed and included in the summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainMultiplePathsAreAllProcessed()
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile("dir1/File1.cs", FormattedFileTestData);
            tempDir.CreateFile("dir2/File2.cs", NeedsFormattingSource);

            var dir1 = Path.Combine(tempDir.Path, "dir1");
            var dir2 = Path.Combine(tempDir.Path, "dir2");

            // Act
            int exitCode;
            string output;

            using (var capture = new ConsoleCapture())
            {
                exitCode = await Program.Main(["--check", dir1, dir2]);
                output = capture.StandardOutput;
            }

            // Assert
            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.Contains("2", output);
        }
    }

    #endregion // Methods
}