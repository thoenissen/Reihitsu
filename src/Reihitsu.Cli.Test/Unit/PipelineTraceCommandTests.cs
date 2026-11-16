using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Diagnostics;
using Reihitsu.Cli.Test.Helpers;

namespace Reihitsu.Cli.Test.Unit;

/// <summary>
/// Tests for <see cref="PipelineTraceCommand"/>
/// </summary>
[TestClass]
public sealed class PipelineTraceCommandTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that changed phases and their unified diffs are reported while the input bytes remain untouched
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncReportsChangedPhasesAndLeavesInputUntouched()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var filePath = fixture.CreateFile("path with spaces/Sample.cs", "internal class Sample{public void Run(){return;}}");
            var originalBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);
            var output = new StringWriter();
            var error = new StringWriter();

            // Act
            var exitCode = await PipelineTraceCommand.ExecuteAsync([filePath],
                                                                   output,
                                                                   error,
                                                                   TestContext.CancellationToken);

            // Assert
            var trace = output.ToString();

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains("maximum passes: 3", trace);
            Assert.Contains("== Pass 1: LineBreakPhase ==", trace);
            Assert.Contains("--- a/", trace);
            Assert.Contains("+++ b/", trace);
            Assert.Contains("Stable after pass 2", trace);
            Assert.AreEqual(string.Empty, error.ToString());

            var currentBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            Assert.AreSequenceEqual(originalBytes, currentBytes);
        }
    }

    /// <summary>
    /// Verifies that a one-pass trace reports exhaustion when the first pass changes source
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncOnePassReturnsFormattingNeededWhenSourceChanges()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var filePath = fixture.CreateFile("Sample.cs", "internal class Sample{}");
            var output = new StringWriter();
            var error = new StringWriter();

            // Act
            var exitCode = await PipelineTraceCommand.ExecuteAsync([filePath, "--passes", "1"],
                                                                   output,
                                                                   error,
                                                                   TestContext.CancellationToken);

            // Assert
            Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
            Assert.Contains("Not stable after 1 pass(es).", output.ToString());
            Assert.AreEqual(string.Empty, error.ToString());
        }
    }

    /// <summary>
    /// Verifies that already-formatted source stabilizes in its first pass without a phase diff
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncAlreadyFormattedSourceStabilizesOnFirstPass()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var source = "internal class Sample;";
            var filePath = fixture.CreateFile("Sample.cs", source);
            var output = new StringWriter();
            var error = new StringWriter();

            // Act
            var exitCode = await PipelineTraceCommand.ExecuteAsync([filePath],
                                                                   output,
                                                                   error,
                                                                   TestContext.CancellationToken);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains("Stable after pass 1 (0 phase changes observed).", output.ToString());
            Assert.IsFalse(output.ToString().Contains("== Pass 1:", StringComparison.Ordinal));
            Assert.AreEqual(string.Empty, error.ToString());
        }
    }

    /// <summary>
    /// Verifies that tracing preserves each consistently detected line-ending convention in memory and on disk
    /// </summary>
    /// <param name="endOfLine">Line-ending convention to exercise</param>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task ExecuteAsyncPreservesDetectedLineEndings(string endOfLine)
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var source = string.Join(endOfLine,
                                     "internal class Sample",
                                     "{",
                                     "public void Run(){}",
                                     "}");
            var filePath = fixture.CreateFile("Sample.cs", source);
            var originalBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);
            var output = new StringWriter();
            var error = new StringWriter();

            // Act
            var exitCode = await PipelineTraceCommand.ExecuteAsync([filePath],
                                                                   output,
                                                                   error,
                                                                   TestContext.CancellationToken);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.IsFalse(output.ToString().Contains("PreparationPhase/LineEndings", StringComparison.Ordinal));
            Assert.AreEqual(string.Empty, error.ToString());

            var currentBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            Assert.AreSequenceEqual(originalBytes, currentBytes);
        }
    }

    /// <summary>
    /// Verifies that a line-ending-only phase change reports separator counts when no unified line hunk exists
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncMixedLineEndingsReportsCountFallback()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var source = "internal class Sample\r\n{\n    public void Run()\r\n    {\n    }\r\n}";
            var filePath = fixture.CreateFile("Sample.cs", source);
            var originalBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);
            var output = new StringWriter();
            var error = new StringWriter();

            // Act
            var exitCode = await PipelineTraceCommand.ExecuteAsync([filePath],
                                                                   output,
                                                                   error,
                                                                   TestContext.CancellationToken);

            // Assert
            var trace = output.ToString();

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains("== Pass 1: PreparationPhase/LineEndings ==", trace);
            Assert.Contains("Line endings: CRLF=3, LF=2, CR=0 -> CRLF=5, LF=0, CR=0", trace);
            Assert.Contains("Stable after pass 2", trace);
            Assert.DoesNotContain("--- a/", trace);
            Assert.AreEqual(string.Empty, error.ToString());

            var currentBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            Assert.AreSequenceEqual(originalBytes, currentBytes);
        }
    }

    /// <summary>
    /// Verifies that the trace reports the replacement of a tab under the name of the TabReplacement sub-phase of the
    /// preparation phase, which is the first phase the trace shows
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncReportsTabReplacementUnderItsSubPhaseName()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var source = "internal class Sample\n{\n\tpublic void Run()\n\t{\n\t}\n}";
            var filePath = fixture.CreateFile("Sample.cs", source);
            var output = new StringWriter();
            var error = new StringWriter();

            // Act
            var exitCode = await PipelineTraceCommand.ExecuteAsync([filePath],
                                                                   output,
                                                                   error,
                                                                   TestContext.CancellationToken);

            // Assert
            var trace = output.ToString();
            var firstPhaseHeader = trace.IndexOf("== Pass 1:", StringComparison.Ordinal);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.IsGreaterThanOrEqualTo(0, firstPhaseHeader, "The trace should report a phase change in the first pass.");
            Assert.AreEqual(firstPhaseHeader, trace.IndexOf("== Pass 1: PreparationPhase/TabReplacement ==", StringComparison.Ordinal), "The first phase change of the trace should be the TabReplacement sub-phase.");
            Assert.DoesNotContain("TabReplacementPhase", trace);
            Assert.AreEqual(string.Empty, error.ToString());
        }
    }

    /// <summary>
    /// Verifies that syntax-invalid and generated source are legitimate no-op skips
    /// </summary>
    /// <param name="relativePath">Input path, including any generated-file suffix</param>
    /// <param name="source">Source text to trace</param>
    /// <param name="expectedMessage">Expected skip reason</param>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [DataRow("Sample.cs", "internal class {", "Skipped (syntax errors)")]
    [DataRow("Sample.cs", "// <auto-generated/>\ninternal class Generated{}", "Skipped (generated)")]
    [DataRow("Sample.Designer.cs", "internal class Generated{}", "Skipped (generated)")]
    [DataRow("Sample.g.cs", "internal class Generated{}", "Skipped (generated)")]
    [DataRow("Sample.g.i.cs", "internal class Generated{}", "Skipped (generated)")]
    public async Task ExecuteAsyncInvalidOrGeneratedSourceIsSkipped(string relativePath, string source, string expectedMessage)
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var filePath = fixture.CreateFile(relativePath, source);
            var originalBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);
            var output = new StringWriter();
            var error = new StringWriter();

            // Act
            var exitCode = await PipelineTraceCommand.ExecuteAsync([filePath],
                                                                   output,
                                                                   error,
                                                                   TestContext.CancellationToken);

            // Assert
            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains(expectedMessage, output.ToString());
            Assert.AreEqual(string.Empty, error.ToString());

            var currentBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            Assert.AreSequenceEqual(originalBytes, currentBytes);
        }
    }

    /// <summary>
    /// Verifies that a file holding code hidden in another build configuration is skipped before any phase runs
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <param name="carrier">Lines of the carrier inside the active conditional group</param>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [DataRow("\n", "    string s = @\"|#else|\";")]
    [DataRow("\r\n", "    string s = @\"|#else|\";")]
    [DataRow("\n", "    /*|#endif|    */")]
    [DataRow("\r\n", "    /*|#endif|    */")]
    public async Task ExecuteAsyncHiddenCodeIsSkippedWithoutRunningAnyPhase(string endOfLine, string carrier)
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var source = string.Join(endOfLine,
                                     "class C",
                                     "{",
                                     "#if true",
                                     carrier.Replace("|", endOfLine, StringComparison.Ordinal),
                                     "#endif",
                                     "    void M( ) { }",
                                     "}");
            var filePath = fixture.CreateFile("Sample.cs", source);
            var fullPath = Path.GetFullPath(filePath);
            var originalBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);
            var output = new StringWriter();
            var error = new StringWriter();

            // Act
            var exitCode = await PipelineTraceCommand.ExecuteAsync([filePath],
                                                                   output,
                                                                   error,
                                                                   TestContext.CancellationToken);

            // Assert
            var expectedOutput = $"Tracing {fullPath} (maximum passes: 3){output.NewLine}"
                                 + $"Skipped (code hidden in another build configuration): {fullPath}{output.NewLine}";

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(expectedOutput, output.ToString());
            Assert.AreEqual(string.Empty, error.ToString());

            var currentBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            Assert.AreSequenceEqual(originalBytes, currentBytes);
        }
    }

    /// <summary>
    /// Verifies that the hidden-code skip applies before the precheck, so a file that also holds an unsupported shape is named as skipped and not refused
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncHiddenCodeIsSkippedBeforeUnsupportedSyntaxIsRefused()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var source = string.Join("\n",
                                     "class C",
                                     "{",
                                     "#if true",
                                     "    string s = @\"",
                                     "#else",
                                     "\";",
                                     "#endif",
                                     "    int a /* c */ = 1;",
                                     "    void M( ) { }",
                                     "}");
            var filePath = fixture.CreateFile("Sample.cs", source);
            var fullPath = Path.GetFullPath(filePath);
            var output = new StringWriter();
            var error = new StringWriter();

            // Act
            var exitCode = await PipelineTraceCommand.ExecuteAsync([filePath],
                                                                   output,
                                                                   error,
                                                                   TestContext.CancellationToken);

            // Assert
            var expectedOutput = $"Tracing {fullPath} (maximum passes: 3){output.NewLine}"
                                 + $"Skipped (code hidden in another build configuration): {fullPath}{output.NewLine}";

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(expectedOutput, output.ToString());
            Assert.AreEqual(string.Empty, error.ToString());
        }
    }

    /// <summary>
    /// Verifies that a file with a syntax error or generated source is skipped under that reason, ahead of the hidden-code skip
    /// </summary>
    /// <param name="relativePath">Input path, including any generated-file suffix</param>
    /// <param name="header">Lines written in front of the class</param>
    /// <param name="trailer">Lines written behind the class</param>
    /// <param name="expectedMessage">Expected skip reason</param>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [DataRow("Sample.cs", "// <auto-generated/>", "", "Skipped (generated)")]
    [DataRow("Sample.g.cs", "", "", "Skipped (generated)")]
    [DataRow("Sample.cs", "", "internal class {", "Skipped (syntax errors)")]
    public async Task ExecuteAsyncEarlierSkipTakesPrecedenceOverHiddenCode(string relativePath, string header, string trailer, string expectedMessage)
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var source = string.Join("\n",
                                     header,
                                     "class C",
                                     "{",
                                     "#if true",
                                     "    string s = @\"",
                                     "#else",
                                     "\";",
                                     "#endif",
                                     "    void M( ) { }",
                                     "}",
                                     trailer);
            var filePath = fixture.CreateFile(relativePath, source);
            var originalBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);
            var output = new StringWriter();
            var error = new StringWriter();

            // Act
            var exitCode = await PipelineTraceCommand.ExecuteAsync([filePath],
                                                                   output,
                                                                   error,
                                                                   TestContext.CancellationToken);

            // Assert
            var trace = output.ToString();

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.Contains($"{expectedMessage}: {Path.GetFullPath(filePath)}", trace);
            Assert.DoesNotContain("code hidden in another build configuration", trace);
            Assert.DoesNotContain("Pass 1", trace);
            Assert.AreEqual(string.Empty, error.ToString());

            var currentBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            Assert.AreSequenceEqual(originalBytes, currentBytes);
        }
    }

    /// <summary>
    /// Verifies that a multi-line literal outside the hidden-code definition is traced: a directive-like line without an enclosing conditional group, and a non-directive line inside one
    /// </summary>
    /// <param name="ifLine">Line in front of the field, or an empty string for none</param>
    /// <param name="innerLine">Line inside the verbatim string</param>
    /// <param name="endIfLine">Line behind the field, or an empty string for none</param>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [DataRow("", "#else", "")]
    [DataRow("#if true", "# Title", "#endif")]
    public async Task ExecuteAsyncLiteralOutsideHiddenCodeDefinitionIsTraced(string ifLine, string innerLine, string endIfLine)
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var source = string.Join("\n",
                                     "class C",
                                     "{",
                                     ifLine,
                                     "    string s = @\"",
                                     innerLine,
                                     "\";",
                                     endIfLine,
                                     "    void M( ) { }",
                                     "}");
            var filePath = fixture.CreateFile("Sample.cs", source);
            var originalBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);
            var output = new StringWriter();
            var error = new StringWriter();

            // Act
            var exitCode = await PipelineTraceCommand.ExecuteAsync([filePath],
                                                                   output,
                                                                   error,
                                                                   TestContext.CancellationToken);

            // Assert
            var trace = output.ToString();

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.DoesNotContain("Skipped", trace);
            Assert.Contains("Pass 1" + output.NewLine, trace);
            Assert.Contains("== Pass 1: HorizontalSpacingPhase ==", trace);
            Assert.AreEqual(string.Empty, error.ToString());

            var currentBytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);

            Assert.AreSequenceEqual(originalBytes, currentBytes);
        }
    }

    /// <summary>
    /// Verifies that missing paths and malformed argument lists return the command-error exit code
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task ExecuteAsyncInvalidArgumentsAndMissingFilesReturnError()
    {
        // Arrange
        using (var fixture = new TemporaryDirectoryFixture())
        {
            var existingPath = fixture.CreateFile("Sample.cs", "internal class Sample{}");
            var missingPath = Path.Combine(fixture.Path, "Missing.cs");
            var cases = new[]
                        {
                            Array.Empty<string>(),
                            new[] { existingPath, "--passes", "0" },
                            new[] { existingPath, "--passes", "-1" },
                            new[] { existingPath, "--passes", "not-a-number" },
                            new[] { existingPath, "--passes" },
                            new[] { existingPath, "--passes", "1", "--passes", "2" },
                            new[] { "--unknown", existingPath },
                            new[] { existingPath, existingPath },
                            new[] { missingPath }
                        };

            foreach (var arguments in cases)
            {
                var output = new StringWriter();
                var error = new StringWriter();

                // Act
                var exitCode = await PipelineTraceCommand.ExecuteAsync(arguments,
                                                                       output,
                                                                       error,
                                                                       TestContext.CancellationToken);

                // Assert
                Assert.AreEqual(ExitCodes.Error, exitCode, string.Join(' ', arguments));
                Assert.AreNotEqual(string.Empty, error.ToString(), string.Join(' ', arguments));
            }
        }
    }

    #endregion // Methods
}