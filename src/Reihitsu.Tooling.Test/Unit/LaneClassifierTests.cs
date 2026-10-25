using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Tooling.Enumerations;

namespace Reihitsu.Tooling.Test.Unit;

/// <summary>
/// Tests for <see cref="LaneClassifier"/>
/// </summary>
[TestClass]
public sealed class LaneClassifierTests
{
    #region Methods

    /// <summary>
    /// Verifies the category of each kind of path when the diff is not proven text-only
    /// </summary>
    /// <param name="path">Changed path</param>
    /// <param name="expected">Expected category</param>
    [TestMethod]
    [DataRow("sdd/changes/2026-10/introduce-sdd.md", PathCategory.ChangeDoc)]
    [DataRow("sdd/specs/formatter/pipeline.md", PathCategory.Text)]
    [DataRow("sdd/work/run/01-spec-author.md", PathCategory.Text)]
    [DataRow("documentation/rules/RH3202.md", PathCategory.Text)]
    [DataRow("README.MD", PathCategory.Text)]
    [DataRow(".github/ISSUE_TEMPLATE/formatter_bug_report.yml", PathCategory.Text)]
    [DataRow(".github/PULL_REQUEST_TEMPLATE.md", PathCategory.Text)]
    [DataRow("src/Reihitsu.Analyzer.Package/README.MD", PathCategory.Text)]
    [DataRow(".github/workflows/build.yml", PathCategory.Tooling)]
    [DataRow("scripts/README.md", PathCategory.Tooling)]
    [DataRow(".claude/agents/auditor.md", PathCategory.Tooling)]
    [DataRow("squad/lead.md", PathCategory.Tooling)]
    [DataRow("AGENTS.md", PathCategory.Tooling)]
    [DataRow("CLAUDE.md", PathCategory.Tooling)]
    [DataRow("Directory.Build.props", PathCategory.Tooling)]
    [DataRow("Release.ruleset", PathCategory.Tooling)]
    [DataRow(".editorconfig", PathCategory.Tooling)]
    [DataRow("Reihitsu.sln", PathCategory.Tooling)]
    [DataRow("src/Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj", PathCategory.Tooling)]
    [DataRow("src/Reihitsu.Tooling/LaneClassifier.cs", PathCategory.Tooling)]
    [DataRow("src/Reihitsu.Tooling.Test/Unit/LaneClassifierTests.cs", PathCategory.Test)]
    [DataRow("src/Reihitsu.Formatter.Test/Unit/SampleTests.cs", PathCategory.Test)]
    [DataRow("src/Reihitsu.ArchitectureTests/SampleTests.cs", PathCategory.Test)]
    [DataRow("src/Reihitsu.Formatter/Pipeline/FormattingPipeline.cs", PathCategory.Production)]
    [DataRow("src/Reihitsu.Analyzer/AnalyzerResources.resx", PathCategory.Production)]
    [DataRow("notes.txt", PathCategory.Unknown)]
    public void ClassifyPathAssignsTheCategoryOfEachKindOfPath(string path, PathCategory expected)
    {
        // Act
        var category = LaneClassifier.ClassifyPath(path, false);

        // Assert
        Assert.AreEqual(expected, category);
    }

    /// <summary>
    /// Verifies that the repository's root contributing guide is text, whether or not the diff was proven text-only
    /// </summary>
    /// <param name="isTextOnlyProven">Whether the whole diff was proven text-only</param>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ClassifyPathTreatsTheRootContributingGuideAsText(bool isTextOnlyProven)
    {
        // Act
        var category = LaneClassifier.ClassifyPath("CONTRIBUTING.md", isTextOnlyProven);

        // Assert
        Assert.AreEqual(PathCategory.Text, category);
    }

    /// <summary>
    /// Verifies that a proven text-only diff turns production and test C# files into text, but leaves tooling and
    /// non-C# production files where they were
    /// </summary>
    /// <param name="path">Changed path</param>
    /// <param name="expected">Expected category</param>
    [TestMethod]
    [DataRow("src/Reihitsu.Formatter/Pipeline/FormattingPipeline.cs", PathCategory.Text)]
    [DataRow("src/Reihitsu.Formatter.Test/Unit/SampleTests.cs", PathCategory.Text)]
    [DataRow("src/Reihitsu.Tooling/LaneClassifier.cs", PathCategory.Tooling)]
    [DataRow("src/Reihitsu.Analyzer/AnalyzerResources.resx", PathCategory.Production)]
    public void ClassifyPathTreatsCSharpAsTextOnlyWhenProven(string path, PathCategory expected)
    {
        // Act
        var category = LaneClassifier.ClassifyPath(path, true);

        // Assert
        Assert.AreEqual(expected, category);
    }

