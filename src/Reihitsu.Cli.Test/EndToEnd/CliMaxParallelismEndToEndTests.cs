using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Test.Helpers;

namespace Reihitsu.Cli.Test.EndToEnd;

/// <summary>
/// End-to-end tests for <c>--max-parallelism</c> and parallel processing: the option's usage text and argument errors
/// (CLI-002, CLI-023), and real runs whose output, written files, summary, and exit code are the same for every degree of
/// parallelism (CLI-022)
/// </summary>
[TestClass]
[DoNotParallelize]
[TestCategory("E2E")]
public class CliMaxParallelismEndToEndTests
{
    #region Constants

    /// <summary>
    /// A file with the unformatted field of the specification examples
    /// </summary>
    private const string UnformattedSource = "class A\n{\n    int  x;\n}\n";

    /// <summary>
    /// The formatted form of <see cref="UnformattedSource"/>
    /// </summary>
    private const string FormattedSource = "class A\n{\n    int x;\n}";

    /// <summary>
    /// A file with a syntax error
    /// </summary>
    private const string SyntaxErrorSource = "class B\n{\n    int x\n}\n";

    /// <summary>
    /// The file of the CLI-019 example, which the formatter refuses as unsupported at line 5, column 13
    /// </summary>
    private const string UnsupportedSource = "class C\n{\n    void M()\n    {\n        int /* c */ a = 1;\n    }\n}\n";

    /// <summary>
    /// The first usage line of the option
    /// </summary>
    private const string UsageOptionLine = "  --max-parallelism <count>";

    /// <summary>
    /// The second usage line of the option
    /// </summary>
    private const string UsageDescriptionLine = "               Maximum number of files processed at the same time (default: the number of logical processors)";

