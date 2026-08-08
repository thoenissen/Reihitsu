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

    #endregion // Methods
}