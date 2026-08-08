using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for issue #610: a comment that trails the closing bracket of an indexer argument
/// list sits outside the interior the single-line collapse rewrites, so it must not block that
/// collapse. Trivia the collapse actually crosses - between the brackets - remains a join barrier
/// </summary>
[TestClass]
public class BracketedArgumentTrailingCommentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a block comment trailing the closing bracket does not block the collapse of a
    /// multiline indexer argument list
    /// </summary>
    [TestMethod]
    public void BlockCommentAfterClosingBracketDoesNotBlockTheCollapse()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int Get(int[] a)
                                 {
                                     return a[
                                         0] /* note */ + 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int Get(int[] a)
                                    {
                                        return a[0] /* note */ + 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line comment trailing the closing bracket does not block the collapse.
    /// The comment runs to the end of the line, so it is the shape most easily mistaken for interior
    /// trivia
    /// </summary>
    [TestMethod]
    public void LineCommentAfterClosingBracketDoesNotBlockTheCollapse()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int Get(int[] a)
                                 {
                                     return a[
                                         0] // note
                                            + 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int Get(int[] a)
                                    {
                                        return a[0] // note
                                               + 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment trailing the closing bracket does not block the collapse of a
    /// multi-argument indexer list, where the collapse additionally crosses the separator gaps
    /// </summary>
    [TestMethod]
    public void CommentAfterClosingBracketDoesNotBlockTheMultiArgumentCollapse()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int Get(int[,] a)
                                 {
                                     return a[
                                         1,
                                         2] /* note */ + 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int Get(int[,] a)
                                    {
                                        return a[1, 2] /* note */ + 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment written between the brackets still blocks the collapse. This gap is
    /// crossed by the rewrite, so it remains a join barrier and marks the other side of the boundary.
    /// The continuation indentation in the expected text is owned by the indentation phase and is not
    /// what this test pins - the argument staying on its own line is
    /// </summary>
    [TestMethod]
    public void CommentBetweenTheBracketsStillBlocksTheCollapse()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int Get(int[] a)
                                 {
                                     return a[ /* note */
                                         0] + 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int Get(int[] a)
                                    {
                                        return a[ /* note */
                                                 0] + 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment written before the closing bracket still blocks the collapse. The
    /// closing bracket is pulled up by the rewrite, so trivia in front of it is crossed
    /// </summary>
    [TestMethod]
    public void CommentBeforeTheClosingBracketStillBlocksTheCollapse()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int Get(int[] a)
                                 {
                                     return a[
                                         0 /* note */] + 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int Get(int[] a)
                                    {
                                        return a[
                                                 0 /* note */] + 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a directive written between the brackets still blocks the collapse
    /// </summary>
    [TestMethod]
    public void DirectiveBetweenTheBracketsStillBlocksTheCollapse()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int Get(int[] a)
                                 {
                                     return a[
                             #if DEBUG
                                         0
                             #else
                                         1
                             #endif
                                         ] + 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int Get(int[] a)
                                    {
                                        return a[
                                #if DEBUG
                                            0
                                #else
                                                 1
                                #endif
                                                 ] + 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}