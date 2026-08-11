using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Tooling.Test.Helpers;

namespace Reihitsu.Tooling.Test.Unit;

/// <summary>
/// Tests for <see cref="CodeFixRunCommand"/>
/// </summary>
[TestClass]
public sealed class CodeFixRunCommandTests
{
    #region Constants

    /// <summary>
    /// Diagnostic ID used by the fixtures. RH7101 is the rule whose provider both offers a fix and deliberately
    /// withholds one for directive-bearing fields, so a single rule reaches every outcome the runner classifies
    /// </summary>
    private const string CombinedFieldsDiagnosticId = "RH7101";

    /// <summary>
    /// Diagnostic ID of a rule that reports but ships no code fix
    /// </summary>
    private const string DiagnosticIdWithoutCodeFix = "RH8011";

    /// <summary>
    /// A type carrying one combined field declaration, which RH7101 reports and its code fix splits
    /// </summary>
    private const string CombinedFieldSource = "internal class Sample\n{\n    private int _first, _second;\n}\n";

    /// <summary>
    /// A type carrying a single declarator, which RH7101 never reports
    /// </summary>
    private const string SingleDeclaratorSource = "internal class Sample\n{\n    private int _first;\n}\n";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that every C# file below the fixture directory is exercised under both line endings in one run,
    /// that nested directories are included, and that non-C# siblings are ignored
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncProcessesEveryFixtureUnderBothLineEndingsInOneRun()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("first.cs", CombinedFieldSource);
            fixture.CreateFile("nested/second.cs", SingleDeclaratorSource);
            fixture.CreateFile("notes.txt", "not a fixture");

