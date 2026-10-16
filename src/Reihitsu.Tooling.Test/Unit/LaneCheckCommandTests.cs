using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Tooling.Test.Unit;

/// <summary>
/// Tests for <see cref="LaneCheckCommand"/>
/// </summary>
[TestClass]
public sealed class LaneCheckCommandTests
{
    #region Methods

    /// <summary>
    /// Verifies that a claim matching the diff is confirmed and every path is listed with its category
    /// </summary>
    [TestMethod]
    public void ExecuteConfirmsAMatchingClaim()
    {
        // Act
        var (exitCode, output, _) = Run("M\tsrc/Reihitsu.Formatter/Pipeline/FormattingPipeline.cs\nA\tsrc/Reihitsu.Formatter.Test/Unit/SampleTests.cs\n", "--lane", "bug-fix", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("Production src/Reihitsu.Formatter/Pipeline/FormattingPipeline.cs", output);
        Assert.Contains("Test       src/Reihitsu.Formatter.Test/Unit/SampleTests.cs", output);
        Assert.Contains("LANE CHECK: confirmed — claimed bug-fix, diff needs bug-fix", output);
    }

    /// <summary>
    /// Verifies that a claim wider than the diff is confirmed and says so, which is how a bug that did not reproduce
    /// ends: the bug-fix lane with a characterization test only
    /// </summary>
    [TestMethod]
    public void ExecuteConfirmsAWiderClaim()
    {
        // Act
        var (exitCode, output, _) = Run("A\tsrc/Reihitsu.Formatter.Test/Unit/SampleTests.cs\n", "--lane", "bug-fix", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("LANE CHECK: confirmed — claimed bug-fix, diff needs only tests", output);
    }

    /// <summary>
    /// Verifies that a claim narrower than the diff is a mismatch naming the lane the diff needs
    /// </summary>
    [TestMethod]
    public void ExecuteRejectsANarrowerClaim()
    {
        // Act
        var (exitCode, output, _) = Run("M\tsrc/Reihitsu.Formatter/Pipeline/FormattingPipeline.cs\n", "--lane", "docs", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.LaneMismatch, exitCode);
        Assert.Contains("LANE CHECK: mismatch — claimed docs, diff needs bug-fix", output);
    }

    /// <summary>
    /// Verifies that a proven text-only production change fits the docs lane
    /// </summary>
    [TestMethod]
    public void ExecuteAcceptsProvenTextOnlyCSharpInTheDocsLane()
    {
        // Act
        var (exitCode, output, _) = Run("M\tsrc/Reihitsu.Formatter/Pipeline/FormattingPipeline.cs\n", "--lane", "docs", "--text-only", "pass");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("LANE CHECK: confirmed — claimed docs, diff needs docs", output);
    }

    /// <summary>
    /// Verifies that the behavior-change lane is rejected without a change doc, even though it covers every other lane
    /// </summary>
    [TestMethod]
    public void ExecuteRejectsABehaviorChangeClaimWithoutChangeDoc()
    {
        // Act
        var (exitCode, output, _) = Run("M\tsrc/Reihitsu.Formatter/Pipeline/FormattingPipeline.cs\n", "--lane", "behavior-change", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.LaneMismatch, exitCode);
        Assert.Contains("no change doc under sdd/changes/; it needs bug-fix", output);
    }

    /// <summary>
    /// Verifies that an unclassified path ends in the no-lane verdict, which the lead escalates
    /// </summary>
    [TestMethod]
    public void ExecuteReportsNoLaneForAnUnknownPath()
    {
        // Act
        var (exitCode, output, _) = Run("A\tnotes.txt\n", "--lane", "docs", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.NoLane, exitCode);
        Assert.Contains("Unknown    notes.txt", output);
        Assert.Contains("LANE CHECK: no lane — claimed docs, but the diff contains paths no lane rule classifies", output);
    }

    /// <summary>
    /// Verifies that an empty diff is reported as such and fits the docs lane
    /// </summary>
    [TestMethod]
    public void ExecuteReportsAnEmptyDiff()
    {
        // Act
        var (exitCode, output, _) = Run(string.Empty, "--lane", "docs", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("no changed files", output);
    }

    /// <summary>
    /// Verifies that invalid arguments are a tool error rather than a verdict
    /// </summary>
    /// <param name="arguments">Command arguments, separated by spaces</param>
    [TestMethod]
    [DataRow("")]
    [DataRow("--lane hotfix --text-only fail")]
    [DataRow("--lane docs")]
    [DataRow("--lane docs --text-only maybe")]
    [DataRow("--lane docs --text-only fail --extra")]
    public void ExecuteRejectsInvalidArguments(string arguments)
    {
        // Act
        var (exitCode, _, error) = Run(string.Empty, arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.Contains("usage: lane-check", error);
    }

    /// <summary>
    /// Verifies that a line that is not a name-status line is a tool error rather than a verdict
    /// </summary>
    [TestMethod]
    public void ExecuteRejectsAMalformedChangeLine()
    {
        // Act
        var (exitCode, _, error) = Run("R100\told.cs\tnew.cs\n", "--lane", "docs", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.Contains("unexpected name-status line", error);
    }

    /// <summary>
    /// Runs the lane check over the provided name-status lines
    /// </summary>
    /// <param name="changes">Name-status lines of the diff</param>
    /// <param name="args">Command arguments</param>
    /// <returns>The exit code, standard output, and standard error</returns>
    private static (int ExitCode, string Output, string Error) Run(string changes, params string[] args)
    {
        using (var input = new StringReader(changes))
            using (var output = new StringWriter())
                using (var error = new StringWriter())
                {
                    var exitCode = LaneCheckCommand.Execute(args, input, output, error);

                    return (exitCode, output.ToString(), error.ToString());
                }
    }

    #endregion // Methods
}