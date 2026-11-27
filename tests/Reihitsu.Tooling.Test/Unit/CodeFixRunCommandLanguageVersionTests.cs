using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Tooling.Test.Helpers;

namespace Reihitsu.Tooling.Test.Unit;

/// <summary>
/// Tests for the <c>--lang-version</c> option of <see cref="CodeFixRunCommand"/>: it takes the values <c>reihitsu-format</c>
/// accepts, resolves them as <c>reihitsu-format</c> does, and every parse the runner makes — the fixture's parse check, the
/// project in which the analyzer reports and the code fix runs, and the check of each fix result — uses the resolved version
/// </summary>
[TestClass]
public sealed class CodeFixRunCommandLanguageVersionTests
{
    #region Constants

    /// <summary>
    /// Diagnostic ID of the rule for expression-bodied methods, whose code fix formats the new block body
    /// </summary>
    private const string ExpressionBodiedMethodDiagnosticId = "RH3202";

    /// <summary>
    /// Diagnostic ID of the rule for a function body whose braces share a line, which reports a lambda inside a non-verbatim
    /// interpolation only from C# 11 on
    /// </summary>
    private const string SharedLineBracesDiagnosticId = "RH5417";

    /// <summary>
    /// An expression-bodied method whose non-verbatim interpolation holds an anonymous method. From C# 11 on, the formatter
    /// inside the fix breaks the anonymous method's braces onto their own lines; before C# 11 it writes no line break there
    /// </summary>
    private const string AnonymousMethodInInterpolationSource = "using System;\n\nclass C\n{\n    int F(Func<int> f)\n    {\n        return f();\n    }\n\n    string M() => $\"a {F(delegate { return 1; })} b\";\n}\n";

    /// <summary>
    /// A lambda with a one-line block body inside a non-verbatim interpolation
    /// </summary>
    private const string LambdaInInterpolationSource = "using System;\n\nclass C\n{\n    string M() => $\"{((Func<int>)(() => { return 1; }))()}\";\n}\n";

    /// <summary>
    /// A file declaring a union, which parses only from C# 15 on
    /// </summary>
    private const string UnionSource = "public union Pet(Cat, Dog)\n{\n    public int Legs() => 4;\n}\n\npublic class Cat { }\n\npublic class Dog { }\n";

    /// <summary>
    /// A type named <c>union</c> used as the return type of an expression-bodied method, which C# 15 reads as a union declaration
    /// and C# 14 parses
    /// </summary>
    private const string TypeNamedUnionSource = "class union { }\n\nclass C\n{\n    union M(int a) => null;\n}\n";

    /// <summary>
    /// The warning for a value newer than the newest supported version
    /// </summary>
    private const string PreviewWarning = "apply-fix: Warning: C# language version 'preview' is newer than the newest supported version 15.0; formatting for 15.0.";

    /// <summary>
    /// The first line of the usage text
    /// </summary>
    private const string UsageLine = "usage: apply-fix <diagnostic-id> <fixture-directory> [--max-iterations <positive-integer>] [--lang-version <version>]";

    /// <summary>
    /// The second line of the usage text
    /// </summary>
    private const string HelpLine = "       apply-fix --help";

    /// <summary>
    /// The example message of an invalid value
    /// </summary>
    private const string ExpectedVersionHint = "Expected a C# language version such as 12, 12.0, latest, or preview.";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that under C# 10 the formatter inside the fix writes no line break into the interpolation, wherever the option
    /// stands among the arguments
    /// </summary>
    /// <param name="value">The language version value</param>
    /// <param name="optionFirst">Whether the option stands in front of the diagnostic ID</param>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [DataRow("10", false)]
    [DataRow("10", true)]
    [DataRow("10.0", false)]
    public async Task ExecuteAsyncBeforeCSharp11FormatsTheFixResultForThatVersion(string value, bool optionFirst)
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("D.cs", AnonymousMethodInInterpolationSource);

