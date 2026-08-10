using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Tooling.Enumerations;
using Reihitsu.Tooling.Test.Helpers;

namespace Reihitsu.Tooling.Test.Unit;

/// <summary>
/// Tests for <see cref="FixtureRunner"/>
/// </summary>
[TestClass]
public sealed class FixtureRunnerTests
{
    #region Constants

    /// <summary>
    /// Diagnostic ID used by the fixtures
    /// </summary>
    private const string CombinedFieldsDiagnosticId = "RH7101";

    /// <summary>
    /// A type carrying one combined field declaration
    /// </summary>
    private const string CombinedFieldSource = "internal class Sample\n{\n    private int _first, _second;\n}\n";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a fixture normalized to line feed is fixed without a carriage return appearing anywhere in
    /// the result, which is the observation that makes the line-ending arm meaningful rather than decorative
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RunAsyncKeepsLineFeedSourceFreeOfCarriageReturns()
    {
        // Arrange
        Assert.IsTrue(CodeFixTargetResolver.TryResolve(CombinedFieldsDiagnosticId, out var target, out _));

        // Act
        var result = await FixtureRunner.RunAsync(target,
                                                  CombinedFieldSource,
                                                  FixtureLineEndings.LineFeed,
                                                  10,
                                                  TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(FixtureOutcome.Fixed, result.Outcome);
        Assert.IsTrue(result.PreservedLineEnding);
        Assert.DoesNotContain("\r", result.FinalSource);
    }

    /// <summary>
    /// Verifies that the same fixture normalized to carriage return line feed is fixed without a lone line feed
    /// surviving, so a fix inserting the host's newline would be observable on this arm
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RunAsyncKeepsCarriageReturnLineFeedSourceFreeOfLoneLineFeeds()
    {
        // Arrange
        Assert.IsTrue(CodeFixTargetResolver.TryResolve(CombinedFieldsDiagnosticId, out var target, out _));

        // Act
        var result = await FixtureRunner.RunAsync(target,
                                                  CombinedFieldSource,
                                                  FixtureLineEndings.CarriageReturnLineFeed,
                                                  10,
                                                  TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(FixtureOutcome.Fixed, result.Outcome);
        Assert.IsTrue(result.PreservedLineEnding);
        Assert.AreEqual(0, CountLoneLineFeeds(result.FinalSource));
    }

    /// <summary>
    /// Verifies that source which does not parse is detected. This is the same check that classifies a fixture as
    /// a parse error and that guards against reporting an unparseable fix result as a success
    /// </summary>
    [TestMethod]
    public void HasSyntaxErrorsDetectsUnparseableSource()
    {
        // Act
        var unparseable = FixtureRunner.HasSyntaxErrors("internal class Sample\n{\n", TestContext.CancellationToken);
        var parseable = FixtureRunner.HasSyntaxErrors(CombinedFieldSource, TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(unparseable);
        Assert.IsFalse(parseable);
    }

    /// <summary>
    /// Verifies that a code action producing invalid source is classified as an invalid result after one iteration
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RunAsyncReportsInvalidResult()
    {
        // Arrange
        var target = CreateTarget(new TextReportingFakeAnalyzer(_ => true),
                                  new SourceTransformingFakeCodeFix(_ => "internal class Sample\n{\n"));

        // Act
        var result = await FixtureRunner.RunAsync(target,
                                                  "internal class Sample\n{\n}\n",
                                                  FixtureLineEndings.LineFeed,
                                                  10,
                                                  TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(FixtureOutcome.InvalidResult, result.Outcome);
        Assert.AreEqual(1, result.Iterations);
    }

    /// <summary>
    /// Verifies that a code action returning identical source is classified separately from iteration-cap exhaustion
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RunAsyncReportsNoProgress()
    {
        // Arrange
        var target = CreateTarget(new TextReportingFakeAnalyzer(_ => true),
                                  new SourceTransformingFakeCodeFix(source => source));

        // Act
        var result = await FixtureRunner.RunAsync(target,
                                                  "internal class Sample\n{\n}\n",
                                                  FixtureLineEndings.LineFeed,
                                                  10,
                                                  TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(FixtureOutcome.NoProgress, result.Outcome);
        Assert.AreEqual(1, result.Iterations);
    }

    /// <summary>
    /// Verifies that a converged fix retaining a separator from the wrong arm records line-ending drift
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RunAsyncReportsLineEndingDrift()
    {
        // Arrange
        var target = CreateTarget(new TextReportingFakeAnalyzer(source => source.Contains("_field", StringComparison.Ordinal) == false),
                                  new SourceTransformingFakeCodeFix(source => source.Replace("}\r\n",
                                                                                             "    private int _field;\n}\r\n",
                                                                                             StringComparison.Ordinal)));

        // Act
        var result = await FixtureRunner.RunAsync(target,
                                                  "internal class Sample\n{\n}\n",
                                                  FixtureLineEndings.CarriageReturnLineFeed,
                                                  10,
                                                  TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(FixtureOutcome.Fixed, result.Outcome);
        Assert.IsFalse(result.PreservedLineEnding);
        Assert.Contains("_field;\n}", result.FinalSource);
    }

    /// <summary>
    /// Verifies that Roslyn's analyzer-failure diagnostic is classified before the target diagnostic is filtered
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task RunAsyncReportsAnalyzerFailure()
    {
        // Arrange
        var target = CreateTarget(new ThrowingFakeAnalyzer(), new FirstFakeCodeFix());

        // Act
        var result = await FixtureRunner.RunAsync(target,
                                                  "internal class Sample\n{\n}\n",
                                                  FixtureLineEndings.LineFeed,
                                                  10,
                                                  TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(FixtureOutcome.AnalyzerFailure, result.Outcome);
        Assert.AreEqual(0, result.Iterations);
    }

    /// <summary>
    /// Builds a direct fixture target from deterministic test doubles
    /// </summary>
    /// <param name="analyzer">Analyzer to execute</param>
    /// <param name="codeFix">Code fix to execute</param>
    /// <returns>The fixture target</returns>
    private static CodeFixTarget CreateTarget(Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer analyzer,
                                              Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider codeFix)
    {
        return new CodeFixTarget(FakeDiagnostic.Id, [analyzer], codeFix);
    }

    /// <summary>
    /// Counts the line feeds that are not preceded by a carriage return
    /// </summary>
    /// <param name="text">Text to inspect</param>
    /// <returns>The number of lone line feeds</returns>
    private static int CountLoneLineFeeds(string text)
    {
        var count = 0;

        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] == '\n'
                && (index == 0 || text[index - 1] != '\r'))
            {
                count++;
            }
        }

        return count;
    }

    #endregion // Methods
}