    /// <summary>
    /// Verifies the narrowest lane each combination of categories needs
    /// </summary>
    /// <param name="categories">Categories of the changed paths</param>
    /// <param name="expected">Expected lane</param>
    [TestMethod]
    [DataRow(new PathCategory[0], Lane.Docs)]
    [DataRow(new[] { PathCategory.Text }, Lane.Docs)]
    [DataRow(new[] { PathCategory.Text, PathCategory.Test }, Lane.Tests)]
    [DataRow(new[] { PathCategory.Test, PathCategory.Production }, Lane.BugFix)]
    [DataRow(new[] { PathCategory.Production, PathCategory.ChangeDoc }, Lane.BehaviorChange)]
    [DataRow(new[] { PathCategory.Production, PathCategory.Tooling, PathCategory.ChangeDoc }, Lane.BehaviorChange)]
    [DataRow(new[] { PathCategory.Text, PathCategory.ChangeDoc }, Lane.BehaviorChange)]
    [DataRow(new[] { PathCategory.Tooling, PathCategory.Test }, Lane.Tooling)]
    [DataRow(new[] { PathCategory.Tooling, PathCategory.ChangeDoc }, Lane.Tooling)]
    public void TryGetRequiredLaneReturnsTheNarrowestAdmittingLane(PathCategory[] categories, Lane expected)
    {
        // Act
        var found = LaneClassifier.TryGetRequiredLane(categories, out var lane, out _);

        // Assert
        Assert.IsTrue(found);
        Assert.AreEqual(expected, lane);
    }

    /// <summary>
    /// Verifies that an unclassified path leaves no lane
    /// </summary>
    [TestMethod]
    public void TryGetRequiredLaneFindsNoLaneForAnUnknownPath()
    {
        // Act
        var found = LaneClassifier.TryGetRequiredLane([PathCategory.Text, PathCategory.Unknown], out _, out var reason);

        // Assert
        Assert.IsFalse(found);
        Assert.Contains("no lane rule classifies", reason);
    }

    /// <summary>
    /// Verifies that production and tooling paths without a change doc leave no lane, because the bug-fix lane does not
    /// admit tooling and the behavior-change lane requires a change doc
    /// </summary>
    [TestMethod]
    public void TryGetRequiredLaneFindsNoLaneForProductionAndToolingWithoutChangeDoc()
    {
        // Act
        var found = LaneClassifier.TryGetRequiredLane([PathCategory.Production, PathCategory.Tooling], out _, out var reason);

        // Assert
        Assert.IsFalse(found);
        Assert.Contains("without a change doc", reason);
    }

    /// <summary>
    /// Verifies which claimed lanes cover which required lanes along the two widening orders
    /// </summary>
    /// <param name="claimed">Claimed lane</param>
    /// <param name="required">Required lane</param>
    /// <param name="expected">Whether the claim covers the requirement</param>
    [TestMethod]
    [DataRow(Lane.Docs, Lane.Docs, true)]
    [DataRow(Lane.Docs, Lane.Tests, false)]
    [DataRow(Lane.Tests, Lane.Docs, true)]
    [DataRow(Lane.BugFix, Lane.Tests, true)]
    [DataRow(Lane.Tests, Lane.BugFix, false)]
    [DataRow(Lane.Tooling, Lane.Tests, true)]
    [DataRow(Lane.BugFix, Lane.Tooling, false)]
    [DataRow(Lane.Tooling, Lane.BugFix, false)]
    [DataRow(Lane.BugFix, Lane.BehaviorChange, false)]
    [DataRow(Lane.Tooling, Lane.BehaviorChange, false)]
    [DataRow(Lane.BehaviorChange, Lane.Tooling, true)]
    [DataRow(Lane.BehaviorChange, Lane.BugFix, true)]
    public void CoversFollowsTheWideningOrders(Lane claimed, Lane required, bool expected)
    {
        // Act
        var covers = LaneClassifier.Covers(claimed, required);

        // Assert
        Assert.AreEqual(expected, covers);
    }

    /// <summary>
    /// Verifies that every lane name parses back to the lane it names
    /// </summary>
    /// <param name="lane">Lane to round-trip</param>
    [TestMethod]
    [DataRow(Lane.Docs)]
    [DataRow(Lane.Tests)]
    [DataRow(Lane.BugFix)]
    [DataRow(Lane.BehaviorChange)]
    [DataRow(Lane.Tooling)]
    public void LaneNamesRoundTrip(Lane lane)
    {
        // Act
        var parsed = LaneClassifier.TryParseLane(LaneClassifier.GetName(lane), out var result);

        // Assert
        Assert.IsTrue(parsed);
        Assert.AreEqual(lane, result);
    }

    #endregion // Methods
}