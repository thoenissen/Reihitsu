using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Tooling.Test.Unit;

/// <summary>
/// Tests for <see cref="LaneCheckCommand"/>
/// </summary>
[TestClass]
public sealed class LaneCheckCommandTests
{
    #region Constants

    /// <summary>
    /// Name-status lines of a wording diff: both resource files, the rule page, and the package README that repeat the texts
    /// </summary>
    private const string WordingChanges = "M\tsrc/Reihitsu.Analyzer/AnalyzerResources.resx\n"
                                          + "M\tsrc/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx\n"
                                          + "M\tdocumentation/rules/RH5201.md\n"
                                          + "M\tsrc/Reihitsu.Analyzer.Package/README.MD\n";

    /// <summary>
    /// Name-status lines of a wording diff with its change doc, spec, a squad notes edit, and a comment edit in the analyzer
    /// </summary>
    private const string WordingChangesWithChangeDoc = WordingChanges
                                                       + "A\tsdd/changes/2026-11/rh5201-member-access-wording.md\n"
                                                       + "M\tsdd/specs/analyzer/RH5201.md\n"
                                                       + "M\tsquad/notes/README.md\n"
                                                       + "M\tsrc/Reihitsu.Analyzer/Rules/Layout/RH5201MethodChainsShouldBeAlignedAnalyzer.cs\n";

