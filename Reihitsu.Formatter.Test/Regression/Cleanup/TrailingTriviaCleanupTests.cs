using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Cleanup;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class TrailingTriviaCleanupTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that trailing whitespace before end-of-line is removed.
    /// </summary>
    [TestMethod]
    public void TrailingWhitespaceRemoved()
    {
        // Arrange
        const string input = """
                             class C   
                             {
                             }

                             """;
        const string expected = """
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
        const string input = """



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
        const string input = """
                             class C
                             {
                                 int x;

                                 int y;
                             }

                             """;
        const string expected = """
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
        const string input = """
                             class C
                             {
                             }

                             """;
        const string expected = """
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
        const string input = """
                             class C
                             {
                             }





                             """;
        const string expected = """
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
        const string input = """
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
        const string input = """
                             class C
                             {
                                 int x;
                                
                                 int y;
                             }

                             """;
        const string expected = """
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
        const string input = """
                             // This is a comment
                             class C
                             {
                             }

                             """;
        const string expected = """
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
        const string input = "";

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
        const string input = $$"""
                               {{bom}}class C
                               {
                               }

                               """;
        const string expected = $$"""
                                  {{bom}}class C
                                  {
                                  }
                                  """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}