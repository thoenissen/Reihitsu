using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the gap around a delimited documentation comment that the right token of a pair carries on the
/// line: the whitespace in front of the comment stays as written, and the gap behind it follows the rule for the pair, also
/// where the rule demands a space the author did not write
/// </summary>
[TestClass]
public class RightCarriedDocumentationCommentSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that the space a binary operator demands in front of it is inserted behind the comment the operator carries
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-014")]
    [Requirement("FMT-SPACE-010")]
    public void SpaceInFrontOfABinaryOperatorIsInsertedBehindTheComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int b)
                                 {
                                     var d = b /** sum */+b;
                                     var e = b /** product */*b;

                                     return d /** both */-e;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int b)
                                    {
                                        var d = b /** sum */ + b;
                                        var e = b /** product */ * b;

                                        return d /** both */ - e;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a run of spaces behind the comment a binary operator carries becomes the one space the operator demands
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-014")]
    [Requirement("FMT-SPACE-010")]
    public void RunOfSpacesBehindTheCommentBecomesTheDemandedSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int b)
                                 {
                                     return b /** sum */   + b;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int b)
                                    {
                                        return b /** sum */ + b;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the double space in front of the comment stays as written, while the gap behind the comment gets the one
    /// space the operator demands
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-014")]
    [Requirement("FMT-SPACE-010")]
    public void DoubleSpaceInFrontOfTheCommentIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int b)
                                 {
                                     var d = b  /** sum */+ b;
                                     var e = b  /** sum */+b;

                                     return d  /** both */ + e;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int b)
                                    {
                                        var d = b  /** sum */ + b;
                                        var e = b  /** sum */ + b;

                                        return d  /** both */ + e;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space the conditional operator demands in front of its colon is inserted behind the comment the colon
    /// carries, and that a run of spaces there becomes that one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-014")]
    [Requirement("FMT-SPACE-018")]
    public void SpaceInFrontOfTheConditionalColonIsInsertedBehindTheComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(bool c, int b)
                                 {
                                     var d = c ? b /** yes */   : b;

                                     return c ? b /** yes */:b;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(bool c, int b)
                                    {
                                        var d = c ? b /** yes */ : b;

                                        return c ? b /** yes */ : b;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the whitespace between a comment the left token carries and a comment the right token carries stays as
    /// written, and the gap behind the right token's comment follows the operator's rule
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-014")]
    [Requirement("FMT-SPACE-013")]
    public void WhitespaceBetweenTheCommentsOfBothTokensIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int b)
                                 {
                                     return b /* x */   /** y */+b;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int b)
                                    {
                                        return b /* x */   /** y */ + b;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}