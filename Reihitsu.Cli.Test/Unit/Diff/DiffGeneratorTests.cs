using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Cli.Test.Unit.Diff;

/// <summary>
/// Tests for <see cref="DiffGenerator.Generate"/>.
/// </summary>
[TestClass]
public class DiffGeneratorTests
{
    #region Methods

    /// <summary>
    /// Verifies that identical content produces an empty diff.
    /// </summary>
    [TestMethod]
    public void GenerateIdenticalContentReturnsEmpty()
    {
        var content = "line1\nline2\nline3";

        var result = DiffGenerator.Generate("test.cs", content, content);

        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Verifies that a single changed line produces a correct unified diff with <c>-</c> and <c>+</c> lines.
    /// </summary>
    [TestMethod]
    public void GenerateSingleLineChangeProducesCorrectDiff()
    {
        var original = "line1\nline2\nline3";
        var formatted = "line1\nmodified\nline3";

        var result = DiffGenerator.Generate("test.cs", original, formatted);

        Assert.Contains("-line2", result);
        Assert.Contains("+modified", result);
        Assert.Contains(" line1", result);
        Assert.Contains(" line3", result);
    }

    /// <summary>
    /// Verifies that inserted lines appear as <c>+</c> lines in the diff output.
    /// </summary>
    [TestMethod]
    public void GenerateInsertedLinesProducesCorrectDiff()
    {
        var original = "line1\nline3";
        var formatted = "line1\nline2\nline3";

        var result = DiffGenerator.Generate("test.cs", original, formatted);

        Assert.Contains("+line2", result);

        var lines = result.Split('\n');
        var hasDeleteLine = lines.Any(l => l.StartsWith('-') && l.StartsWith("---", StringComparison.Ordinal) == false);

        Assert.IsFalse(hasDeleteLine, "Expected no delete lines in an insertion-only diff");
    }

    /// <summary>
    /// Verifies that deleted lines appear as <c>-</c> lines in the diff output.
    /// </summary>
    [TestMethod]
    public void GenerateDeletedLinesProducesCorrectDiff()
    {
        var original = "line1\nline2\nline3";
        var formatted = "line1\nline3";

        var result = DiffGenerator.Generate("test.cs", original, formatted);

        Assert.Contains("-line2", result);
    }

    /// <summary>
    /// Verifies that far-apart changes produce multiple separate hunk headers.
    /// </summary>
    [TestMethod]
    public void GenerateMultipleHunksProducesCorrectDiff()
    {
        var originalLines = new List<string>();
        var formattedLines = new List<string>();

        // First change at line 0
        originalLines.Add("old1");
        formattedLines.Add("new1");

        // 10 identical lines in between (more than 2 × ContextLines)
        for (var i = 1; i <= 10; i++)
        {
            originalLines.Add($"same{i}");
            formattedLines.Add($"same{i}");
        }

        // Second change at line 11
        originalLines.Add("old2");
        formattedLines.Add("new2");

        var original = string.Join("\n", originalLines);
        var formatted = string.Join("\n", formattedLines);

        var result = DiffGenerator.Generate("test.cs", original, formatted);

        var hunkCount = 0;
        var index = 0;

        while ((index = result.IndexOf("@@", index, StringComparison.Ordinal)) >= 0)
        {
            hunkCount++;

            index += 2;
        }

        // Each hunk header has two @@ markers, so hunkCount / 2 = number of hunks
        Assert.IsGreaterThanOrEqualTo(hunkCount / 2, 2, $"Expected at least 2 hunks but found {hunkCount / 2}");
    }

    /// <summary>
    /// Verifies that an empty original produces all <c>+</c> (insert) lines in the diff.
    /// </summary>
    [TestMethod]
    public void GenerateEmptyOriginalProducesAllInserts()
    {
        var original = string.Empty;
        var formatted = "line1\nline2";

        var result = DiffGenerator.Generate("test.cs", original, formatted);

        Assert.Contains("+line1", result);
        Assert.Contains("+line2", result);
    }

    /// <summary>
    /// Verifies that an empty formatted content produces all <c>-</c> (delete) lines in the diff.
    /// </summary>
    [TestMethod]
    public void GenerateEmptyFormattedProducesAllDeletes()
    {
        var original = "line1\nline2";
        var formatted = string.Empty;

        var result = DiffGenerator.Generate("test.cs", original, formatted);

        Assert.Contains("-line1", result);
        Assert.Contains("-line2", result);
    }

    /// <summary>
    /// Verifies that the diff header includes the file path with <c>--- a/</c> and <c>+++ b/</c> prefixes.
    /// </summary>
    [TestMethod]
    public void GenerateIncludesFilePathInHeader()
    {
        var original = "old";
        var formatted = "new";

        var result = DiffGenerator.Generate("src/MyFile.cs", original, formatted);

        Assert.Contains("--- a/src/MyFile.cs", result);
        Assert.Contains("+++ b/src/MyFile.cs", result);
    }

    /// <summary>
    /// Verifies that Windows-style <c>\r\n</c> line endings are handled correctly.
    /// </summary>
    [TestMethod]
    public void GenerateHandlesWindowsLineEndings()
    {
        var original = "line1\r\nline2\r\nline3";
        var formatted = "line1\r\nmodified\r\nline3";

        var result = DiffGenerator.Generate("test.cs", original, formatted);

        Assert.Contains("-line2", result);
        Assert.Contains("+modified", result);
    }

    #endregion // Methods
}