            // Act
            var (exitCode, output, _) = await RunAsync(CombinedFieldsDiagnosticId, fixture.Path);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains("== first.cs [LF] ==", output);
            Assert.Contains("== first.cs [CRLF] ==", output);
            Assert.Contains("== nested/second.cs [LF] ==", output);
            Assert.Contains("== nested/second.cs [CRLF] ==", output);
            Assert.DoesNotContain("notes.txt", output);
            Assert.Contains("CODE-FIX RUN: 2 fixture(s) x 2 endings", output);
        }
    }

    /// <summary>
    /// Verifies that a fixture named like a generated file is still exercised. The runner deliberately does not
    /// inherit the CLI's generated-file skip, because a silently skipped fixture is indistinguishable from a
    /// fixture that reports nothing
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncProcessesGeneratedLookingFixture()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("Sample.g.cs", CombinedFieldSource);

            // Act
            var (exitCode, output, _) = await RunAsync(CombinedFieldsDiagnosticId, fixture.Path);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains("== Sample.g.cs [LF] == fixed", output);
        }
    }

    /// <summary>
    /// Verifies that a fixable fixture reports the fixed status with its action and iteration counts, and that the
    /// change is rendered as a unified diff
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncReportsFixedWithUnifiedDiffAndActionCount()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("combined.cs", CombinedFieldSource);

            // Act
            var (exitCode, output, _) = await RunAsync(CombinedFieldsDiagnosticId, fixture.Path);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains("== combined.cs [LF] == fixed (1 action(s), 1 iteration(s))", output);
            Assert.Contains("--- a/combined.cs", output);
            Assert.Contains("+++ b/combined.cs", output);
            Assert.Contains("-    private int _first, _second;", output);
            Assert.Contains("+    private int _first;", output);
            Assert.Contains("+    private int _second;", output);
        }
    }

    /// <summary>
    /// Verifies that each arm's output uses the requested line ending exclusively, so a fix that inserted the
    /// host's newline instead of the document's own separator would be visible rather than silently normalized
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncReportsNoLineEndingDriftWhenTheFixPreservesTheSeparator()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("combined.cs", CombinedFieldSource);

            // Act
            var (exitCode, output, _) = await RunAsync(CombinedFieldsDiagnosticId, fixture.Path);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.DoesNotContain("line-ending-drift]", output);
            Assert.Contains("0 line-ending-drift", output);
        }
    }

    /// <summary>
    /// Verifies that a fixture the analyzer never reports is a successful run rather than a tool failure. Every
    /// sweep row that records "does not reproduce" depends on this staying at exit code 0
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncReportsNoDiagnosticAtSuccessExitCode()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("single.cs", SingleDeclaratorSource);

            // Act
            var (exitCode, output, _) = await RunAsync(CombinedFieldsDiagnosticId, fixture.Path);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains("== single.cs [LF] == no-diagnostic", output);
            Assert.Contains("== single.cs [CRLF] == no-diagnostic", output);
            Assert.DoesNotContain("--- a/single.cs", output);
        }
    }

    /// <summary>
    /// Verifies that a reported diagnostic for which the provider registers no action is a supported end state at
    /// exit code 0, matching the repository's code-fix coverage policy
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncReportsNoFixOfferedForDirectiveBearingFieldAtSuccessExitCode()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            // The condition is deliberately true, so the field stays active code the analyzer reports on. A
            // disabled branch would be reported as no-diagnostic and would prove nothing about fix availability.
            fixture.CreateFile("directive.cs",
                               "internal class Sample\n{\n#if !NEVER\n    private int _first, _second;\n#endif\n}\n");

            // Act
            var (exitCode, output, _) = await RunAsync(CombinedFieldsDiagnosticId, fixture.Path);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains("== directive.cs [LF] == no-fix-offered", output);
            Assert.Contains("== directive.cs [CRLF] == no-fix-offered", output);
        }
    }

    /// <summary>
    /// Verifies that a fixture reporting the diagnostic in two independent types converges by applying the fix
    /// once per occurrence. Convergence is decided by the diagnostic disappearing, so a fixture whose text keeps
    /// changing on every iteration still converges
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncConvergesOverSeveralIterations()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("two-types.cs", TwoCombinedFieldTypesSource());

            // Act
            var (exitCode, output, _) = await RunAsync(CombinedFieldsDiagnosticId, fixture.Path);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains("== two-types.cs [LF] == fixed (1 action(s), 2 iteration(s))", output);
        }
    }

    /// <summary>
    /// Verifies that a fixture still reporting the diagnostic at the iteration cap is reported as not converged
    /// and moves the exit code to 1, which is distinct from the tool being unable to run
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncReportsNotConvergedWhenIterationCapIsReached()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("two-types.cs", TwoCombinedFieldTypesSource());

            // Act
            var (exitCode, output, _) = await RunAsync(CombinedFieldsDiagnosticId, fixture.Path, "--max-iterations", "1");

            // Assert
            Assert.AreEqual(ExitCodes.NotConverged, exitCode);
            Assert.Contains("== two-types.cs [LF] == not-converged (1 iteration(s))", output);
            Assert.Contains("2 not-converged", output);
        }
    }

    /// <summary>
    /// Verifies that a fixture which does not parse is reported as a caller error rather than skipped, because a
    /// fixture directory has no legitimately unparseable member
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncReportsParseErrorAtErrorExitCode()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("broken.cs", "internal class Sample\n{\n    private int _first, _second;\n");

            // Act
            var (exitCode, output, _) = await RunAsync(CombinedFieldsDiagnosticId, fixture.Path);

            // Assert
            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.Contains("== broken.cs [LF] == parse-error", output);
            Assert.Contains("2 parse-error", output);
        }
    }

    /// <summary>
    /// Verifies that the runner never writes to the fixtures it reads
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncLeavesFixtureFilesUntouched()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var path = fixture.CreateFile("combined.cs", CombinedFieldSource);
            var originalBytes = await File.ReadAllBytesAsync(path, TestContext.CancellationToken);

            // Act
            var (exitCode, _, _) = await RunAsync(CombinedFieldsDiagnosticId, fixture.Path);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreSequenceEqual(originalBytes, await File.ReadAllBytesAsync(path, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that a diagnostic ID no analyzer reports fails before any fixture runs, naming the ID
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncUnknownDiagnosticIdReturnsError()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("combined.cs", CombinedFieldSource);

            // Act
            var (exitCode, output, error) = await RunAsync("RH9999", fixture.Path);

            // Assert
            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.Contains("RH9999", error);
            Assert.AreEqual(string.Empty, output);
        }
    }

    /// <summary>
    /// Verifies that a rule which reports but ships no code fix fails resolution instead of reporting every
    /// fixture as unfixable, so a missing provider cannot be mistaken for a withheld fix
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncDiagnosticWithoutCodeFixProviderReturnsError()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("combined.cs", CombinedFieldSource);

            // Act
            var (exitCode, _, error) = await RunAsync(DiagnosticIdWithoutCodeFix, fixture.Path);

            // Assert
            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.Contains("no code fix provider", error);
            Assert.Contains(DiagnosticIdWithoutCodeFix, error);
        }
    }

    /// <summary>
    /// Verifies that a missing fixture directory is reported as a tool failure
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncMissingDirectoryReturnsError()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var missing = Path.Combine(fixture.Path, "does-not-exist");

            // Act
            var (exitCode, _, error) = await RunAsync(CombinedFieldsDiagnosticId, missing);

            // Assert
            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.Contains("fixture directory not found", error);
        }
    }

    /// <summary>
    /// Verifies that a directory holding no C# file is reported as a tool failure rather than as an empty sweep
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncDirectoryWithoutFixturesReturnsError()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("notes.txt", "not a fixture");

            // Act
            var (exitCode, _, error) = await RunAsync(CombinedFieldsDiagnosticId, fixture.Path);

            // Assert
            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.Contains("no C# fixture found", error);
        }
    }

    /// <summary>
    /// Verifies that usage is printed on request without running anything
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncHelpPrintsUsageAtSuccessExitCode()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();

        // Act
        var exitCode = await CodeFixRunCommand.ExecuteAsync(["--help"], output, error, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("usage: apply-fix", output.ToString());
        Assert.AreEqual(string.Empty, error.ToString());
    }

    /// <summary>
    /// Verifies that a missing fixture directory argument is rejected before anything runs
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncWithoutFixtureDirectoryReturnsError()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();

        // Act
        var exitCode = await CodeFixRunCommand.ExecuteAsync([CombinedFieldsDiagnosticId], output, error, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.Contains("expected one diagnostic ID and one fixture directory", error.ToString());
    }

    /// <summary>
    /// Verifies that a non-positive iteration cap is rejected
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncRejectsNonPositiveIterationCap()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();

        // Act
        var exitCode = await CodeFixRunCommand.ExecuteAsync([CombinedFieldsDiagnosticId, ".", "--max-iterations", "0"],
                                                            output,
                                                            error,
                                                            TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.Contains("--max-iterations requires a positive integer", error.ToString());
    }

    /// <summary>
    /// Verifies that an unknown option is rejected instead of being treated as a positional argument
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncRejectsUnknownOption()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();

        // Act
        var exitCode = await CodeFixRunCommand.ExecuteAsync([CombinedFieldsDiagnosticId, ".", "--verbose"],
                                                            output,
                                                            error,
                                                            TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.Contains("unknown argument '--verbose'", error.ToString());
    }

    /// <summary>
    /// Builds a fixture whose diagnostic occurs in two independent types, so the fix has to be applied twice
    /// </summary>
    /// <returns>The fixture source</returns>
    private static string TwoCombinedFieldTypesSource()
    {
        return "internal class First\n{\n    private int _a, _b;\n}\n\ninternal class Second\n{\n    private int _c, _d;\n}\n";
    }

    /// <summary>
    /// Runs the command over a fixture directory and captures its streams
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID to exercise</param>
    /// <param name="directory">Fixture directory</param>
    /// <param name="additionalArguments">Additional command arguments</param>
    /// <returns>The exit code and the captured output and error text</returns>
    private async Task<(int ExitCode, string Output, string Error)> RunAsync(string diagnosticId, string directory, params string[] additionalArguments)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var arguments = new List<string>
                        {
                            diagnosticId,
                            directory
                        };

        arguments.AddRange(additionalArguments);

        var exitCode = await CodeFixRunCommand.ExecuteAsync([.. arguments], output, error, TestContext.CancellationToken);

        return (exitCode, output.ToString(), error.ToString());
    }

    #endregion // Methods
}