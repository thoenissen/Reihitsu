using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Test.Helpers;

namespace Reihitsu.Cli.Test.EndToEnd;

/// <summary>
/// End-to-end tests for files whose syntax nests thousands of levels deep, run between two ordinary files: such a file either
/// formats or is reported as a failed file, and in both cases the other files are processed, the summary is written, and the exit
/// code follows (CLI-016, CLI-017). The CLI runs in a child process, because a stack overflow cannot be caught and would end the
/// test run instead of failing the test
/// </summary>
[TestClass]
[DoNotParallelize]
[TestCategory("E2E")]
public class CliDeepNestingEndToEndTests
{
    #region Constants

    /// <summary>
    /// A file with the unformatted field of the specification examples
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
    /// Verifies that a deeply nested file between two unformatted files, processed one after another, either formats or is
    /// reported as failed, and that the other two files, the summary, and the exit code follow
    /// </summary>
    /// <param name="shape">The shape that nests</param>
    /// <param name="depth">The nesting depth</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-016")]
    [DataRow("chain", 6400)]
    [DataRow("concat", 16000)]
    [DataRow("concat1line", 16000)]
    [DataRow("andchain", 16000)]
    [DataRow("orpattern", 16000)]
    [DataRow("elementaccess", 16000)]
    [DataRow("elseif", 6400)]
    [DataRow("memberaccess", 16000)]
    public async Task DeeplyNestedFileDoesNotStopTheRun(string shape, int depth)
    {
        await VerifyRunContinues(shape, depth, "\n", "1");
    }

    /// <summary>
    /// Verifies the fluent chain of the issue with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-016")]
    public async Task DeeplyNestedChainWithCrlfDoesNotStopTheRun()
    {
        await VerifyRunContinues("chain", 6400, "\r\n", "1");
    }

    /// <summary>
    /// Verifies the fluent chain of the issue with files processed at the same time: every file is still reported in selection
    /// order, followed by the summary and the exit code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-016")]
    [Requirement("CLI-022")]
    public async Task DeeplyNestedChainProcessedInParallelDoesNotStopTheRun()
    {
        await VerifyRunContinues("chain", 6400, "\n", "4");
    }

    /// <summary>
    /// Verifies that nesting which formats today keeps formatting without an error: the file is reported as needing formatting
    /// like the two files around it
    /// </summary>
    /// <param name="shape">The shape that nests</param>
    /// <param name="depth">The nesting depth</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [DataRow("chain", 2000)]
    [DataRow("elseif", 2000)]
    [DataRow("memberaccess", 2000)]
    [DataRow("concat", 2000)]
    public async Task ModeratelyNestedFileKeepsFormatting(string shape, int depth)
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var aPath = tempDir.CreateFile("A.cs", UnformattedSource);
            var deepPath = tempDir.CreateFile("Deep.cs", CreateSource(shape, depth, "\n"));
            var zPath = tempDir.CreateFile("Z.cs", UnformattedSource);

