using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.Cleanup;
using Reihitsu.Formatter.Test.Unit.Rules.Base;

namespace Reihitsu.Formatter.Test.Unit.Rules.Cleanup;

/// <summary>
/// Tests for <see cref="TrailingTriviaCleanupRule"/>
/// </summary>
[TestClass]
public class TrailingTriviaCleanupRuleTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that trailing whitespace before end-of-line is removed.
    /// </summary>
    [TestMethod]
    public void TrailingWhitespaceRemoved()
    {
        // Arrange
        var input = """
            class C   
            {
            }

            """;
        var expected = """
            class C
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that three or more consecutive blank lines are collapsed to a maximum of one blank line.
    /// </summary>
    [TestMethod]
    public void ConsecutiveBlankLinesCollapsedToOne()
    {
        // Arrange — 4 leading EOLs (3 blank lines) before content
        var input = """



            class C
            {
            }

            """;
        var expected = """

            class C
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single blank line between statements is preserved.
    /// </summary>
    [TestMethod]
    public void SingleBlankLinePreserved()
    {
        // Arrange
        var input = """
            class C
            {
                int x;

                int y;
            }

            """;
        var expected = """
            class C
            {
                int x;

                int y;
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a file ending with a single newline has the trailing newline removed.
    /// </summary>
    [TestMethod]
    public void FileWithSingleTrailingNewlineStripsIt()
    {
        // Arrange — file already ends with one newline
        var input = """
            class C
            {
            }

            """;
        var expected = """
            class C
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that multiple trailing newlines at end of file are all removed.
    /// </summary>
    [TestMethod]
    public void FileWithMultipleTrailingNewlinesStripsAll()
    {
        // Arrange — file ends with excessive newlines
        var input = """
            class C
            {
            }





            """;
        var expected = """
            class C
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a file without a trailing newline remains unchanged.
    /// </summary>
    [TestMethod]
    public void FileWithoutTrailingNewlineRemainsUnchanged()
    {
        // Arrange — file has no trailing newline
        var input = """
            class C
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that whitespace between end-of-line trivia is removed.
    /// </summary>
    [TestMethod]
    public void WhitespaceBetweenEOLsRemoved()
    {
        // Arrange — whitespace between EOLs (blank line with spaces)
        var input = """
            class C
            {
                int x;
               
                int y;
            }

            """;
        var expected = """
            class C
            {
                int x;

                int y;
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that comments in the source code are preserved.
    /// </summary>
    [TestMethod]
    public void CommentsPreserved()
    {
        // Arrange
        var input = """
            // This is a comment
            class C
            {
            }

            """;
        var expected = """
            // This is a comment
            class C
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty file remains empty.
    /// </summary>
    [TestMethod]
    public void EmptyFileRemainsEmpty()
    {
        // Arrange
        var input = string.Empty;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a UTF-8 BOM at the start of a file is preserved.
    /// </summary>
    [TestMethod]
    public void BomPreserved()
    {
        // Arrange
        const string bom = "\uFEFF";
        var input = $$"""
            {{bom}}class C
            {
            }

            """;
        var expected = $$"""
            {{bom}}class C
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
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

    #endregion // Methods
}