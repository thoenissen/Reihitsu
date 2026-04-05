using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.Cleanup;

namespace Reihitsu.Formatter.Test.Unit.Rules.Cleanup;

/// <summary>
/// Tests for <see cref="TrailingTriviaCleanupRule"/>
/// </summary>
[TestClass]
public class TrailingTriviaCleanupRuleTests
{
    #region Methods

    /// <summary>
    /// Verifies that trailing whitespace before end-of-line is removed.
    /// </summary>
    [TestMethod]
    public void TrailingWhitespaceRemoved()
    {
        // Arrange
        const string input = "class C   \n{\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.DoesNotContain("C   \n", actual, "Trailing whitespace before end-of-line should be removed.");
        Assert.Contains("class C\n", actual, "Content should be preserved without trailing whitespace.");
    }

    /// <summary>
    /// Verifies that three or more consecutive blank lines are collapsed to a maximum of one blank line.
    /// </summary>
    [TestMethod]
    public void ConsecutiveBlankLinesCollapsedToOne()
    {
        // Arrange — 4 leading EOLs (3 blank lines) before content
        const string input = "\n\n\n\nclass C\n{\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert — should have at most 2 consecutive EOLs (1 blank line) within any trivia list
        var maxConsecutiveEols = CountMaxConsecutiveEndOfLines(actual);

        Assert.IsLessThanOrEqualTo(2, maxConsecutiveEols, $"Expected at most 2 consecutive EOLs but found {maxConsecutiveEols}.");
    }

    /// <summary>
    /// Verifies that a single blank line between statements is preserved.
    /// </summary>
    [TestMethod]
    public void SingleBlankLinePreserved()
    {
        // Arrange
        const string input = "class C\n{\n    int x;\n\n    int y;\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert — the blank line between `int x;` and `int y;` should still be present
        Assert.Contains("x;\n\n", actual, "Single blank line between statements should be preserved.");
    }

    /// <summary>
    /// Verifies that a file ending with a single newline has the trailing newline removed.
    /// </summary>
    [TestMethod]
    public void FileWithSingleTrailingNewlineStripsIt()
    {
        // Arrange — file already ends with one newline
        const string input = "class C\n{\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert — trailing newline should be stripped
        Assert.AreEqual("class C\n{\n}", actual, "File should not end with a trailing newline.");
    }

    /// <summary>
    /// Verifies that multiple trailing newlines at end of file are all removed.
    /// </summary>
    [TestMethod]
    public void FileWithMultipleTrailingNewlinesStripsAll()
    {
        // Arrange — file ends with excessive newlines
        const string input = "class C\n{\n}\n\n\n\n\n";

        // Act
        var actual = ApplyRule(input);

        // Assert — all trailing newlines should be stripped
        Assert.AreEqual("class C\n{\n}", actual, "File should not end with trailing newlines.");
    }

    /// <summary>
    /// Verifies that a file without a trailing newline remains unchanged.
    /// </summary>
    [TestMethod]
    public void FileWithoutTrailingNewlineRemainsUnchanged()
    {
        // Arrange — file has no trailing newline
        const string input = "class C\n{\n}";

        // Act
        var actual = ApplyRule(input);

        // Assert — file should remain without a trailing newline
        Assert.AreEqual("class C\n{\n}", actual, "File should remain without a trailing newline.");
    }

    /// <summary>
    /// Verifies that whitespace between end-of-line trivia is removed.
    /// </summary>
    [TestMethod]
    public void WhitespaceBetweenEOLsRemoved()
    {
        // Arrange — whitespace between EOLs (blank line with spaces)
        const string input = "class C\n{\n    int x;\n   \n    int y;\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert — the whitespace-only line should be cleaned to just EOL
        Assert.DoesNotContain("\n   \n", actual, "Whitespace between end-of-lines should be removed.");
    }

    /// <summary>
    /// Verifies that comments in the source code are preserved.
    /// </summary>
    [TestMethod]
    public void CommentsPreserved()
    {
        // Arrange
        const string input = "// This is a comment\nclass C\n{\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.Contains("// This is a comment", actual, "Comments should be preserved.");
    }

    /// <summary>
    /// Verifies that an empty file remains empty.
    /// </summary>
    [TestMethod]
    public void EmptyFileRemainsEmpty()
    {
        // Arrange
        var input = string.Empty;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(string.Empty, actual, "Empty file should remain empty.");
    }

    /// <summary>
    /// Verifies that a UTF-8 BOM at the start of a file is preserved.
    /// </summary>
    [TestMethod]
    public void BomPreserved()
    {
        // Arrange
        const string input = "\uFEFFclass C\n{\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.StartsWith("\uFEFF", actual, "UTF-8 BOM should be preserved at the start of the file.");
        Assert.Contains("class C", actual, "Content should be preserved.");
    }

    /// <summary>
    /// Verifies that the <see cref="TrailingTriviaCleanupRule.Phase"/> property returns <see cref="FormattingPhase.Cleanup"/>.
    /// </summary>
    [TestMethod]
    public void PhaseReturnsCleanup()
    {
        // Arrange
        var context = new FormattingContext("\n");
        var rule = new TrailingTriviaCleanupRule(context, CancellationToken.None);

        // Act
        var phase = rule.Phase;

        // Assert
        Assert.AreEqual(FormattingPhase.Cleanup, phase);
    }

    /// <summary>
    /// Applies the <see cref="TrailingTriviaCleanupRule"/> to the given input source code.
    /// </summary>
    /// <param name="input">The source code to format.</param>
    /// <returns>The formatted source code.</returns>
    private static string ApplyRule(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input);
        var context = new FormattingContext("\n");
        var rule = new TrailingTriviaCleanupRule(context, CancellationToken.None);
        var result = rule.Apply(tree.GetRoot());

        return result.ToFullString();
    }

    /// <summary>
    /// Counts the maximum number of consecutive end-of-line characters in the given text.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <returns>The maximum number of consecutive newline characters.</returns>
    private static int CountMaxConsecutiveEndOfLines(string text)
    {
        var max = 0;
        var current = 0;

        foreach (var c in text)
        {
            if (c == '\n')
            {
                current++;

                if (current > max)
                {
                    max = current;
                }
            }
            else
            {
                current = 0;
            }
        }

        return max;
    }

    #endregion // Methods
}