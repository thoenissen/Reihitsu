using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Cli.Test.Unit.Diff;

/// <summary>
/// Tests for <see cref="DiffGenerator.Generate"/>
/// </summary>
[TestClass]
public class DiffGeneratorTests
{
    #region Constants

    /// <summary>
    /// Authored text matching what used to be <see cref="DiffGenerator"/>'s internal no-newline sentinel before this
    /// text-encoded representation was replaced by an out-of-band termination flag
    /// </summary>
    private const string SentinelLikeAuthoredText = "￼NO-NEWLINE-AT-END-OF-FILE￼";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that identical content produces an empty diff
    /// </summary>
    [TestMethod]
    public void GenerateIdenticalContentReturnsEmpty()
    {
        var content = "line1\nline2\nline3";

        var result = DiffGenerator.Generate("test.cs", content, content);

        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Verifies that a single changed line produces a correct unified diff with <c>-</c> and <c>+</c> lines
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
    /// Verifies that inserted lines appear as <c>+</c> lines in the diff output
    /// </summary>
    [TestMethod]
    public void GenerateInsertedLinesProducesCorrectDiff()
    {
        var original = "line1\nline3";
        var formatted = "line1\nline2\nline3";

        var result = DiffGenerator.Generate("test.cs", original, formatted);

        Assert.Contains("+line2", result);

        var lines = result.Split('\n');
        var hasDeleteLine = lines.Any(line => line.StartsWith('-') && line.StartsWith("---", StringComparison.Ordinal) == false);

        Assert.IsFalse(hasDeleteLine, "Expected no delete lines in an insertion-only diff");
    }

    /// <summary>
    /// Verifies that deleted lines appear as <c>-</c> lines in the diff output
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
    /// Verifies that far-apart changes produce multiple separate hunk headers
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
        for (var lineIndex = 1; lineIndex <= 10; lineIndex++)
        {
            originalLines.Add($"same{lineIndex}");
            formattedLines.Add($"same{lineIndex}");
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
    /// Verifies that an empty original produces all <c>+</c> (insert) lines in the diff
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
    /// Verifies that an empty formatted content produces all <c>-</c> (delete) lines in the diff
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
    /// Verifies that the diff header includes the file path with <c>--- a/</c> and <c>+++ b/</c> prefixes
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
    /// Verifies that a localized change in a large file produces a single small hunk with bounded memory rather than allocating a full O(n×m) table
    /// </summary>
    [TestMethod]
    public void GenerateLargeFileWithLocalizedChangeProducesSingleHunk()
    {
        const int lineCount = 10_000;
        const int changedLine = 5_000;

        var originalLines = new List<string>(lineCount);
        var formattedLines = new List<string>(lineCount);

        for (var index = 0; index < lineCount; index++)
        {
            originalLines.Add($"line{index}");
            formattedLines.Add(index == changedLine ? "    changed" : $"line{index}");
        }

        var original = string.Join("\n", originalLines);
        var formatted = string.Join("\n", formattedLines);

        var result = DiffGenerator.Generate("test.cs", original, formatted);

        Assert.Contains($"-line{changedLine}", result);
        Assert.Contains("+    changed", result);

        var hunkHeaderCount = result.Split('\n').Count(line => line.StartsWith("@@", StringComparison.Ordinal));

        Assert.AreEqual(1, hunkHeaderCount);
    }

    /// <summary>
    /// Verifies that Windows-style <c>\r\n</c> line endings are handled correctly
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

    /// <summary>
    /// Verifies that lone carriage-return separators are treated as line breaks
    /// </summary>
    [TestMethod]
    public void GenerateHandlesLoneCarriageReturnLineEndings()
    {
        var original = "a\rb\rc";
        var formatted = "a\rB\rc";

        var result = DiffGenerator.Generate("test.cs", original, formatted);

        Assert.Contains("-b", result);
        Assert.Contains("+B", result);
    }

    /// <summary>
    /// Verifies that inserting into empty original content uses the zero-count "line before" range convention
    /// </summary>
    [TestMethod]
    public void GenerateInsertIntoEmptyOriginalUsesZeroCountRange()
    {
        var result = DiffGenerator.Generate("test.cs", string.Empty, "line1\nline2");

        Assert.Contains("@@ -0,0 +1,2 @@", result);
    }

    /// <summary>
    /// Verifies that deleting all original content uses the zero-count "line before" range convention
    /// </summary>
    [TestMethod]
    public void GenerateDeleteAllContentUsesZeroCountRange()
    {
        var result = DiffGenerator.Generate("test.cs", "line1\nline2", string.Empty);

        Assert.Contains("@@ -1,2 +0,0 @@", result);
    }

    /// <summary>
    /// Verifies that a missing trailing newline produces the "no newline at end of file" marker
    /// </summary>
    [TestMethod]
    public void GenerateMissingTrailingNewlineEmitsNoNewlineMarker()
    {
        var result = DiffGenerator.Generate("test.cs", "a\nb", "a\nB");

        Assert.Contains("\\ No newline at end of file", result);
    }

    /// <summary>
    /// Verifies that content ending with a trailing newline does not produce the "no newline at end of file" marker
    /// </summary>
    [TestMethod]
    public void GenerateTrailingNewlineDoesNotEmitNoNewlineMarker()
    {
        var result = DiffGenerator.Generate("test.cs", "a\nb\n", "a\nB\n");

        Assert.DoesNotContain("No newline at end of file", result);
    }

    /// <summary>
    /// Verifies that a last common line that is unterminated on only one side is rendered as a delete and an insert
    /// rather than as a context line carrying a mid-hunk no-newline marker (which <c>git apply</c> rejects)
    /// </summary>
    [TestMethod]
    public void GenerateLastCommonLineUnterminatedOnOneSideIsNotContext()
    {
        var result = DiffGenerator.Generate("test.cs", "a\nb", "a\nb\nc");

        var lines = result.Split(Environment.NewLine);

        Assert.IsFalse(lines.Contains(" b"), "The differing line must not be rendered as a context line.");
        Assert.IsTrue(lines.Contains("-b"));
        Assert.IsTrue(lines.Contains("+b"));
        Assert.IsTrue(lines.Contains("+c"));
    }

    /// <summary>
    /// Verifies that an unchanged, newline-terminated final line whose authored text happens to equal the internal
    /// no-newline sentinel renders verbatim, without truncating the comment or emitting a false no-newline marker
    /// </summary>
    [TestMethod]
    public void GenerateRendersTerminatedFinalLineContainingSentinelLikeTextVerbatim()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs",
                                            $"class A\n{{\n}}\n// {SentinelLikeAuthoredText}\n",
                                            $"class B\n{{\n}}\n// {SentinelLikeAuthoredText}\n");

        // Assert
        Assert.AreEqual(ExpectedDiff("--- a/test.cs",
                                     "+++ b/test.cs",
                                     "@@ -1,4 +1,4 @@",
                                     "-class A",
                                     "+class B",
                                     " {",
                                     " }",
                                     $" // {SentinelLikeAuthoredText}"),
                        result);
    }

