using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Test.Helpers;

namespace Reihitsu.Cli.Test.EndToEnd;

/// <summary>
/// End-to-end tests for files the formatter refuses as unsupported: the CLI leaves them unchanged in every mode, reports them on
/// standard error, counts them in the summary, and decides the exit code by <c>--suppress-unsupported</c>
/// </summary>
[TestClass]
[DoNotParallelize]
[TestCategory("E2E")]
public class CliUnsupportedSyntaxEndToEndTests
{
    #region Constants

    /// <summary>
    /// The file of the CLI-019 example, whose comment between the declaration's own tokens starts in line 5, column 13
    /// </summary>
    private const string UnsupportedSource = "class C\n{\n    void M()\n    {\n        int /* c */ a = 1;\n    }\n}\n";

    /// <summary>
    /// A file that holds two unsupported comments, the first in line 4, column 17, and that the formatter would otherwise change
    /// </summary>
    private const string TwoShapesSource = "class C{\n    void M()\n    {\n        int a = /* first */ 1;\n        int /* second */ b=2;\n    }\n}\n";

    /// <summary>
    /// A formatted file
    /// </summary>
    private const string FormattedSource = "class B\n{\n    void M()\n    {\n    }\n}\n";

    /// <summary>
    /// A file the formatter changes
    /// </summary>
    private const string UnformattedSource = "class A{ void M(){ } }\n";