            var (exitCode, output, error) = await RunCliProcess(["--check", "--max-parallelism", "1", aPath, deepPath, zPath]);

            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.AreEqual(Join($"Not formatted: {aPath}",
                                 $"Not formatted: {deepPath}",
                                 $"Not formatted: {zPath}",
                                 string.Empty,
                                 "3 of 3 file(s) need formatting."),
                            output);
        }
    }

    /// <summary>
    /// Creates a file whose method nests the given shape to the given depth
    /// </summary>
    /// <param name="shape">The shape that nests</param>
    /// <param name="depth">The nesting depth</param>
    /// <param name="endOfLine">The line ending of the file</param>
    /// <returns>The source text</returns>
    private static string CreateSource(string shape, int depth, string endOfLine)
    {
        var body = shape switch
                   {
                       "chain" => $"    internal object M(System.Collections.Generic.List<int> items)\n    {{\n        return items\n{Repeat(0, depth, index => $"            .Where(x => x > {index})\n")}            .ToList();\n    }}\n",
                       "concat" => $"    internal string M(string s)\n    {{\n        return s{Repeat(0, depth, index => $"\n            + \"{index}\"")};\n    }}\n",
                       "concat1line" => CreateMethod("string", $"\"\"{Repeat(0, depth, index => $" + \"{index}\"")}"),
                       "andchain" => CreateMethod("bool", $"c{Repeat(0, depth, _ => "\n            && c")}"),
                       "orpattern" => CreateMethod("bool", $"a is 0{Repeat(1, depth, index => $"\n            or {index}")}"),
                       "elementaccess" => CreateMethod("int", $"arr{Repeat(0, depth, _ => "[0]")}"),
                       "elseif" => $"    internal int M(int x)\n    {{\n        if (x == 0)\n        {{\n            return 0;\n        }}\n{Repeat(1, depth, index => $"        else if (x == {index})\n        {{\n            return {index};\n        }}\n")}        return -1;\n    }}\n",
                       "memberaccess" => CreateMethod("object", $"a{Repeat(0, depth, _ => ".P")}"),
                       _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, null)
                   };

        var source = $"namespace N;\n\ninternal class C\n{{\n{body}}}\n";

        return source.Replace("\n", endOfLine);
    }

    /// <summary>
    /// Concatenates one part per index, from the first index up to but not including the end index
    /// </summary>
    /// <param name="firstIndex">The index of the first part</param>
    /// <param name="endIndex">The index behind the last part</param>
    /// <param name="createPart">Creates the part of an index</param>
    /// <returns>The concatenated parts; empty when the end index is not behind the first index</returns>
    private static string Repeat(int firstIndex, int endIndex, Func<int, string> createPart)
    {
        var parts = new StringBuilder();

        for (var index = firstIndex; index < endIndex; index++)
        {
            parts.Append(createPart(index));
        }

        return parts.ToString();
    }

    /// <summary>
    /// Creates a method that returns the given expression
    /// </summary>
    /// <param name="returnType">The return type</param>
    /// <param name="expression">The returned expression</param>
    /// <returns>The method declaration</returns>
    private static string CreateMethod(string returnType, string expression)
    {
        return $"    internal {returnType} M(object a, bool c, int[] arr, System.Func<int,int> f)\n    {{\n        return {expression};\n    }}\n";
    }

    /// <summary>
    /// Gets the path of the <c>dotnet</c> host that runs the CLI assembly
    /// </summary>
    /// <returns>The host path</returns>
    private static string GetDotnetHostPath()
    {
        var hostPath = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");

        if (string.IsNullOrEmpty(hostPath) == false && File.Exists(hostPath))
        {
            return hostPath;
        }

        var processPath = Environment.ProcessPath;

        if (processPath != null
            && string.Equals(Path.GetFileNameWithoutExtension(processPath), "dotnet", StringComparison.OrdinalIgnoreCase))
        {
            return processPath;
        }

        return "dotnet";
    }

    /// <summary>
    /// Gets the first lines of a long text, such as the stack trace the runtime writes on a stack overflow, so a failure message stays readable
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>At most the first 10 lines</returns>
    private static string Head(string text)
    {
        var lines = Lines(text);

        return string.Join(Environment.NewLine, lines.Take(10));
    }

    /// <summary>
    /// Splits console output into its lines, without the line break that ends the last one
    /// </summary>
    /// <param name="text">The console output</param>
    /// <returns>The lines</returns>
    private static List<string> Lines(string text)
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

    /// <summary>
    /// Runs the CLI with <c>--check</c> over <c>A.cs</c>, a deeply nested <c>Deep.cs</c>, and <c>Z.cs</c>, and verifies the whole
    /// output: when <c>Deep.cs</c> fails, its <c>Error processing</c> line on standard error, both other files, the summary with
    /// the error count, and exit code 2; when it formats, all three files, the summary, and exit code 1
    /// </summary>
    /// <param name="shape">The shape that nests</param>
    /// <param name="depth">The nesting depth</param>
    /// <param name="endOfLine">The line ending of the nested file</param>
    /// <param name="degree">The degree of parallelism</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous verification</returns>
    private async Task VerifyRunContinues(string shape, int depth, string endOfLine, string degree)
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var aPath = tempDir.CreateFile("A.cs", UnformattedSource);
            var deepPath = tempDir.CreateFile("Deep.cs", CreateSource(shape, depth, endOfLine));
            var zPath = tempDir.CreateFile("Z.cs", UnformattedSource);

            var (exitCode, output, error) = await RunCliProcess(["--check", "--max-parallelism", degree, aPath, deepPath, zPath]);

            Assert.IsTrue(exitCode is ExitCodes.FormattingNeeded or ExitCodes.Error,
                          $"The CLI ended with exit code {exitCode} instead of finishing the run.{Environment.NewLine}{Head(output)}{Environment.NewLine}{Head(error)}");

            var errorLines = Lines(error);

            if (errorLines.Count > 0)
            {
                Assert.HasCount(1, errorLines, error);
                Assert.StartsWith($"Error processing {deepPath}: ", errorLines[0]);
                Assert.AreEqual(ExitCodes.Error, exitCode);
                Assert.AreEqual(Join($"Not formatted: {aPath}",
                                     $"Not formatted: {zPath}",
                                     string.Empty,
                                     "2 of 3 file(s) need formatting.",
                                     "1 file(s) encountered errors."),
                                output);
            }
            else
            {
                var deepLine = $"Not formatted: {deepPath}";
                var deepNeedsFormatting = Lines(output).Contains(deepLine);
                var fileLines = deepNeedsFormatting
                                    ? new[] { $"Not formatted: {aPath}", deepLine, $"Not formatted: {zPath}" }
                                    : new[] { $"Not formatted: {aPath}", $"Not formatted: {zPath}" };

                Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
                Assert.AreEqual(Join([.. fileLines, string.Empty, $"{fileLines.Length} of 3 file(s) need formatting."]), output);
            }
        }
    }

    /// <summary>
    /// Runs the CLI in a child process and captures its exit code and console output
    /// </summary>
    /// <param name="arguments">The command-line arguments</param>
    /// <returns>The exit code, standard output, and standard error</returns>
    private async Task<(int ExitCode, string Output, string Error)> RunCliProcess(string[] arguments)
    {
        var startInfo = new ProcessStartInfo(GetDotnetHostPath())
                        {
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false
                        };

        startInfo.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "Reihitsu.Cli.dll"));

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using (var process = new Process
                             {
                                 StartInfo = startInfo
                             })
        {
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync(TestContext.CancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(TestContext.CancellationToken);

            try
            {
                await process.WaitForExitAsync(TestContext.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);

                throw;
            }

            return (process.ExitCode, await outputTask, await errorTask);
        }
    }

    #endregion // Methods
}