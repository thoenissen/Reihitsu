using System.IO;

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
    /// Verifies the category of each kind of path when the diff is not proven text-only; of the resource files, only
    /// the two that hold the diagnostic and code-fix texts are wording, and every other one keeps the category of
    /// where it lives
    /// </summary>
    /// <param name="path">Changed path</param>
    /// <param name="expected">Expected category</param>
    [TestMethod]
    [DataRow("sdd/changes/2026-10/introduce-sdd.md", PathCategory.ChangeDoc)]
    [DataRow("sdd/specs/formatter/pipeline.md", PathCategory.Text)]
    [DataRow("sdd/work/run/01-spec-author.md", PathCategory.Text)]
    [DataRow("squad/notes/README.md", PathCategory.Text)]
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
    [DataRow("reihitsu.json", PathCategory.Tooling)]
    [DataRow("samples/reihitsu.json", PathCategory.Unknown)]
    [DataRow("src/Reihitsu.Cli.Test/Fixtures/reihitsu.json", PathCategory.Test)]
    [DataRow("src/Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj", PathCategory.Tooling)]
    [DataRow("src/Reihitsu.Tooling/LaneClassifier.cs", PathCategory.Tooling)]
    [DataRow("src/Reihitsu.Tooling.Test/Unit/LaneClassifierTests.cs", PathCategory.Test)]
    [DataRow("src/Reihitsu.Formatter.Test/Unit/SampleTests.cs", PathCategory.Test)]
    [DataRow("src/Reihitsu.ArchitectureTests/SampleTests.cs", PathCategory.Test)]
    [DataRow("src/Reihitsu.Formatter/Pipeline/FormattingPipeline.cs", PathCategory.Production)]
    [DataRow("src/Reihitsu.Analyzer/AnalyzerResources.resx", PathCategory.Wording)]
    [DataRow("src/Reihitsu.Cli/CliResources.resx", PathCategory.Production)]
    [DataRow("src/Reihitsu.Formatter/FormatterResources.resx", PathCategory.Production)]
    [DataRow("src/Reihitsu.Analyzer/Rules/AnalyzerResources.resx", PathCategory.Production)]
    [DataRow("src/Reihitsu.Analyzer/CodeFixResources.resx", PathCategory.Production)]
    [DataRow("src/Reihitsu.Analyzer.CodeFixes/AnalyzerResources.resx", PathCategory.Production)]
    [DataRow("src/Reihitsu.Analyzer/AnalyzerResources.de.resx", PathCategory.Production)]
    [DataRow("src/Reihitsu.Analyzer.Test/AnalyzerResources.resx", PathCategory.Test)]
    [DataRow("src/Reihitsu.Tooling/ToolingResources.resx", PathCategory.Tooling)]
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
    /// Verifies that the root code of conduct and security policy are text, whether or not the diff was proven text-only
    /// </summary>
    /// <param name="path">Root Markdown file</param>
    /// <param name="isTextOnlyProven">Whether the whole diff was proven text-only</param>
    [TestMethod]
    [DataRow("CODE_OF_CONDUCT.md", false)]
    [DataRow("CODE_OF_CONDUCT.md", true)]
    [DataRow("SECURITY.md", false)]
    [DataRow("SECURITY.md", true)]
    public void ClassifyPathTreatsTheRootCommunityFilesAsText(string path, bool isTextOnlyProven)
    {
        // Act
        var category = LaneClassifier.ClassifyPath(path, isTextOnlyProven);

        // Assert
        Assert.AreEqual(PathCategory.Text, category);
    }

    /// <summary>
    /// Verifies that every Markdown file at the repository root is classified by some lane rule
    /// </summary>
    [TestMethod]
    public void ClassifyPathClassifiesEveryRootMarkdownFile()
    {
        // Arrange
        var rootMarkdownFiles = Directory.GetFiles(FindRepositoryRoot())
                                         .Select(Path.GetFileName)
                                         .Where(name => name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                                         .ToList();

        // Act
        var unknown = rootMarkdownFiles.Where(name => LaneClassifier.ClassifyPath(name, false) == PathCategory.Unknown)
                                       .ToList();

        // Assert
        Assert.IsNotEmpty(rootMarkdownFiles);
        Assert.IsEmpty(unknown, $"Root Markdown files no lane rule classifies: {string.Join(", ", unknown)}");
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
    [DataRow("src/Reihitsu.Analyzer/AnalyzerResources.resx", PathCategory.Wording)]
    [DataRow("src/Reihitsu.Cli/CliResources.resx", PathCategory.Production)]
    public void ClassifyPathTreatsCSharpAsTextOnlyWhenProven(string path, PathCategory expected)
    {
        // Act
        var category = LaneClassifier.ClassifyPath(path, true);

        // Assert
        Assert.AreEqual(expected, category);
    }

    /// <summary>
    /// Verifies that the two resource files holding the diagnostic and code-fix texts are wording, in any letter case
    /// and whether or not the diff was proven text-only
    /// </summary>
    /// <param name="path">Changed path</param>
    /// <param name="isTextOnlyProven">Whether the whole diff was proven text-only</param>
    [TestMethod]
    [DataRow("src/Reihitsu.Analyzer/AnalyzerResources.resx", false)]
    [DataRow("src/Reihitsu.Analyzer/AnalyzerResources.resx", true)]
    [DataRow("src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx", false)]
    [DataRow("src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx", true)]
    [DataRow("SRC/reihitsu.analyzer/analyzerresources.RESX", false)]
    [DataRow("src/reihitsu.analyzer.codefixes/CODEFIXRESOURCES.resx", true)]
    public void ClassifyPathTreatsTheDiagnosticAndCodeFixResourcesAsWording(string path, bool isTextOnlyProven)
    {
        // Act
        var category = LaneClassifier.ClassifyPath(path, isTextOnlyProven);

        // Assert
        Assert.AreEqual(PathCategory.Wording, category);
    }

    /// <summary>
    /// Verifies the narrowest lane each combination of categories needs under the ordered lane rules: production
    /// decides first, tooling next, then wording with or without test paths, then change doc, test, and text
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
    [DataRow(new[] { PathCategory.Tooling, PathCategory.Text, PathCategory.ChangeDoc }, Lane.Tooling)]
    [DataRow(new[] { PathCategory.Wording }, Lane.Wording)]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Text }, Lane.Wording)]
    [DataRow(new[] { PathCategory.Wording, PathCategory.ChangeDoc }, Lane.Wording)]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Text, PathCategory.ChangeDoc }, Lane.Wording)]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Test }, Lane.Tests)]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Text, PathCategory.Test }, Lane.Tests)]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Test, PathCategory.ChangeDoc }, Lane.BehaviorChange)]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Tooling, PathCategory.ChangeDoc }, Lane.BehaviorChange)]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Tooling, PathCategory.Test, PathCategory.ChangeDoc }, Lane.BehaviorChange)]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Production }, Lane.BugFix)]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Production, PathCategory.Test }, Lane.BugFix)]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Production, PathCategory.ChangeDoc }, Lane.BehaviorChange)]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Production, PathCategory.Tooling, PathCategory.ChangeDoc }, Lane.BehaviorChange)]
    public void TryGetRequiredLaneReturnsTheNarrowestAdmittingLane(PathCategory[] categories, Lane expected)
    {
        // Act
        var found = LaneClassifier.TryGetRequiredLane(categories, out var lane, out _);

        // Assert
        Assert.IsTrue(found);
        Assert.AreEqual(expected, lane);
    }

    /// <summary>
    /// Verifies that wording next to tooling without a change doc leaves no lane, because the tooling lane runs no
    /// spec author to write the texts and the behavior-change lane requires a change doc
    /// </summary>
    /// <param name="categories">Categories of the changed paths</param>
    [TestMethod]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Tooling })]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Tooling, PathCategory.Text })]
    [DataRow(new[] { PathCategory.Wording, PathCategory.Tooling, PathCategory.Test })]
    public void TryGetRequiredLaneFindsNoLaneForWordingAndToolingWithoutChangeDoc(PathCategory[] categories)
    {
        // Act
        var found = LaneClassifier.TryGetRequiredLane(categories, out _, out var reason);

        // Assert
        Assert.IsFalse(found);
        Assert.Contains("wording", reason);
        Assert.Contains("tooling", reason);
        Assert.Contains("without a change doc", reason);
    }

    /// <summary>
    /// Verifies that production next to wording and tooling without a change doc leaves no lane under the production
    /// rule, which decides before the wording rule
    /// </summary>
    [TestMethod]
    public void TryGetRequiredLaneFindsNoLaneForProductionWordingAndToolingWithoutChangeDoc()
    {
        // Act
        var found = LaneClassifier.TryGetRequiredLane([PathCategory.Wording, PathCategory.Production, PathCategory.Tooling], out _, out var reason);

        // Assert
        Assert.IsFalse(found);
        Assert.Contains("without a change doc", reason);
    }

    /// <summary>
    /// Verifies that an unclassified path next to a wording path leaves no lane, because the unknown-path rule decides
    /// first
    /// </summary>
    [TestMethod]
    public void TryGetRequiredLaneFindsNoLaneForAnUnknownPathNextToWording()
    {
        // Act
        var found = LaneClassifier.TryGetRequiredLane([PathCategory.Wording, PathCategory.Unknown], out _, out var reason);

        // Assert
        Assert.IsFalse(found);
        Assert.Contains("no lane rule classifies", reason);
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
    /// Verifies which claimed lanes cover which required lanes along the two widening orders; wording sits between
    /// docs and tests on the bug-fix order, so tooling does not cover it and it covers only docs and itself
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
    [DataRow(Lane.Wording, Lane.Wording, true)]
    [DataRow(Lane.Docs, Lane.Wording, false)]
    [DataRow(Lane.Tests, Lane.Wording, true)]
    [DataRow(Lane.BugFix, Lane.Wording, true)]
    [DataRow(Lane.BehaviorChange, Lane.Wording, true)]
    [DataRow(Lane.Tooling, Lane.Wording, false)]
    [DataRow(Lane.Wording, Lane.Docs, true)]
    [DataRow(Lane.Wording, Lane.Tests, false)]
    [DataRow(Lane.Wording, Lane.BugFix, false)]
    [DataRow(Lane.Wording, Lane.BehaviorChange, false)]
    [DataRow(Lane.Wording, Lane.Tooling, false)]
    public void CoversFollowsTheWideningOrders(Lane claimed, Lane required, bool expected)
    {
        // Act
        var covers = LaneClassifier.Covers(claimed, required);

        // Assert
        Assert.AreEqual(expected, covers);
    }

    /// <summary>
    /// Verifies that the wording lane is spelled <c>wording</c> as the scripts take it
    /// </summary>
    [TestMethod]
    public void WordingLaneIsNamedWording()
    {
        // Act
        var parsed = LaneClassifier.TryParseLane("wording", out var lane);
        var name = LaneClassifier.GetName(Lane.Wording);

        // Assert
        Assert.IsTrue(parsed);
        Assert.AreEqual(Lane.Wording, lane);
        Assert.AreEqual("wording", name);
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
    [DataRow(Lane.Wording)]
    public void LaneNamesRoundTrip(Lane lane)
    {
        // Act
        var parsed = LaneClassifier.TryParseLane(LaneClassifier.GetName(lane), out var result);

        // Assert
        Assert.IsTrue(parsed);
        Assert.AreEqual(lane, result);
    }

    /// <summary>
    /// Finds the repository root from the test output directory
    /// </summary>
    /// <returns>Absolute repository-root path</returns>
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Reihitsu.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Reihitsu repository root.");
    }

    #endregion // Methods
}