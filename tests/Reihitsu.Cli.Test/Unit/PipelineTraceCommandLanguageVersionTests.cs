using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Diagnostics;
using Reihitsu.Cli.Test.Helpers;

namespace Reihitsu.Cli.Test.Unit;

/// <summary>
/// Tests for the <c>--lang-version</c> option of <see cref="PipelineTraceCommand"/>: it takes the values <c>reihitsu-format</c>
/// accepts, resolves them as <c>reihitsu-format</c> does, and the trace parses and formats every pass for the resolved version
/// </summary>
[TestClass]
public sealed class PipelineTraceCommandLanguageVersionTests
{
    #region Constants

    /// <summary>
    /// A method whose non-verbatim interpolation holds a line break. From C# 11 on, the formatter moves the operator to the
    /// start of the hole's second line; before C# 11 it writes no line break there and the file stays as written
    /// </summary>
    private const string InterpolationHoleSource = "class C\n{\n    string M(int a)\n    {\n        return $\"{a +\n            1}\";\n    }\n}";

    /// <summary>
    /// A file declaring a union, which parses only from C# 15 on
    /// </summary>
    private const string UnionSource = "public union Pet(Cat, Dog)\n{\n    public int Legs() => 4;\n}\n\npublic class Cat { }\n\npublic class Dog { }";

    /// <summary>
    /// A type named <c>union</c> used as a method's return type, which C# 15 reads as a union declaration and C# 14 parses
    /// </summary>
    private const string TypeNamedUnionSource = "class union { }\nclass C { union M(int a) { return null; } }";

    /// <summary>
    /// The warning for a value newer than the newest supported version
    /// </summary>
    private const string PreviewWarning = "trace: Warning: C# language version 'preview' is newer than the newest supported version 15.0; formatting for 15.0.";

    /// <summary>
    /// The first line of the usage text
    /// </summary>
    private const string UsageLine = "usage: trace <file> [--passes <positive-integer>] [--lang-version <version>]";

    /// <summary>
    /// The second line of the usage text
    /// </summary>
    private const string HelpLine = "       trace --help";

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
    /// Verifies that under C# 10 the trace leaves a line break in a non-verbatim interpolation as written, as
    /// <c>reihitsu-format --lang-version 10</c> does, wherever the option stands among the arguments
    /// </summary>
    /// <param name="value">The language version value</param>
    /// <param name="optionFirst">Whether the option stands in front of the file</param>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [DataRow("10", false)]
    [DataRow("10", true)]
    [DataRow("10.0", false)]
    public async Task ExecuteAsyncBeforeCSharp11KeepsTheInterpolationAsWritten(string value, bool optionFirst)
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var filePath = fixture.CreateFile("H.cs", InterpolationHoleSource);
            string[] arguments = optionFirst ? ["--lang-version", value, filePath] : [filePath, "--lang-version", value];

            // Act
            var (exitCode, output, error, newLine) = await RunAsync(arguments);