            string[] arguments = optionFirst
                                     ? ["--lang-version", value, ExpressionBodiedMethodDiagnosticId, fixture.Path]
                                     : [ExpressionBodiedMethodDiagnosticId, fixture.Path, "--lang-version", value];

            // Act
            var (exitCode, output, error) = await RunAsync(arguments);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains("== D.cs [LF] == fixed (1 action(s), 1 iteration(s))\n", output.Replace("\r\n", "\n", StringComparison.Ordinal));
            Assert.Contains("== D.cs [CRLF] == fixed (1 action(s), 1 iteration(s))\n", output.Replace("\r\n", "\n", StringComparison.Ordinal));
            Assert.Contains("-    string M() => $\"a {F(delegate { return 1; })} b\";\n"
                            + "+    string M()\n"
                            + "+    {\n"
                            + "+        return $\"a {F(delegate { return 1; })} b\";\n"
                            + "+    }\n"
                            + " }\n",
                            output);
            Assert.AreEqual(string.Empty, error);
        }
    }

    /// <summary>
    /// Verifies that without the option the formatter inside the fix lays the anonymous method out for the newest supported
    /// version, which breaks its braces onto their own lines
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncWithoutLangVersionFormatsTheFixResultForTheNewestVersion()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("D.cs", AnonymousMethodInInterpolationSource);

            // Act
            var (exitCode, output, error) = await RunAsync([ExpressionBodiedMethodDiagnosticId, fixture.Path]);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains("+        return $\"a {F(delegate\n"
                            + "+        {\n"
                            + "+            return 1;\n"
                            + "+        })} b\";\n",
                            output);
            Assert.AreEqual(string.Empty, error);
        }
    }

    /// <summary>
    /// Verifies that every value that resolves to C# 11 or later, up to the newest supported version, runs the fixture exactly
    /// as a run without the option does, without a warning
    /// </summary>
    /// <param name="value">The language version value</param>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [DataRow("11")]
    [DataRow("15")]
    [DataRow("15.0")]
    [DataRow("latest")]
    [DataRow("default")]
    [DataRow("latestmajor")]
    public async Task ExecuteAsyncFromCSharp11RunsAsWithoutTheOption(string value)
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("D.cs", AnonymousMethodInInterpolationSource);

            var (defaultExitCode, defaultOutput, _) = await RunAsync([ExpressionBodiedMethodDiagnosticId, fixture.Path]);

            // Act
            var (exitCode, output, error) = await RunAsync([ExpressionBodiedMethodDiagnosticId, fixture.Path, "--lang-version", value]);

            // Assert
            Assert.AreEqual(defaultExitCode, exitCode);
            Assert.AreEqual(defaultOutput, output);
            Assert.AreEqual(string.Empty, error);
        }
    }

    /// <summary>
    /// Verifies that the analyzer runs in a project parsed with the version: a lambda inside a non-verbatim interpolation is not
    /// reported under C# 10, while from C# 11 on it is reported and the fix is withheld
    /// </summary>
    /// <param name="value">The language version value, or <see langword="null"/> for none</param>
    /// <param name="expectedOutcome">The expected outcome of both line-ending arms</param>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [DataRow("10", "no-diagnostic")]
    [DataRow("11", "no-fix-offered")]
    [DataRow(null, "no-fix-offered")]
    public async Task ExecuteAsyncAnalyzerReportsForTheProjectVersion(string value, string expectedOutcome)
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("L.cs", LambdaInInterpolationSource);

            string[] arguments = value == null
                                     ? [SharedLineBracesDiagnosticId, fixture.Path]
                                     : [SharedLineBracesDiagnosticId, fixture.Path, "--lang-version", value];

            // Act
            var (exitCode, output, error) = await RunAsync(arguments);

            // Assert
            var lines = SplitLines(output);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains($"== L.cs [LF] == {expectedOutcome}", lines);
            Assert.Contains($"== L.cs [CRLF] == {expectedOutcome}", lines);
            Assert.AreEqual(string.Empty, error);
        }
    }

    /// <summary>
    /// Verifies that the fixture's parse check uses the version: a union declaration is a parse error under C# 14, while without
    /// the option the fixture is fixed
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncUnionUnderCSharp14IsAParseError()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("Pet.cs", UnionSource);

            // Act
            var (exitCode, output, error) = await RunAsync([ExpressionBodiedMethodDiagnosticId, fixture.Path, "--lang-version", "14"]);
            var (defaultExitCode, defaultOutput, defaultError) = await RunAsync([ExpressionBodiedMethodDiagnosticId, fixture.Path]);

            // Assert
            var lines = SplitLines(output);
            var defaultLines = SplitLines(defaultOutput);

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.Contains("== Pet.cs [LF] == parse-error", lines);
            Assert.Contains("== Pet.cs [CRLF] == parse-error", lines);
            Assert.AreEqual(string.Empty, error);

            Assert.AreEqual(ExitCodes.Success, defaultExitCode);
            Assert.Contains("== Pet.cs [LF] == fixed (1 action(s), 1 iteration(s))", defaultLines);
            Assert.Contains("== Pet.cs [CRLF] == fixed (1 action(s), 1 iteration(s))", defaultLines);
            Assert.AreEqual(string.Empty, defaultError);
        }
    }

    /// <summary>
    /// Verifies that all three parses use the version: a method returning a type named <c>union</c> parses under C# 14, so the
    /// fixture passes its parse check, the analyzer reports in a project parsed as C# 14, and the fix result passes its parse
    /// check, while without the option the fixture is a parse error
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncTypeNamedUnionUnderCSharp14IsFixed()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("C.cs", TypeNamedUnionSource);

            // Act
            var (exitCode, output, error) = await RunAsync([ExpressionBodiedMethodDiagnosticId, fixture.Path, "--lang-version", "14"]);
            var (defaultExitCode, defaultOutput, defaultError) = await RunAsync([ExpressionBodiedMethodDiagnosticId, fixture.Path]);

            // Assert
            var lines = SplitLines(output);
            var defaultLines = SplitLines(defaultOutput);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains("== C.cs [LF] == fixed (1 action(s), 1 iteration(s))", lines);
            Assert.Contains("== C.cs [CRLF] == fixed (1 action(s), 1 iteration(s))", lines);
            Assert.Contains("+        return null;", lines);
            Assert.AreEqual(string.Empty, error);

            Assert.AreEqual(ExitCodes.Error, defaultExitCode);
            Assert.Contains("== C.cs [LF] == parse-error", defaultLines);
            Assert.Contains("== C.cs [CRLF] == parse-error", defaultLines);
            Assert.AreEqual(string.Empty, defaultError);
        }
    }

    /// <summary>
    /// Verifies that a value newer than the newest supported version is lowered to it with one warning on standard error, and
    /// that the run is the same as without the option
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncPreviewWarnsAndRunsAsWithoutTheOption()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            fixture.CreateFile("D.cs", AnonymousMethodInInterpolationSource);

            var (defaultExitCode, defaultOutput, _) = await RunAsync([ExpressionBodiedMethodDiagnosticId, fixture.Path]);

            // Act
            var (exitCode, output, error) = await RunAsync([ExpressionBodiedMethodDiagnosticId, fixture.Path, "--lang-version", "preview"]);

            // Assert
            Assert.AreEqual(defaultExitCode, exitCode);
            Assert.AreEqual(defaultOutput, output);
            Assert.AreEqual(PreviewWarning + Environment.NewLine, error);
        }
    }

    /// <summary>
    /// Verifies the argument errors of the option: each fails before the fixture directory is read, with its message and the
    /// usage text on standard error and the error exit code. The value is always the argument behind the option, also when it
    /// starts with <c>-</c>, and an empty value is not a language version
    /// </summary>
    /// <param name="first">The first argument behind the fixture directory</param>
    /// <param name="second">The second argument behind the fixture directory, or <see langword="null"/> for none</param>
    /// <param name="third">The third argument behind the fixture directory, or <see langword="null"/> for none</param>
    /// <param name="fourth">The fourth argument behind the fixture directory, or <see langword="null"/> for none</param>
    /// <param name="expectedMessage">The expected first line of standard error</param>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [DataRow("--lang-version", "99", null, null, "apply-fix: invalid value for --lang-version: '99'. " + ExpectedVersionHint)]
    [DataRow("--lang-version", "", null, null, "apply-fix: invalid value for --lang-version: ''. " + ExpectedVersionHint)]
    [DataRow("--lang-version", "--max-iterations", "1", null, "apply-fix: invalid value for --lang-version: '--max-iterations'. " + ExpectedVersionHint)]
    [DataRow("--lang-version", null, null, null, "apply-fix: missing value for --lang-version.")]
    [DataRow("--lang-version", "10", "--lang-version", "10", "apply-fix: --lang-version may only be specified once.")]
    public async Task ExecuteAsyncInvalidLangVersionArgumentsReturnError(string first, string second, string third, string fourth, string expectedMessage)
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var missingDirectory = Path.Combine(fixture.Path, "does-not-exist");
            var arguments = new[] { ExpressionBodiedMethodDiagnosticId, missingDirectory, first, second, third, fourth }.Where(argument => argument != null).ToArray();

            // Act
            var output = new StringWriter();
            var error = new StringWriter();
            var exitCode = await CodeFixRunCommand.ExecuteAsync(arguments, output, error, TestContext.CancellationToken);

            // Assert
            var newLine = error.NewLine;
            var expectedError = $"{expectedMessage}{newLine}{UsageLine}{newLine}{HelpLine}{newLine}";

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreEqual(expectedError, error.ToString());
            Assert.AreEqual(string.Empty, output.ToString());
        }
    }

    /// <summary>
    /// Verifies that behind a literal <c>--</c> the option name is the fixture directory
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncLangVersionAfterDoubleDashIsTheFixtureDirectory()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();

        // Act
        var exitCode = await CodeFixRunCommand.ExecuteAsync([ExpressionBodiedMethodDiagnosticId, "--", "--lang-version"], output, error, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.AreEqual("apply-fix: fixture directory not found: --lang-version" + error.NewLine, error.ToString());
        Assert.AreEqual(string.Empty, output.ToString());
    }

    /// <summary>
    /// Verifies that the usage text names the option
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncHelpNamesTheLangVersionOption()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();

        // Act
        var exitCode = await CodeFixRunCommand.ExecuteAsync(["--help"], output, error, TestContext.CancellationToken);

        // Assert
        var expectedOutput = $"{UsageLine}{output.NewLine}{HelpLine}{output.NewLine}";

        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.AreEqual(expectedOutput, output.ToString());
        Assert.AreEqual(string.Empty, error.ToString());
    }

    /// <summary>
    /// Splits captured output into its lines, whichever line terminator the writer or the diff used
    /// </summary>
    /// <param name="text">The captured output</param>
    /// <returns>The lines</returns>
    private static string[] SplitLines(string text)
    {
        return text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
    }

    /// <summary>
    /// Runs the command and captures its streams
    /// </summary>
    /// <param name="arguments">The command arguments</param>
    /// <returns>The exit code and the captured output and error text</returns>
    private async Task<(int ExitCode, string Output, string Error)> RunAsync(string[] arguments)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var exitCode = await CodeFixRunCommand.ExecuteAsync(arguments, output, error, TestContext.CancellationToken);

        return (exitCode, output.ToString(), error.ToString());
    }

    #endregion // Methods
}