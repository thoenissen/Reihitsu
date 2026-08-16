using System.IO;
using System.Text;
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
    /// Verifies that the --help flag prints usage information, documents dry-run exit behavior, and returns success
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
        Assert.Contains("exit code 1 if changes would be made", output);
        Assert.Contains("--utf8-bom", output);
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
    /// Verifies that the --version flag prints the tool name and version, and returns success
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainVersionFlagPrintsVersionAndReturnsSuccess()
    {
        // Act
        int exitCode;
        string output;

        using (var capture = new ConsoleCapture())
        {
            exitCode = await Program.Main(["--version"]);
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
    /// Verifies that the usage text for an unknown option is written to standard error rather than standard output
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainUnknownOptionWritesUsageToStandardError()
    {
        // Act
        string standardOutput;
        string errorOutput;

        using (var capture = new ConsoleCapture())
        {
            await Program.Main(["--unknown"]);
            standardOutput = capture.StandardOutput;
            errorOutput = capture.StandardError;
        }

        // Assert
        Assert.Contains("reihitsu-format", errorOutput);
        Assert.DoesNotContain("reihitsu-format", standardOutput);
    }

    /// <summary>
    /// Verifies that combining <c>--check</c> and <c>--dry-run</c> is rejected as an argument error
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainCheckAndDryRunCombinedReturnsError()
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile("Formatted.cs", FormattedFileTestData);

            // Act
            int exitCode;
            string errorOutput;

            using (var capture = new ConsoleCapture())
            {
                exitCode = await Program.Main(["--check", "--dry-run", tempDir.Path]);
                errorOutput = capture.StandardError;
            }

            // Assert
            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.Contains("cannot be combined", errorOutput);
        }
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

            var updatedContent = await File.ReadAllTextAsync(filePath, TestContext.CancellationToken)
                                           .ConfigureAwait(false);

            Assert.AreNotEqual(NeedsFormattingSource, updatedContent);
        }
    }

    /// <summary>
    /// Verifies that dry-run mode on already formatted files returns success without producing a diff
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainDryRunModeOnFormattedFilesReturnsSuccess()
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            tempDir.CreateFile("Formatted.cs", FormattedFileTestData);

            // Act
            int exitCode;
            string output;

            using (var capture = new ConsoleCapture())
            {
                exitCode = await Program.Main(["--dry-run", tempDir.Path]);
                output = capture.StandardOutput;
            }

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.DoesNotContain("@@", output);
        }
    }

    /// <summary>
    /// Verifies that <c>--utf8-bom</c> normalizes a content-clean file and that a subsequent check is idempotent
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainUtf8BomNormalizesContentCleanFileAndIsIdempotent()
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Formatted.cs", FormattedFileTestData);

            // Act
            int formatExitCode;
            int checkExitCode;

            using (new ConsoleCapture())
            {
                formatExitCode = await Program.Main(["--utf8-bom", filePath]);
                checkExitCode = await Program.Main(["--check", "--utf8-bom", filePath]);
            }

            // Assert
            Assert.AreEqual(ExitCodes.Success, formatExitCode);
            Assert.AreEqual(ExitCodes.Success, checkExitCode);

            var fileBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);
            var utf8Bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetPreamble();

            Assert.IsTrue(fileBytes.AsSpan().StartsWith(utf8Bom));
        }
    }

    /// <summary>
    /// Verifies that <c>--check --utf8-bom</c> reports an encoding-only change without modifying the file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainCheckUtf8BomReportsEncodingOnlyChangeWithoutWriting()
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Formatted.cs", FormattedFileTestData);
            var originalBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            // Act
            int exitCode;
            string output;

            using (var capture = new ConsoleCapture())
            {
                exitCode = await Program.Main(["--check", "--utf8-bom", filePath]);
                output = capture.StandardOutput;
            }

            // Assert
            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.Contains("Not formatted:", output);

            var actualBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            Assert.AreSequenceEqual(originalBytes, actualBytes);
        }
    }

    /// <summary>
    /// Verifies that <c>--dry-run --utf8-bom</c> reports an encoding-only change without modifying the file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainDryRunUtf8BomReportsEncodingOnlyChangeWithoutWriting()
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Formatted.cs", FormattedFileTestData);
            var originalBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            // Act
            int exitCode;
            string output;

            using (var capture = new ConsoleCapture())
            {
                exitCode = await Program.Main(["--dry-run", "--utf8-bom", filePath]);
                output = capture.StandardOutput;
            }

            // Assert
            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.Contains("Would format:", output);

            var actualBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            Assert.AreSequenceEqual(originalBytes, actualBytes);
        }
    }

    /// <summary>
    /// Verifies that the --force flag formats more files than the confirmation threshold without prompting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainForceFlagFormatsLargeRunWithoutPrompting()
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var fileCount = FormatCommandHandler.LargeRunConfirmationThreshold + 1;
            var filePaths = new string[fileCount];

            for (var index = 0; index < fileCount; index++)
            {
                filePaths[index] = tempDir.CreateFile($"Unformatted{index}.cs", NeedsFormattingSource);
            }

            // Act
            int exitCode;

            using (new ConsoleCapture())
            {
                exitCode = await Program.Main(["--force", tempDir.Path]);
            }

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);

            foreach (var filePath in filePaths)
            {
                var updatedContent = await File.ReadAllTextAsync(filePath, TestContext.CancellationToken)
                                               .ConfigureAwait(false);

                Assert.AreNotEqual(NeedsFormattingSource, updatedContent);
            }
        }
    }

    /// <summary>
    /// Verifies that check mode never prompts for confirmation, even above the large run threshold
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task MainCheckModeLargeRunDoesNotPrompt()
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var fileCount = FormatCommandHandler.LargeRunConfirmationThreshold + 1;

            for (var index = 0; index < fileCount; index++)
            {
                tempDir.CreateFile($"Unformatted{index}.cs", NeedsFormattingSource);
            }

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
            var contentAfterDryRun = await File.ReadAllTextAsync(filePath, TestContext.CancellationToken).ConfigureAwait(false);

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
    /// Verifies that explicitly targeted bin and obj directories are processed for every supported path spelling
    /// </summary>
    /// <param name="directoryName">The build-output directory name</param>
    /// <param name="targetSpelling">The path spelling to pass to the CLI</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [DataRow("bin", "relative")]
    [DataRow("bin", "dot-relative")]
    [DataRow("bin", "absolute")]
    [DataRow("obj", "relative")]
    [DataRow("obj", "dot-relative")]
    [DataRow("obj", "absolute")]
    public async Task MainExplicitBuildOutputDirectoryTargetFormatsFile(string directoryName, string targetSpelling)
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile(Path.Combine(directoryName, "Unformatted.cs"), NeedsFormattingSource);
            var targetPath = CreateTargetPath(tempDir.Path, directoryName, targetSpelling);
            var previousDirectory = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(tempDir.Path);

                // Act
                int exitCode;

                using (new ConsoleCapture())
                {
                    exitCode = await Program.Main([targetPath]);
                }

                // Assert
                var actualContent = await File.ReadAllTextAsync(filePath, TestContext.CancellationToken);

                Assert.AreEqual(ExitCodes.Success, exitCode);
                Assert.AreNotEqual(NeedsFormattingSource, actualContent);
            }
            finally
            {
                Directory.SetCurrentDirectory(previousDirectory);
            }
        }
    }

    /// <summary>
    /// Verifies that explicitly targeted files inside bin and obj directories are processed for every supported path
    /// spelling
    /// </summary>
    /// <param name="directoryName">The build-output directory name</param>
    /// <param name="targetSpelling">The path spelling to pass to the CLI</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [DataRow("bin", "relative")]
    [DataRow("bin", "dot-relative")]
    [DataRow("bin", "absolute")]
    [DataRow("obj", "relative")]
    [DataRow("obj", "dot-relative")]
    [DataRow("obj", "absolute")]
    public async Task MainExplicitBuildOutputFileTargetFormatsFile(string directoryName, string targetSpelling)
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var relativeFilePath = Path.Combine(directoryName, "Unformatted.cs");
            var filePath = tempDir.CreateFile(relativeFilePath, NeedsFormattingSource);
            var targetPath = CreateTargetPath(tempDir.Path, relativeFilePath, targetSpelling);
            var previousDirectory = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(tempDir.Path);

                // Act
                int exitCode;

                using (new ConsoleCapture())
                {
                    exitCode = await Program.Main([targetPath]);
                }

                // Assert
                var actualContent = await File.ReadAllTextAsync(filePath, TestContext.CancellationToken);

                Assert.AreEqual(ExitCodes.Success, exitCode);
                Assert.AreNotEqual(NeedsFormattingSource, actualContent);
            }
            finally
            {
                Directory.SetCurrentDirectory(previousDirectory);
            }
        }
    }

    /// <summary>
    /// Verifies that recursive discovery formats directories whose names merely contain bin or obj
    /// </summary>
    /// <param name="directoryName">The ordinary directory name to discover</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [DataRow("binary")]
    [DataRow("objects")]
    public async Task MainRecursiveTargetFormatsDirectoriesThatOnlyContainBuildOutputNames(string directoryName)
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile(Path.Combine(directoryName, "Unformatted.cs"), NeedsFormattingSource);

            // Act
            int exitCode;

            using (new ConsoleCapture())
            {
                exitCode = await Program.Main([tempDir.Path]);
            }

            // Assert
            var actualContent = await File.ReadAllTextAsync(filePath, TestContext.CancellationToken);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreNotEqual(NeedsFormattingSource, actualContent);
        }
    }

    /// <summary>
    /// Verifies that recursive discovery applies the platform's path casing rules to bin and obj segments
    /// </summary>
    /// <param name="directoryName">The uppercase build-output directory name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [DataRow("BIN")]
    [DataRow("OBJ")]
    public async Task MainRecursiveTargetUsesPlatformCasingForBuildOutputDirectories(string directoryName)
    {
        // Arrange
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile(Path.Combine(directoryName, "Unformatted.cs"), NeedsFormattingSource);

            // Act
            int exitCode;

            using (new ConsoleCapture())
            {
                exitCode = await Program.Main([tempDir.Path]);
            }

            // Assert
            var actualContent = await File.ReadAllTextAsync(filePath, TestContext.CancellationToken);

            Assert.AreEqual(ExitCodes.Success, exitCode);

            if (OperatingSystem.IsWindows())
            {
                Assert.AreEqual(NeedsFormattingSource, actualContent);
            }
            else
            {
                Assert.AreNotEqual(NeedsFormattingSource, actualContent);
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

    /// <summary>
    /// Creates an explicit target path using the requested spelling
    /// </summary>
    /// <param name="rootPath">The absolute root path</param>
    /// <param name="relativePath">The path relative to <paramref name="rootPath"/></param>
    /// <param name="targetSpelling">The requested path spelling</param>
    /// <returns>The target path to pass to the CLI</returns>
    private static string CreateTargetPath(string rootPath, string relativePath, string targetSpelling)
    {
        return targetSpelling switch
               {
                   "relative" => relativePath,
                   "dot-relative" => $".{Path.DirectorySeparatorChar}{relativePath}",
                   "absolute" => Path.Combine(rootPath, relativePath),
                   _ => throw new ArgumentOutOfRangeException(nameof(targetSpelling), targetSpelling, "Unsupported target spelling."),
               };
    }

    #endregion // Methods
}