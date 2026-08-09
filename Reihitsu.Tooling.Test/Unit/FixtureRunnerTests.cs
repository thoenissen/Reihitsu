using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Tooling.Enumerations;

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