    /// <summary>
    /// The first line of the usage text
    /// </summary>
    private const string UsageFirstLine = "reihitsu-format [options] [--] [<paths>...]";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the help lists the option with the two usage lines of the specification, between <c>--lang-version</c> and
    /// <c>--help</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-023")]
    public async Task HelpListsMaxParallelismWithItsDescription()
    {
        var (exitCode, output, _) = await Run(["--help"]);

        var lines = Lines(output);
        var optionIndex = lines.IndexOf(UsageOptionLine);

        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.IsGreaterThanOrEqualTo(0, optionIndex);
        Assert.AreEqual(UsageDescriptionLine, lines[optionIndex + 1]);
        Assert.IsLessThan(optionIndex, lines.FindIndex(line => line.StartsWith("  --lang-version", StringComparison.Ordinal)));
        Assert.IsGreaterThan(optionIndex, lines.FindIndex(line => line.StartsWith("  --help", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies the specification example: <c>--max-parallelism 0</c> writes the invalid-value error and the usage text to
    /// standard error, exits with code 2, and touches no file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-002")]
    [Requirement("CLI-023")]
    public async Task InvalidCountFailsBeforeAnyFileIsRead()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("A.cs", UnformattedSource);

            var (exitCode, output, error) = await Run(["--max-parallelism", "0", filePath]);

            var errorLines = Lines(error);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreEqual(string.Empty, output);
            Assert.AreEqual("Invalid value for --max-parallelism: '0'. Expected a whole number of 1 or more.", errorLines[0]);
            Assert.AreEqual(UsageFirstLine, errorLines[1]);
            Assert.AreEqual(UnformattedSource, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that <c>--max-parallelism</c> as the last argument writes the missing-value error and the usage text to standard
    /// error and exits with code 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-002")]
    public async Task MissingCountFailsWithMissingValueError()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("A.cs", UnformattedSource);

            var (exitCode, output, error) = await Run(["--check", filePath, "--max-parallelism"]);

            var errorLines = Lines(error);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreEqual(string.Empty, output);
            Assert.AreEqual("Missing value for --max-parallelism.", errorLines[0]);
            Assert.AreEqual(UsageFirstLine, errorLines[1]);
        }
    }

    /// <summary>
    /// Verifies that an invalid count takes precedence over <c>--help</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-002")]
    public async Task InvalidCountTakesPrecedenceOverHelp()
    {
        var (exitCode, output, error) = await Run(["--max-parallelism", "two", "--help"]);

        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.AreEqual(string.Empty, output);
        Assert.AreEqual("Invalid value for --max-parallelism: 'two'. Expected a whole number of 1 or more.", Lines(error)[0]);
    }

    /// <summary>
    /// Verifies the specification example: <c>--check --verbose --max-parallelism 4</c> over <c>A.cs</c>, <c>B.cs</c> with a
    /// syntax error, and <c>C.cs</c> writes the files in selection order and exits with code 1, as with
    /// <c>--max-parallelism 1</c>
    /// </summary>
    /// <param name="degree">The degree of parallelism</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    [DataRow("4")]
    [DataRow("1")]
    public async Task CheckSpecExampleWritesSelectionOrderForEveryDegree(string degree)
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var aPath = tempDir.CreateFile("A.cs", UnformattedSource);
            var bPath = tempDir.CreateFile("B.cs", SyntaxErrorSource);
            var cPath = tempDir.CreateFile("C.cs", UnformattedSource);

            var (exitCode, output, error) = await Run(["--check", "--verbose", "--max-parallelism", degree, aPath, bPath, cPath]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join($"Not formatted: {aPath}",
                                 $"Skipped (syntax errors): {bPath}",
                                 $"Not formatted: {cPath}",
                                 string.Empty,
                                 "2 of 3 file(s) need formatting.",
                                 "Skipped 1 file(s) with syntax errors."),
                            output);
        }
    }

    /// <summary>
    /// Verifies the specification example with the unsupported <c>u.cs</c> between two unformatted files: both
    /// <c>Not formatted:</c> lines and the summary on standard output, the <c>error:</c> line on standard error, exit code 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    public async Task CheckSpecExampleWithUnsupportedFileReportsEveryFile()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var aPath = tempDir.CreateFile("A.cs", UnformattedSource);
            var uPath = tempDir.CreateFile("u.cs", UnsupportedSource);
            var cPath = tempDir.CreateFile("C.cs", UnformattedSource);

            var (exitCode, output, error) = await Run(["--check", "--max-parallelism", "4", aPath, uPath, cPath]);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreEqual(Join($"Not formatted: {aPath}",
                                 $"Not formatted: {cPath}",
                                 string.Empty,
                                 "2 of 3 file(s) need formatting.",
                                 "Skipped 1 file(s) with unsupported syntax."),
                            output);
            Assert.AreEqual(Join($"error: {uPath}(5,13): unsupported syntax: a comment between a variable declaration's own tokens. The file was left unchanged."), error);
        }
    }

    /// <summary>
    /// Verifies the specification example: <c>--check --max-parallelism 2</c> over a directory writes what the run without the
    /// option writes, and what <c>--max-parallelism 1</c> writes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    [Requirement("CLI-023")]
    public async Task CheckOfDirectoryWritesTheSameOutputForEveryDegree()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            CreateMixedTree(tempDir);

            var (defaultExitCode, defaultOutput, defaultError) = await Run(["--check", "--verbose", tempDir.Path]);
            var (twoExitCode, twoOutput, twoError) = await Run(["--check", "--verbose", "--max-parallelism", "2", tempDir.Path]);
            var (oneExitCode, oneOutput, oneError) = await Run(["--check", "--verbose", "--max-parallelism", "1", tempDir.Path]);

            Assert.AreEqual(ExitCodes.FormattingNeeded, oneExitCode);
            Assert.AreEqual(oneExitCode, defaultExitCode);
            Assert.AreEqual(oneExitCode, twoExitCode);
            Assert.AreEqual(oneOutput, defaultOutput);
            Assert.AreEqual(oneOutput, twoOutput);
            Assert.AreEqual(oneError, defaultError);
            Assert.AreEqual(oneError, twoError);
        }
    }

    /// <summary>
    /// Verifies that write mode with a degree above one writes the same files with the same bytes, the same lines, and the same
    /// summary as with a degree of one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    public async Task WriteModeWritesTheSameFilesForEveryDegree()
    {
        using (var oneDir = new TemporaryDirectoryFixture())
            using (var fourDir = new TemporaryDirectoryFixture())
            {
                CreateMixedTree(oneDir);
                CreateMixedTree(fourDir);

                var (oneExitCode, oneOutput, oneError) = await Run(["--force", "--verbose", "--max-parallelism", "1", oneDir.Path]);
                var (fourExitCode, fourOutput, fourError) = await Run(["--force", "--verbose", "--max-parallelism", "4", fourDir.Path]);

                Assert.AreEqual(ExitCodes.Success, oneExitCode);
                Assert.AreEqual(oneExitCode, fourExitCode);
                Assert.AreEqual(oneOutput.Replace(oneDir.Path, "<root>", StringComparison.Ordinal), fourOutput.Replace(fourDir.Path, "<root>", StringComparison.Ordinal));
                Assert.AreEqual(oneError.Replace(oneDir.Path, "<root>", StringComparison.Ordinal), fourError.Replace(fourDir.Path, "<root>", StringComparison.Ordinal));

                var oneFiles = Directory.EnumerateFiles(oneDir.Path, "*", SearchOption.AllDirectories).Select(path => Path.GetRelativePath(oneDir.Path, path)).Order(StringComparer.Ordinal).ToArray();
                var fourFiles = Directory.EnumerateFiles(fourDir.Path, "*", SearchOption.AllDirectories).Select(path => Path.GetRelativePath(fourDir.Path, path)).Order(StringComparer.Ordinal).ToArray();

                Assert.AreSequenceEqual(oneFiles, fourFiles);

                foreach (var relativePath in oneFiles)
                {
                    var oneBytes = await File.ReadAllBytesAsync(Path.Combine(oneDir.Path, relativePath), TestContext.CancellationToken);
                    var fourBytes = await File.ReadAllBytesAsync(Path.Combine(fourDir.Path, relativePath), TestContext.CancellationToken);

                    Assert.AreSequenceEqual(oneBytes, fourBytes, $"Bytes of {relativePath} differ.");
                }
            }
    }

    /// <summary>
    /// Verifies the specification example in write mode for one file reached under two paths through a symbolic link: the file
    /// holds the formatted text, at least one of the paths is named <c>Formatted:</c>, and the summary counts one or two
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    public async Task WriteModeFormatsFileReachedThroughSymbolicLink()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var realFile = tempDir.CreateFile(Path.Combine("real", "A.cs"), UnformattedSource);
            var linkDirectory = Path.Combine(tempDir.Path, "link");
            var linkFile = Path.Combine(linkDirectory, "A.cs");

            Directory.CreateSymbolicLink(linkDirectory, Path.Combine(tempDir.Path, "real"));

            var (exitCode, output, error) = await Run(["--verbose", "--max-parallelism", "4", tempDir.Path]);

            var lines = Lines(output);
            var fileLines = lines.Take(2).Order(StringComparer.Ordinal).ToArray();

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(FormattedSource, await File.ReadAllTextAsync(realFile, TestContext.CancellationToken));
            Assert.HasCount(4, lines);
            Assert.IsTrue(fileLines.SequenceEqual([$"Formatted: {linkFile}", $"Formatted: {realFile}"])
                          || fileLines.SequenceEqual([$"Formatted: {linkFile}", $"Unchanged: {realFile}"])
                          || fileLines.SequenceEqual([$"Formatted: {realFile}", $"Unchanged: {linkFile}"]),
                          $"Unexpected file lines: {string.Join(" | ", fileLines)}");
            Assert.AreEqual(string.Empty, lines[2]);
            Assert.IsTrue(lines[3] == "Formatted 1 of 2 file(s)." || lines[3] == "Formatted 2 of 2 file(s).", $"Unexpected summary: {lines[3]}");
        }
    }

    /// <summary>
    /// Verifies the specification example with <c>--check</c> for one file reached under two paths through a symbolic link:
    /// both paths are selected and named <c>Not formatted:</c>, and the summary counts two, as one after another
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-006")]
    [Requirement("CLI-022")]
    public async Task CheckNamesBothPathsOfFileReachedThroughSymbolicLink()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var realFile = tempDir.CreateFile(Path.Combine("real", "A.cs"), UnformattedSource);
            var linkDirectory = Path.Combine(tempDir.Path, "link");
            var linkFile = Path.Combine(linkDirectory, "A.cs");

            Directory.CreateSymbolicLink(linkDirectory, Path.Combine(tempDir.Path, "real"));

            var (oneExitCode, oneOutput, _) = await Run(["--check", "--verbose", "--max-parallelism", "1", tempDir.Path]);
            var (exitCode, output, error) = await Run(["--check", "--verbose", "--max-parallelism", "4", tempDir.Path]);

            var lines = Lines(output);

            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.AreEqual(oneExitCode, exitCode);
            Assert.AreEqual(oneOutput, output);
            Assert.AreEqual(string.Empty, error);
            Assert.AreSequenceEqual(new[] { $"Not formatted: {linkFile}", $"Not formatted: {realFile}" }, lines.Take(2).Order(StringComparer.Ordinal).ToArray());
            Assert.AreEqual("2 of 2 file(s) need formatting.", lines[^1]);
            Assert.AreEqual(UnformattedSource, await File.ReadAllTextAsync(realFile, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Creates a tree of files with every per-file outcome that writes no error: unformatted files with and without a byte order
    /// mark, a formatted file, a file with a syntax error, a generated file by name, and a file generated by its header, in
    /// several directories
    /// </summary>
    /// <param name="tempDir">The temporary directory</param>
    private static void CreateMixedTree(TemporaryDirectoryFixture tempDir)
    {
        for (var index = 0; index < 8; index++)
        {
            tempDir.CreateFile(Path.Combine($"dir{index % 3}", $"Unformatted{index}.cs"), UnformattedSource.Replace("class A", $"class A{index}", StringComparison.Ordinal));
        }

        tempDir.CreateFile(Path.Combine("dir0", "WithBom.cs"), UnformattedSource, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        tempDir.CreateFile(Path.Combine("dir1", "Formatted.cs"), FormattedSource);
        tempDir.CreateFile(Path.Combine("dir1", "Broken.cs"), SyntaxErrorSource);
        tempDir.CreateFile(Path.Combine("dir2", "Generated.g.cs"), UnformattedSource);
        tempDir.CreateFile(Path.Combine("dir2", "Header.cs"), "// <auto-generated />\n" + UnformattedSource);
    }

    /// <summary>
    /// Runs the CLI and captures its exit code and console output
    /// </summary>
    /// <param name="arguments">The command-line arguments</param>
    /// <returns>The exit code, standard output, and standard error</returns>
    private static async Task<(int ExitCode, string Output, string Error)> Run(string[] arguments)
    {
        using (var capture = new ConsoleCapture())
        {
            var exitCode = await Program.Main(arguments);

            return (exitCode, capture.StandardOutput, capture.StandardError);
        }
    }

    /// <summary>
    /// Splits console output into its lines, without the line break that ends the last one
    /// </summary>
    /// <param name="text">The console output</param>
    /// <returns>The lines</returns>
    private static System.Collections.Generic.List<string> Lines(string text)
    {
        var lines = text.Split(Environment.NewLine).ToList();

        if (lines.Count > 0 && lines[^1].Length == 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return lines;
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