    #endregion // Constants

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
    /// Verifies that a diff changing only the root contributing guide is text and fits the docs lane
    /// </summary>
    [TestMethod]
    public void ExecuteConfirmsTheDocsLaneForTheRootContributingGuide()
    {
        // Act
        var (exitCode, output, _) = Run("M\tCONTRIBUTING.md\n", "--lane", "docs", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("Text       CONTRIBUTING.md", output);
        Assert.Contains("LANE CHECK: confirmed — claimed docs, diff needs docs", output);
    }

    /// <summary>
    /// Verifies that a diff changing only a root community file is text and fits the docs lane
    /// </summary>
    /// <param name="path">Root Markdown file</param>
    [TestMethod]
    [DataRow("CODE_OF_CONDUCT.md")]
    [DataRow("SECURITY.md")]
    public void ExecuteConfirmsTheDocsLaneForARootCommunityFile(string path)
    {
        // Act
        var (exitCode, output, _) = Run($"M\t{path}\n", "--lane", "docs", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains($"Text       {path}", output);
        Assert.Contains("LANE CHECK: confirmed — claimed docs, diff needs docs", output);
    }

    /// <summary>
    /// Verifies that the root contributing guide next to tooling paths leaves the tooling lane rather than no lane
    /// </summary>
    [TestMethod]
    public void ExecuteConfirmsTheToolingLaneForTheRootContributingGuideNextToTooling()
    {
        // Act
        var (exitCode, output, _) = Run("M\tAGENTS.md\nM\tCONTRIBUTING.md\nM\tscripts/test.sh\n", "--lane", "tooling", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("Text       CONTRIBUTING.md", output);
        Assert.Contains("LANE CHECK: confirmed — claimed tooling, diff needs tooling", output);
    }

    /// <summary>
    /// Verifies that a diff changing the two resource files and the text that repeats them is wording and fits the
    /// wording lane
    /// </summary>
    [TestMethod]
    public void ExecuteConfirmsTheWordingLaneForTheResourceFilesAndText()
    {
        // Act
        var (exitCode, output, _) = Run(WordingChanges, "--lane", "wording", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("Wording    src/Reihitsu.Analyzer/AnalyzerResources.resx", output);
        Assert.Contains("Wording    src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx", output);
        Assert.Contains("Text       documentation/rules/RH5201.md", output);
        Assert.Contains("LANE CHECK: confirmed — claimed wording, diff needs wording", output);
    }

    /// <summary>
    /// Verifies that a wording claim needs no change doc, unlike a behavior-change claim
    /// </summary>
    [TestMethod]
    public void ExecuteConfirmsAWordingClaimWithoutChangeDoc()
    {
        // Act
        var (exitCode, output, _) = Run("M\tsrc/Reihitsu.Analyzer/AnalyzerResources.resx\n", "--lane", "wording", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("LANE CHECK: confirmed — claimed wording, diff needs wording", output);
    }

    /// <summary>
    /// Verifies that the change of a diagnostic wording with its change doc, its spec, its notes, and a proven
    /// text-only comment edit in the analyzer fits the wording lane
    /// </summary>
    [TestMethod]
    public void ExecuteConfirmsTheWordingLaneForAWordingChangeWithChangeDocAndProvenCommentEdit()
    {
        // Act
        var (exitCode, output, _) = Run(WordingChangesWithChangeDoc, "--lane", "wording", "--text-only", "pass");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("ChangeDoc  sdd/changes/2026-11/rh5201-member-access-wording.md", output);
        Assert.Contains("Text       src/Reihitsu.Analyzer/Rules/Layout/RH5201MethodChainsShouldBeAlignedAnalyzer.cs", output);
        Assert.Contains("LANE CHECK: confirmed — claimed wording, diff needs wording", output);
    }

    /// <summary>
    /// Verifies that the same wording change with a changed test, which fails the text-only proof through its literal,
    /// needs the behavior-change lane, so a wording claim is a mismatch
    /// </summary>
    [TestMethod]
    public void ExecuteRejectsAWordingClaimForAWordingChangeWithAChangedTest()
    {
        // Arrange
        var changes = WordingChangesWithChangeDoc + "M\tsrc/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5201CommentExemptChainWithNonInvokedPrefixDotTests.cs\n";

        // Act
        var (exitCode, output, _) = Run(changes, "--lane", "wording", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.LaneMismatch, exitCode);
        Assert.Contains("LANE CHECK: mismatch — claimed wording, diff needs behavior-change", output);
    }

    /// <summary>
    /// Verifies that wording next to a test path without a change doc needs the tests lane, so a wording claim is a
    /// mismatch
    /// </summary>
    [TestMethod]
    public void ExecuteRejectsAWordingClaimForWordingWithATestPath()
    {
        // Arrange
        var changes = WordingChanges + "M\tsrc/Reihitsu.Analyzer.Test/Layout/RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs\n";

        // Act
        var (exitCode, output, _) = Run(changes, "--lane", "wording", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.LaneMismatch, exitCode);
        Assert.Contains("LANE CHECK: mismatch — claimed wording, diff needs tests", output);
    }

    /// <summary>
    /// Verifies that a claim narrower than or beside the wording lane is a mismatch naming the wording lane
    /// </summary>
    /// <param name="claimed">Claimed lane</param>
    [TestMethod]
    [DataRow("docs")]
    [DataRow("tooling")]
    public void ExecuteRejectsAClaimThatDoesNotCoverTheWordingLane(string claimed)
    {
        // Act
        var (exitCode, output, _) = Run(WordingChanges, "--lane", claimed, "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.LaneMismatch, exitCode);
        Assert.Contains($"LANE CHECK: mismatch — claimed {claimed}, diff needs wording", output);
    }

    /// <summary>
    /// Verifies that a tests or bug-fix claim covers a wording diff, also one that carries a change doc
    /// </summary>
    /// <param name="claimed">Claimed lane</param>
    [TestMethod]
    [DataRow("tests")]
    [DataRow("bug-fix")]
    public void ExecuteConfirmsAWiderClaimForAWordingDiffWithChangeDoc(string claimed)
    {
        // Act
        var (exitCode, output, _) = Run(WordingChangesWithChangeDoc, "--lane", claimed, "--text-only", "pass");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains($"LANE CHECK: confirmed — claimed {claimed}, diff needs only wording", output);
    }

    /// <summary>
    /// Verifies that a behavior-change claim over a wording diff still needs a change doc
    /// </summary>
    [TestMethod]
    public void ExecuteRejectsABehaviorChangeClaimForAWordingDiffWithoutChangeDoc()
    {
        // Act
        var (exitCode, output, _) = Run(WordingChanges, "--lane", "behavior-change", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.LaneMismatch, exitCode);
        Assert.Contains("no change doc under sdd/changes/; it needs wording", output);
    }

    /// <summary>
    /// Verifies that wording next to tooling without a change doc ends in the no-lane verdict
    /// </summary>
    [TestMethod]
    public void ExecuteReportsNoLaneForWordingAndToolingWithoutChangeDoc()
    {
        // Arrange
        var changes = WordingChanges + "M\tscripts/lane-check.sh\n";

        // Act
        var (exitCode, output, _) = Run(changes, "--lane", "behavior-change", "--text-only", "fail");

        // Assert
        Assert.AreEqual(ExitCodes.NoLane, exitCode);
        Assert.Contains("LANE CHECK: no lane — claimed behavior-change, but", output);
    }

    /// <summary>
    /// Verifies that wording next to tooling with a change doc needs the behavior-change lane
    /// </summary>
    [TestMethod]
    public void ExecuteConfirmsTheBehaviorChangeLaneForWordingAndToolingWithChangeDoc()
    {
        // Arrange
        var changes = WordingChangesWithChangeDoc + "M\tscripts/lane-check.sh\n";

        // Act
        var (exitCode, output, _) = Run(changes, "--lane", "behavior-change", "--text-only", "pass");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change", output);
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
    /// Verifies that the wording-paths query writes exactly the changed paths the classifier counts as wording, one per
    /// line in diff order, with no category column and no verdict, and leaves out every other resource file and the
    /// C# file that would be text under a passing proof
    /// </summary>
    [TestMethod]
    public void ExecuteWritesOnlyTheWordingPathsOfADiff()
    {
        // Arrange
        var changes = WordingChangesWithChangeDoc
                      + "M\tsrc/Reihitsu.Cli/CliResources.resx\n"
                      + "M\tsrc/Reihitsu.Analyzer/AnalyzerResources.de.resx\n";

        // Act
        var (exitCode, output, error) = Run(changes, "--wording-paths");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.AreEqual($"src/Reihitsu.Analyzer/AnalyzerResources.resx{Environment.NewLine}src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx{Environment.NewLine}", output);
        Assert.AreEqual(string.Empty, error);
    }

    /// <summary>
    /// Verifies that the wording-paths query writes nothing, and no verdict or empty-diff line, for a diff without a
    /// wording path and for an empty diff
    /// </summary>
    /// <param name="changes">Name-status lines of the diff</param>
    [TestMethod]
    [DataRow("M\tdocumentation/rules/RH5201.md\nM\tsrc/Reihitsu.Cli/CliResources.resx\nM\tsrc/Reihitsu.Analyzer/Rules/Layout/RH5201MethodChainsShouldBeAlignedAnalyzer.cs\n")]
    [DataRow("")]
    public void ExecuteWritesNoWordingPathForADiffWithoutWording(string changes)
    {
        // Act
        var (exitCode, output, error) = Run(changes, "--wording-paths");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.AreEqual(string.Empty, output);
        Assert.AreEqual(string.Empty, error);
    }

    /// <summary>
    /// Verifies that the wording-paths query recognizes a resource file in another letter case or with backslashes and
    /// writes it as read, with forward slashes, so the proof's case-insensitive exclusion matches it
    /// </summary>
    [TestMethod]
    public void ExecuteWritesAWordingPathInItsOwnSpellingWithForwardSlashes()
    {
        // Act
        var (exitCode, output, _) = Run("M\tSRC\\reihitsu.analyzer\\ANALYZERRESOURCES.resx\n", "--wording-paths");

        // Assert
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.AreEqual($"SRC/reihitsu.analyzer/ANALYZERRESOURCES.resx{Environment.NewLine}", output);
    }

    /// <summary>
    /// Verifies that the wording-paths query next to a lane or a proof verdict is a tool error rather than a verdict or
    /// a path list
    /// </summary>
    /// <param name="arguments">Command arguments, separated by spaces</param>
    [TestMethod]
    [DataRow("--wording-paths --lane docs")]
    [DataRow("--wording-paths --text-only pass")]
    [DataRow("--lane docs --text-only fail --wording-paths")]
    public void ExecuteRejectsTheWordingPathsQueryWithOtherArguments(string arguments)
    {
        // Act
        var (exitCode, output, error) = Run(WordingChanges, arguments.Split(' '));

        // Assert
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.AreEqual(string.Empty, output);
        Assert.Contains("lane-check: --wording-paths takes no other argument.", error);
        Assert.Contains("usage: lane-check", error);
    }

    /// <summary>
    /// Verifies that the wording-paths query with an unknown argument is a tool error naming that argument
    /// </summary>
    [TestMethod]
    public void ExecuteRejectsTheWordingPathsQueryWithAnUnknownArgument()
    {
        // Act
        var (exitCode, output, error) = Run(WordingChanges, "--wording-paths", "--extra");

        // Assert
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.AreEqual(string.Empty, output);
        Assert.Contains("lane-check: unexpected argument '--extra'.", error);
    }

    /// <summary>
    /// Verifies that the wording-paths query over a line that is not a name-status line is a tool error and writes no
    /// path, so the script never passes a partial exclusion list to the proof
    /// </summary>
    [TestMethod]
    public void ExecuteRejectsAMalformedChangeLineInTheWordingPathsQuery()
    {
        // Act
        var (exitCode, output, error) = Run("M\tsrc/Reihitsu.Analyzer/AnalyzerResources.resx\nR100\told.cs\tnew.cs\n", "--wording-paths");

        // Assert
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.AreEqual(string.Empty, output);
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