            // Assert
            var expectedOutput = $"Tracing {Path.GetFullPath(filePath)} (maximum passes: 3){newLine}Pass 1{newLine}Stable after pass 1 (0 phase changes observed).{newLine}";

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(expectedOutput, output);
            Assert.AreEqual(string.Empty, error);
        }
    }

    /// <summary>
    /// Verifies that without the option the trace formats the interpolation for the newest supported version, which moves the
    /// operator to the start of the hole's second line
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncWithoutLangVersionFormatsTheInterpolationForTheNewestVersion()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var filePath = fixture.CreateFile("H.cs", InterpolationHoleSource);

            // Act
            var (exitCode, output, error, newLine) = await RunAsync([filePath]);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains("== Pass 1: LineBreakPhase ==", output);
            Assert.Contains($"+                  + 1}}\";{newLine}", output);
            Assert.AreEqual(string.Empty, error);
        }
    }

    /// <summary>
    /// Verifies that every value that resolves to C# 11 or later, up to the newest supported version, traces the interpolation
    /// exactly as a run without the option does, without a warning
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
    public async Task ExecuteAsyncFromCSharp11TracesAsWithoutTheOption(string value)
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var filePath = fixture.CreateFile("H.cs", InterpolationHoleSource);
            var (defaultExitCode, defaultOutput, _, _) = await RunAsync([filePath]);

            // Act
            var (exitCode, output, error, _) = await RunAsync([filePath, "--lang-version", value]);

            // Assert
            Assert.AreEqual(defaultExitCode, exitCode);
            Assert.AreEqual(defaultOutput, output);
            Assert.AreEqual(string.Empty, error);
        }
    }

    /// <summary>
    /// Verifies that a value newer than the newest supported version is lowered to it with one warning on standard error, and
    /// that the trace is the same as without the option
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncPreviewWarnsAndTracesAsWithoutTheOption()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var filePath = fixture.CreateFile("H.cs", InterpolationHoleSource);
            var (defaultExitCode, defaultOutput, _, _) = await RunAsync([filePath]);

            // Act
            var (exitCode, output, error, newLine) = await RunAsync([filePath, "--lang-version", "preview"]);

            // Assert
            Assert.AreEqual(defaultExitCode, exitCode);
            Assert.AreEqual(defaultOutput, output);
            Assert.AreEqual(PreviewWarning + newLine, error);
        }
    }

    /// <summary>
    /// Verifies that the trace parses every pass with the version: a union declaration is a syntax error under C# 14, so the
    /// trace skips the file, while without the option it traces it
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncUnionUnderCSharp14IsSkippedForSyntaxErrors()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var filePath = fixture.CreateFile("Pet.cs", UnionSource);
            var fullPath = Path.GetFullPath(filePath);

            // Act
            var (exitCode, output, error, newLine) = await RunAsync([filePath, "--lang-version", "14"]);
            var (defaultExitCode, defaultOutput, defaultError, _) = await RunAsync([filePath]);

            // Assert
            var expectedOutput = $"Tracing {fullPath} (maximum passes: 3){newLine}"
                                 + $"Skipped (syntax errors): {fullPath}{newLine}";

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(expectedOutput, output);
            Assert.AreEqual(string.Empty, error);

            Assert.AreEqual(ExitCodes.Success, defaultExitCode);
            Assert.DoesNotContain("Skipped", defaultOutput);
            Assert.Contains("== Pass 1: ", defaultOutput);
            Assert.AreEqual(string.Empty, defaultError);
        }
    }

    /// <summary>
    /// Verifies that the trace parses every pass with the version: a type named <c>union</c> used as a return type parses under
    /// C# 14, so the trace formats the file, while without the option it is skipped for syntax errors
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncTypeNamedUnionUnderCSharp14IsTraced()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var filePath = fixture.CreateFile("C.cs", TypeNamedUnionSource);
            var fullPath = Path.GetFullPath(filePath);

            // Act
            var (exitCode, output, error, newLine) = await RunAsync([filePath, "--lang-version", "14"]);
            var (defaultExitCode, defaultOutput, defaultError, _) = await RunAsync([filePath]);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.DoesNotContain("Skipped", output);
            Assert.Contains("== Pass 1: ", output);
            Assert.AreEqual(string.Empty, error);

            Assert.AreEqual(ExitCodes.Success, defaultExitCode);
            Assert.Contains($"Skipped (syntax errors): {fullPath}{newLine}", defaultOutput);
            Assert.AreEqual(string.Empty, defaultError);
        }
    }

    /// <summary>
    /// Verifies the argument errors of the option: each fails before the file is read, with its message and the usage text on
    /// standard error and the error exit code. The value is always the argument behind the option, also when it starts with
    /// <c>-</c>, and an empty value is not a language version
    /// </summary>
    /// <param name="first">The first argument behind the file</param>
    /// <param name="second">The second argument behind the file, or <see langword="null"/> for none</param>
    /// <param name="third">The third argument behind the file, or <see langword="null"/> for none</param>
    /// <param name="fourth">The fourth argument behind the file, or <see langword="null"/> for none</param>
    /// <param name="expectedMessage">The expected first line of standard error</param>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [DataRow("--lang-version", "99", null, null, "trace: invalid value for --lang-version: '99'. " + ExpectedVersionHint)]
    [DataRow("--lang-version", "", null, null, "trace: invalid value for --lang-version: ''. " + ExpectedVersionHint)]
    [DataRow("--lang-version", "--passes", "1", null, "trace: invalid value for --lang-version: '--passes'. " + ExpectedVersionHint)]
    [DataRow("--lang-version", null, null, null, "trace: missing value for --lang-version.")]
    [DataRow("--lang-version", "10", "--lang-version", "10", "trace: --lang-version may only be specified once.")]
    public async Task ExecuteAsyncInvalidLangVersionArgumentsReturnError(string first, string second, string third, string fourth, string expectedMessage)
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var missingPath = Path.Combine(fixture.Path, "Missing.cs");
            var arguments = new[] { missingPath, first, second, third, fourth }.Where(argument => argument != null).ToArray();

            // Act
            var (exitCode, output, error, newLine) = await RunAsync(arguments);

            // Assert
            var expectedError = $"{expectedMessage}{newLine}{UsageLine}{newLine}{HelpLine}{newLine}";

            Assert.AreEqual(ExitCodes.Error, exitCode);
            Assert.AreEqual(expectedError, error);
            Assert.AreEqual(string.Empty, output);
        }
    }

    /// <summary>
    /// Verifies that behind a literal <c>--</c> the option name is the file path
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncLangVersionAfterDoubleDashIsTheFilePath()
    {
        // Act
        var (exitCode, output, error, newLine) = await RunAsync(["--", "--lang-version"]);

        // Assert
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.AreEqual("trace: file not found: --lang-version" + newLine, error);
        Assert.AreEqual(string.Empty, output);
    }

    /// <summary>
    /// Verifies that the usage text names the option
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncHelpNamesTheLangVersionOption()
    {
        // Act
        var (exitCode, output, error, newLine) = await RunAsync(["--help"]);

        // Assert
        var expectedOutput = $"{UsageLine}{newLine}{HelpLine}{newLine}";

        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.AreEqual(expectedOutput, output);
        Assert.AreEqual(string.Empty, error);
    }

    /// <summary>
    /// Runs the trace and captures its streams
    /// </summary>
    /// <param name="arguments">The trace arguments</param>
    /// <returns>The exit code, the standard output, the standard error, and the line terminator of the writers</returns>
    private async Task<(int ExitCode, string Output, string Error, string NewLine)> RunAsync(string[] arguments)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var exitCode = await PipelineTraceCommand.ExecuteAsync(arguments, output, error, TestContext.CancellationToken);

        return (exitCode, output.ToString(), error.ToString(), output.NewLine);
    }

    #endregion // Methods
}