    /// <summary>
    /// The reason the formatter gives for the shape of <see cref="UnsupportedSource"/>
    /// </summary>
    private const string Reason = "a comment between a variable declaration's own tokens";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies the specification example: write mode leaves the file unchanged, writes one error line with the path, the line,
    /// the column, and the reason to standard error, prints the summary with the skipped group, and exits with code 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-019")]
    [Requirement("CLI-014")]
    [Requirement("CLI-017")]
    public async Task SpecExampleWriteModeLeavesTheFileUnchangedAndReportsAnError()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("u.cs", UnsupportedSource);
            var originalBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            var (exitCode, output, error) = await Run([filePath]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreSequenceEqual(new[] { ErrorLine("error", filePath, 5, 13) }, Lines(error));
            Assert.AreEqual(Join(string.Empty, "Formatted 0 of 1 file(s).", "Skipped 1 file(s) with unsupported syntax."), output);
            Assert.AreSequenceEqual(originalBytes, await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that write mode formats the other file of the run, counts the refused file in the total but not as changed,
    /// and still exits with code 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-019")]
    [Requirement("CLI-017")]
    public async Task WriteModeFormatsTheOtherFilesAndExitsWithError()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var unsupportedPath = tempDir.CreateFile("u.cs", UnsupportedSource);
            var unformattedPath = tempDir.CreateFile("A.cs", UnformattedSource);

            var (exitCode, output, error) = await Run([tempDir.Path]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreSequenceEqual(new[] { ErrorLine("error", unsupportedPath, 5, 13) }, Lines(error));
            Assert.Contains($"{Environment.NewLine}Formatted 1 of 2 file(s).{Environment.NewLine}Skipped 1 file(s) with unsupported syntax.{Environment.NewLine}", output);
            Assert.AreEqual(UnsupportedSource, await File.ReadAllTextAsync(unsupportedPath, TestContext.CancellationToken));
            Assert.AreNotEqual(UnformattedSource, await File.ReadAllTextAsync(unformattedPath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that a refused file is not written with a byte order mark when <c>--utf8-bom</c> would add one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-019")]
    public async Task Utf8BomDoesNotChangeTheEncodingOfARefusedFile()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("u.cs", UnsupportedSource, new UTF8Encoding(false));
            var originalBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            var (exitCode, _, error) = await Run(["--utf8-bom", filePath]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreSequenceEqual(new[] { ErrorLine("error", filePath, 5, 13) }, Lines(error));
            Assert.AreSequenceEqual(originalBytes, await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies the specification example: <c>--check</c> with the refused file and a formatted file writes the error line, no
    /// <c>Not formatted:</c> line, counts no file as needing formatting, and exits with code 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-019")]
    [Requirement("CLI-014")]
    [Requirement("CLI-017")]
    public async Task SpecExampleCheckReportsTheRefusedFileOnlyOnStandardError()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var unsupportedPath = tempDir.CreateFile("u.cs", UnsupportedSource);
            var formattedPath = tempDir.CreateFile("B.cs", FormattedSource);

            var (exitCode, output, error) = await Run(["--check", unsupportedPath, formattedPath]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreSequenceEqual(new[] { ErrorLine("error", unsupportedPath, 5, 13) }, Lines(error));
            Assert.AreEqual(Join(string.Empty, "0 of 2 file(s) need formatting.", "Skipped 1 file(s) with unsupported syntax."), output);
        }
    }

    /// <summary>
    /// Verifies the specification example: <c>--check</c> with the refused file and an unformatted file reports the unformatted
    /// file, counts only it as needing formatting, and still exits with code 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-019")]
    [Requirement("CLI-017")]
    public async Task SpecExampleCheckExitsWithErrorAlsoWhenAnotherFileNeedsFormatting()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var unsupportedPath = tempDir.CreateFile("u.cs", UnsupportedSource);
            var unformattedPath = tempDir.CreateFile("A.cs", UnformattedSource);

            var (exitCode, output, error) = await Run(["--check", unsupportedPath, unformattedPath]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreSequenceEqual(new[] { ErrorLine("error", unsupportedPath, 5, 13) }, Lines(error));
            Assert.AreEqual(Join($"Not formatted: {Path.GetFullPath(unformattedPath)}", string.Empty, "1 of 2 file(s) need formatting.", "Skipped 1 file(s) with unsupported syntax."), output);
        }
    }

    /// <summary>
    /// Verifies the specification example: <c>--dry-run</c> with the refused file and a formatted file writes the error line, no
    /// <c>Would format:</c> line and no diff, counts no file as would be formatted, and exits with code 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-019")]
    [Requirement("CLI-017")]
    public async Task SpecExampleDryRunReportsTheRefusedFileOnlyOnStandardError()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var unsupportedPath = tempDir.CreateFile("u.cs", UnsupportedSource);
            var formattedPath = tempDir.CreateFile("B.cs", FormattedSource);

            var (exitCode, output, error) = await Run(["--dry-run", unsupportedPath, formattedPath]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreSequenceEqual(new[] { ErrorLine("error", unsupportedPath, 5, 13) }, Lines(error));
            Assert.AreEqual(Join(string.Empty, "0 of 2 file(s) would be formatted.", "Skipped 1 file(s) with unsupported syntax."), output);
            Assert.AreEqual(UnsupportedSource, await File.ReadAllTextAsync(unsupportedPath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that <c>--verbose</c> prints no further line for a refused file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-019")]
    public async Task VerbosePrintsNoFurtherLineForARefusedFile()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("u.cs", UnsupportedSource);

            var (exitCode, output, error) = await Run(["--verbose", filePath]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreSequenceEqual(new[] { ErrorLine("error", filePath, 5, 13) }, Lines(error));
            Assert.DoesNotContain("u.cs", output, "The refused file must be reported on standard error only.");
        }
    }

    /// <summary>
    /// Verifies that a file with several unsupported shapes gets one line, which names the shape that starts first in the text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-019")]
    public async Task FileWithSeveralShapesGetsOneLineForTheFirstShape()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("u.cs", TwoShapesSource);

            var (exitCode, _, error) = await Run([filePath]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreSequenceEqual(new[] { ErrorLine("error", filePath, 4, 17) }, Lines(error));
            Assert.AreEqual(TwoShapesSource, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies the line and column of a refused file with CRLF line endings, which count lines as LF does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-019")]
    public async Task LocationOfAShapeInACrlfFileIsReportedAsInAnLfFile()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var source = UnsupportedSource.Replace("\n", "\r\n");
            var filePath = tempDir.CreateFile("u.cs", source);

            var (exitCode, _, error) = await Run([filePath]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreSequenceEqual(new[] { ErrorLine("error", filePath, 5, 13) }, Lines(error));
            Assert.AreEqual(source, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies the specification example: with <c>--suppress-unsupported</c>, <c>--check</c> reports the refused file with a
    /// warning line, still counts it, and exits with the code the run has for the unformatted file alone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-020")]
    [Requirement("CLI-017")]
    public async Task SpecExampleSuppressUnsupportedReportsAWarningAndKeepsTheExitCodeOfTheOtherFiles()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var unsupportedPath = tempDir.CreateFile("u.cs", UnsupportedSource);
            var unformattedPath = tempDir.CreateFile("A.cs", UnformattedSource);

            var (exitCode, output, error) = await Run(["--check", "--suppress-unsupported", unsupportedPath, unformattedPath]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.AreSequenceEqual(new[] { ErrorLine("warning", unsupportedPath, 5, 13) }, Lines(error));
            Assert.AreEqual(Join($"Not formatted: {Path.GetFullPath(unformattedPath)}", string.Empty, "1 of 2 file(s) need formatting.", "Skipped 1 file(s) with unsupported syntax."), output);
        }
    }

    /// <summary>
    /// Verifies that with <c>--suppress-unsupported</c> a write run whose only file is refused leaves it unchanged, reports a
    /// warning, and exits with code 0
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-020")]
    public async Task SuppressUnsupportedWriteRunLeavesTheFileUnchangedAndSucceeds()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("u.cs", UnsupportedSource);

            var (exitCode, output, error) = await Run(["--suppress-unsupported", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreSequenceEqual(new[] { ErrorLine("warning", filePath, 5, 13) }, Lines(error));
            Assert.AreEqual(Join(string.Empty, "Formatted 0 of 1 file(s).", "Skipped 1 file(s) with unsupported syntax."), output);
            Assert.AreEqual(UnsupportedSource, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that with <c>--suppress-unsupported</c> a <c>--dry-run</c> whose other file is formatted exits with code 0
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-020")]
    public async Task SuppressUnsupportedDryRunWithOtherwiseFormattedFilesSucceeds()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var unsupportedPath = tempDir.CreateFile("u.cs", UnsupportedSource);
            var formattedPath = tempDir.CreateFile("B.cs", FormattedSource);

            var (exitCode, output, error) = await Run(["--dry-run", "--suppress-unsupported", unsupportedPath, formattedPath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreSequenceEqual(new[] { ErrorLine("warning", unsupportedPath, 5, 13) }, Lines(error));
            Assert.AreEqual(Join(string.Empty, "0 of 2 file(s) would be formatted.", "Skipped 1 file(s) with unsupported syntax."), output);
        }
    }

    /// <summary>
    /// Verifies that a file the formatter formats produces no error line and no skipped group, so the refusal is decided by the
    /// formatter and not by every comment next to a declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-019")]
    public async Task FileWithCommentsOutsideTheDeclarationsOwnTokensIsFormatted()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("s.cs", "class C{\n    void M()\n    {\n        // above\n        int a = 1; // behind\n    }\n}\n");

            var (exitCode, output, error) = await Run([filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.DoesNotContain("unsupported syntax", output);
            Assert.Contains("Formatted 1 of 1 file(s).", output);
        }
    }

    /// <summary>
    /// Runs the CLI with the given arguments and captures its exit code and both output streams
    /// </summary>
    /// <param name="arguments">The command-line arguments</param>
    /// <returns>The exit code, the standard output, and the standard error</returns>
    private static async Task<(int ExitCode, string Output, string Error)> Run(string[] arguments)
    {
        using (var capture = new ConsoleCapture())
        {
            var exitCode = await Program.Main(arguments);

            return (exitCode, capture.StandardOutput, capture.StandardError);
        }
    }

    /// <summary>
    /// Builds the report line of CLI-019 for the given file and location
    /// </summary>
    /// <param name="severity"><c>error</c> or <c>warning</c></param>
    /// <param name="filePath">The path of the file</param>
    /// <param name="line">The one-based line</param>
    /// <param name="column">The one-based column</param>
    /// <returns>The report line</returns>
    private static string ErrorLine(string severity, string filePath, int line, int column)
    {
        return $"{severity}: {Path.GetFullPath(filePath)}({line},{column}): unsupported syntax: {Reason}. The file was left unchanged.";
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

    /// <summary>
    /// Joins lines with the console's line break and ends the last one with it
    /// </summary>
    /// <param name="lines">The lines</param>
    /// <returns>The text the console writes for the lines</returns>
    private static string Join(params string[] lines)
    {
        return string.Concat(lines.Select(line => line + Environment.NewLine));
    }

    #endregion // Methods
}