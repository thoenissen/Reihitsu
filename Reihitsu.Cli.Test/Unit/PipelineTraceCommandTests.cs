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
            Assert.AreSequenceEqual(originalBytes, await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken));
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
            Assert.IsFalse(output.ToString().Contains("LineEndingNormalizationPhase", StringComparison.Ordinal));
            Assert.AreEqual(string.Empty, error.ToString());
            Assert.AreSequenceEqual(originalBytes, await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken));
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
            Assert.Contains("== Pass 1: LineEndingNormalizationPhase ==", trace);
            Assert.Contains("Line endings: CRLF=3, LF=2, CR=0 -> CRLF=5, LF=0, CR=0", trace);
            Assert.Contains("Stable after pass 2", trace);
            Assert.DoesNotContain("--- a/", trace);
            Assert.AreEqual(string.Empty, error.ToString());
            Assert.AreSequenceEqual(originalBytes, await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken));
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
            Assert.AreSequenceEqual(originalBytes, await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken));
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