    /// <summary>
    /// Verifies that an unchanged, CRLF-terminated final line whose authored text happens to equal the internal
    /// no-newline sentinel renders verbatim, without truncating the comment or emitting a false no-newline marker
    /// </summary>
    [TestMethod]
    public void GenerateRendersTerminatedFinalLineContainingSentinelLikeTextVerbatimCrlf()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs",
                                            $"class A\r\n{{\r\n}}\r\n// {SentinelLikeAuthoredText}\r\n",
                                            $"class B\r\n{{\r\n}}\r\n// {SentinelLikeAuthoredText}\r\n");

        // Assert
        Assert.AreEqual(ExpectedDiff("--- a/test.cs",
                                     "+++ b/test.cs",
                                     "@@ -1,4 +1,4 @@",
                                     "-class A",
                                     "+class B",
                                     " {",
                                     " }",
                                     $" // {SentinelLikeAuthoredText}"),
                        result);
    }

    /// <summary>
    /// Verifies that a non-final line whose authored text equals the internal no-newline sentinel renders verbatim
    /// and never emits the no-newline marker, which only a genuinely unterminated final line may carry
    /// </summary>
    [TestMethod]
    public void GenerateRendersNonFinalLineContainingSentinelLikeTextVerbatim()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs",
                                            $"a\n// {SentinelLikeAuthoredText}\nb\n",
                                            $"a\n// {SentinelLikeAuthoredText}\nc\n");

        // Assert
        Assert.AreEqual(ExpectedDiff("--- a/test.cs",
                                     "+++ b/test.cs",
                                     "@@ -1,3 +1,3 @@",
                                     " a",
                                     $" // {SentinelLikeAuthoredText}",
                                     "-b",
                                     "+c"),
                        result);
    }

    /// <summary>
    /// Verifies that an unterminated line and a terminated line whose authored text equals the internal no-newline
    /// sentinel never compare equal, so a real change is rendered as a change rather than collapsed into one
    /// context line. The comparison key has to carry termination state, not only line text
    /// </summary>
    [TestMethod]
    public void GenerateDistinguishesUnterminatedContentFromTerminatedContentEndingInSentinelLikeText()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs", "}", $"}}{SentinelLikeAuthoredText}\n");

        // Assert
        Assert.AreEqual(ExpectedDiff("--- a/test.cs",
                                     "+++ b/test.cs",
                                     "@@ -1,1 +1,1 @@",
                                     "-}",
                                     "\\ No newline at end of file",
                                     $"+}}{SentinelLikeAuthoredText}"),
                        result);
    }

    /// <summary>
    /// Verifies that a terminated final line whose authored text equals the internal no-newline sentinel renders
    /// verbatim under lone-carriage-return line endings, not only under LF and CRLF
    /// </summary>
    [TestMethod]
    public void GenerateRendersTerminatedFinalLineContainingSentinelLikeTextVerbatimLoneCr()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs",
                                            $"class A\r{{\r}}\r// {SentinelLikeAuthoredText}\r",
                                            $"class B\r{{\r}}\r// {SentinelLikeAuthoredText}\r");

        // Assert
        Assert.AreEqual(ExpectedDiff("--- a/test.cs",
                                     "+++ b/test.cs",
                                     "@@ -1,4 +1,4 @@",
                                     "-class A",
                                     "+class B",
                                     " {",
                                     " }",
                                     $" // {SentinelLikeAuthoredText}"),
                        result);
    }

    /// <summary>
    /// Verifies that removing a non-final line whose authored text equals the internal no-newline sentinel renders
    /// the line verbatim as a deletion, without truncating it or emitting a false no-newline marker
    /// </summary>
    [TestMethod]
    public void GenerateRendersDeletedNonFinalLineContainingSentinelLikeTextVerbatim()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs", $"a\n// {SentinelLikeAuthoredText}\nb\n", "a\nb\n");

        // Assert
        Assert.Contains(Line($"-// {SentinelLikeAuthoredText}"), result);
        Assert.DoesNotContain("\\ No newline at end of file", result);
    }

    /// <summary>
    /// Verifies that inserting a non-final line whose authored text equals the internal no-newline sentinel renders
    /// the line verbatim as an insertion, without truncating it or emitting a false no-newline marker
    /// </summary>
    [TestMethod]
    public void GenerateRendersInsertedNonFinalLineContainingSentinelLikeTextVerbatim()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs", "a\nb\n", $"a\n// {SentinelLikeAuthoredText}\nb\n");

        // Assert
        Assert.Contains(Line($"+// {SentinelLikeAuthoredText}"), result);
        Assert.DoesNotContain("\\ No newline at end of file", result);
    }

    /// <summary>
    /// Verifies that two adjacent non-final lines whose authored text equals the internal no-newline sentinel both
    /// render verbatim as context, without truncation or a false no-newline marker
    /// </summary>
    [TestMethod]
    public void GenerateRendersAdjacentSentinelLikeContextLinesVerbatim()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs",
                                            $"a\n// {SentinelLikeAuthoredText}\n// {SentinelLikeAuthoredText}\nb\n",
                                            $"a\n// {SentinelLikeAuthoredText}\n// {SentinelLikeAuthoredText}\nc\n");

        // Assert
        var contextLineCount = result.Split(Environment.NewLine).Count(line => line == $" // {SentinelLikeAuthoredText}");

        Assert.AreEqual(2, contextLineCount);
        Assert.Contains(Line("-b"), result);
        Assert.Contains(Line("+c"), result);
        Assert.DoesNotContain("\\ No newline at end of file", result);
    }

    /// <summary>
    /// Verifies that a multi-line unterminated file and a multi-line terminated file whose final line's authored
    /// text equals the internal no-newline sentinel never compare equal at the common-prefix stage
    /// </summary>
    [TestMethod]
    public void GenerateDistinguishesUnterminatedContentFromTerminatedSentinelLikeContentAtCommonPrefix()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs", "a\nb\n}", $"a\nb\n}}{SentinelLikeAuthoredText}\n");

        // Assert
        Assert.AreNotEqual(string.Empty, result);
        Assert.Contains(Line("-}"), result);
        Assert.Contains(Line("\\ No newline at end of file"), result);
        Assert.Contains(Line($"+}}{SentinelLikeAuthoredText}"), result);
    }

    /// <summary>
    /// Verifies that an unterminated line and a terminated line whose authored text equals the internal no-newline
    /// sentinel never compare equal at the common-suffix stage, even when an earlier line also differs
    /// </summary>
    [TestMethod]
    public void GenerateDistinguishesUnterminatedContentFromTerminatedSentinelLikeContentAtCommonSuffix()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs", "A\n}", $"B\n}}{SentinelLikeAuthoredText}\n");

        // Assert
        Assert.Contains(Line("-A"), result);
        Assert.Contains(Line("-}"), result);
        Assert.Contains(Line("\\ No newline at end of file"), result);
        Assert.Contains(Line("+B"), result);
        Assert.Contains(Line($"+}}{SentinelLikeAuthoredText}"), result);
        Assert.DoesNotContain($" }}{SentinelLikeAuthoredText}", result);
    }

    /// <summary>
    /// Verifies that an unterminated line and a terminated line whose authored text equals the internal no-newline
    /// sentinel never compare equal inside the LCS-backtracked changed region
    /// </summary>
    [TestMethod]
    public void GenerateDistinguishesUnterminatedContentFromTerminatedSentinelLikeContentInLcsRegion()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs", "p\n}", $"q\n}}{SentinelLikeAuthoredText}\nr\n");

        // Assert
        Assert.Contains(Line("-}"), result);
        Assert.Contains(Line("\\ No newline at end of file"), result);
        Assert.Contains(Line($"+}}{SentinelLikeAuthoredText}"), result);
        Assert.DoesNotContain($" }}{SentinelLikeAuthoredText}", result);
    }

    /// <summary>
    /// Verifies that an unterminated final line whose authored text already ends with the internal no-newline
    /// sentinel still renders with the full authored text and exactly one no-newline marker
    /// </summary>
    [TestMethod]
    public void GenerateRendersUnterminatedFinalLineAlreadyEndingInSentinelLikeTextVerbatim()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs", $"x\n}}{SentinelLikeAuthoredText}", $"y\n}}{SentinelLikeAuthoredText}");

        // Assert
        Assert.AreEqual(ExpectedDiff("--- a/test.cs",
                                     "+++ b/test.cs",
                                     "@@ -1,2 +1,2 @@",
                                     "-x",
                                     "+y",
                                     $" }}{SentinelLikeAuthoredText}",
                                     "\\ No newline at end of file"),
                        result);
    }

    /// <summary>
    /// Verifies that authored text containing the sentinel text away from the end of the line never renders as
    /// unterminated, since only a line ending with the exact sentinel text is eligible for misclassification
    /// </summary>
    [TestMethod]
    public void GenerateDoesNotTreatSentinelLikeTextInMiddleOfLineAsUnterminated()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs", $"a\n{SentinelLikeAuthoredText} tail\nb\n", $"a\n{SentinelLikeAuthoredText} tail\nc\n");

        // Assert
        Assert.Contains(Line($" {SentinelLikeAuthoredText} tail"), result);
        Assert.DoesNotContain("\\ No newline at end of file", result);
    }

    /// <summary>
    /// Verifies that content differing only in line endings still produces an empty diff when a line's authored
    /// text equals the internal no-newline sentinel, so line-ending-only changes stay a no-op
    /// </summary>
    [TestMethod]
    public void GenerateLineEndingOnlyChangeWithSentinelLikeLineStaysEmptyDiff()
    {
        // Act
        var result = DiffGenerator.Generate("test.cs", $"a\r\n// {SentinelLikeAuthoredText}\r\nb\r\n", $"a\n// {SentinelLikeAuthoredText}\nb\n");

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Builds the expected output of a rendered diff line, using the platform's own line separator so assertions
    /// stay correct regardless of whether <see cref="Environment.NewLine"/> is LF or CRLF
    /// </summary>
    /// <param name="text">The rendered line's text, without a trailing separator</param>
    /// <returns><paramref name="text"/> followed by <see cref="Environment.NewLine"/></returns>
    private static string Line(string text)
    {
        return text + Environment.NewLine;
    }

    /// <summary>
    /// Builds the expected output of a complete rendered diff from its individual lines, using the platform's own
    /// line separator so assertions stay correct regardless of whether <see cref="Environment.NewLine"/> is LF or CRLF
    /// </summary>
    /// <param name="lines">The rendered lines, each without a trailing separator</param>
    /// <returns>The concatenation of every line, each followed by <see cref="Environment.NewLine"/></returns>
    private static string ExpectedDiff(params string[] lines)
    {
        return string.Concat(lines.Select(Line));
    }

    #endregion // Methods
}