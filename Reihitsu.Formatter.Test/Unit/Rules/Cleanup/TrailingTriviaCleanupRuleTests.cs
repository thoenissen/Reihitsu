using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.Cleanup;
using Reihitsu.Formatter.Test;

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
        var input = Lf("""
            class C   
            {
            }

            """);
        var expected = Lf("""
            class C
            {
            }
            """);

        // Act
        var actual = ApplyRule(input);

        // Assert
        AssertNormalized(expected, actual, "Trailing whitespace before end-of-line should be removed.");
    }

    /// <summary>
    /// Verifies that three or more consecutive blank lines are collapsed to a maximum of one blank line.
    /// </summary>
    [TestMethod]
    public void ConsecutiveBlankLinesCollapsedToOne()
    {
        // Arrange — 4 leading EOLs (3 blank lines) before content
        var input = Lf("""



            class C
            {
            }

            """);
        var expected = Lf("""

            class C
            {
            }
            """);

        // Act
        var actual = ApplyRule(input);

        // Assert
        AssertNormalized(expected, actual, "Consecutive blank lines should be collapsed to a single blank line.");
    }

    /// <summary>
    /// Verifies that a single blank line between statements is preserved.
    /// </summary>
    [TestMethod]
    public void SingleBlankLinePreserved()
    {
        // Arrange
        var input = Lf("""
            class C
            {
                int x;

                int y;
            }

            """);
        var expected = Lf("""
            class C
            {
                int x;

                int y;
            }
            """);

        // Act
        var actual = ApplyRule(input);

        // Assert
        AssertNormalized(expected, actual, "Single blank line between statements should be preserved.");
    }

    /// <summary>
    /// Verifies that a file ending with a single newline has the trailing newline removed.
    /// </summary>
    [TestMethod]
    public void FileWithSingleTrailingNewlineStripsIt()
    {
        // Arrange — file already ends with one newline
        var input = Lf("""
            class C
            {
            }

            """);
        var expected = Lf("""
            class C
            {
            }
            """);

        // Act
        var actual = ApplyRule(input);

        // Assert — trailing newline should be stripped
        AssertNormalized(expected, actual, "File should not end with a trailing newline.");
    }

    /// <summary>
    /// Verifies that multiple trailing newlines at end of file are all removed.
    /// </summary>
    [TestMethod]
    public void FileWithMultipleTrailingNewlinesStripsAll()
    {
        // Arrange — file ends with excessive newlines
        var input = Lf("""
            class C
            {
            }





            """);
        var expected = Lf("""
            class C
            {
            }
            """);

        // Act
        var actual = ApplyRule(input);

        // Assert — all trailing newlines should be stripped
        AssertNormalized(expected, actual, "File should not end with trailing newlines.");
    }

    /// <summary>
    /// Verifies that a file without a trailing newline remains unchanged.
    /// </summary>
    [TestMethod]
    public void FileWithoutTrailingNewlineRemainsUnchanged()
    {
        // Arrange — file has no trailing newline
        var input = Lf("""
            class C
            {
            }
            """);

        // Act
        var actual = ApplyRule(input);

        // Assert — file should remain without a trailing newline
        AssertNormalized(input, actual, "File should remain without a trailing newline.");
    }

    /// <summary>
    /// Verifies that whitespace between end-of-line trivia is removed.
    /// </summary>
    [TestMethod]
    public void WhitespaceBetweenEOLsRemoved()
    {
        // Arrange — whitespace between EOLs (blank line with spaces)
        var input = Lf("""
            class C
            {
                int x;
               
                int y;
            }

            """);
        var expected = Lf("""
            class C
            {
                int x;

                int y;
            }
            """);

        // Act
        var actual = ApplyRule(input);

        // Assert
        AssertNormalized(expected, actual, "Whitespace between end-of-lines should be removed.");
    }

    /// <summary>
    /// Verifies that comments in the source code are preserved.
    /// </summary>
    [TestMethod]
    public void CommentsPreserved()
    {
        // Arrange
        var input = Lf("""
            // This is a comment
            class C
            {
            }

            """);
        var expected = Lf("""
            // This is a comment
            class C
            {
            }
            """);

        // Act
        var actual = ApplyRule(input);

        // Assert
        AssertNormalized(expected, actual, "Comments should be preserved.");
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
        AssertNormalized(string.Empty, actual, "Empty file should remain empty.");
    }

    /// <summary>
    /// Verifies that a UTF-8 BOM at the start of a file is preserved.
    /// </summary>
    [TestMethod]
    public void BomPreserved()
    {
        // Arrange
        const string bom = "\uFEFF";
        var input = Lf($$"""
            {{bom}}class C
            {
            }

            """);
        var expected = Lf($$"""
            {{bom}}class C
            {
            }
            """);

        // Act
        var actual = ApplyRule(input);

        // Assert
        AssertNormalized(expected, actual, "UTF-8 BOM and content should be preserved.");
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
    /// Normalizes line endings in the provided text to LF.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <returns>The text with LF line endings.</returns>
    private static string Lf(string text)
    {
        return text.Replace("\r\n", "\n");
    }

    #endregion // Methods
}