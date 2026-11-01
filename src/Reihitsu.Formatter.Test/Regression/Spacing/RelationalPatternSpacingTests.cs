using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the space between the operator of a relational pattern and its operand: one space, wherever the
/// pattern stands and whether the author wrote it on one line or the line-break phase joined it
/// </summary>
[TestClass]
public class RelationalPatternSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that every relational operator gets one space in front of its operand, however the author spaced it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-023")]
    public void EveryRelationalOperatorGetsOneSpaceInFrontOfItsOperand()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     var a = x is >5;
                                     var b = x is <10;
                                     var c = x is >=5;
                                     var d = x is <=  10;
                                     var e = x is >5 and <10;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        var a = x is > 5;
                                        var b = x is < 10;
                                        var c = x is >= 5;
                                        var d = x is <= 10;
                                        var e = x is > 5 and < 10;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that relational patterns in a <c>case</c> label, a switch-expression arm, a property subpattern, and a list
    /// pattern are spaced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-023")]
    public void RelationalPatternsInEveryPatternContextAreSpaced()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x, string s, int[] a)
                                 {
                                     switch (x)
                                     {
                                         case >5:
                                             break;
                                     }

                                     var p = s is { Length: >0 };
                                     var q = a is [>0, <=9];

                                     return x switch
                                            {
                                                <0 => -1,
                                                _ => 1
                                            };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x, string s, int[] a)
                                    {
                                        switch (x)
                                        {
                                            case > 5:
                                                break;
                                        }

                                        var p = s is { Length: > 0 };
                                        var q = a is [> 0, <= 9];

                                        return x switch
                                               {
                                                   < 0 => -1,
                                                   _ => 1
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a relational pattern wrapped behind its operator inside an argument is joined and spaced like the same
    /// pattern on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-023")]
    [Requirement("FMT-BREAK-031")]
    public void JoinedRelationalPatternIsSpacedLikeTheSamePatternOnOneLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     N(x is >
                                       1);
                                     N(x is >1);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        N(x is > 1);
                                        N(x is > 1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the rule: the operators of a binary comparison are decided by the operator rule, not by this
    /// one
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-023")]
    public void BinaryComparisonsAreNotRelationalPatterns()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int x)
                                 {
                                     return x>5 && x<=10;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(int x)
                                    {
                                        return x > 5 && x <= 10;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment the operator carries keeps its place, and the operand follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-023")]
    [Requirement("FMT-SPACE-013")]
    public void CommentBehindTheOperatorKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int x)
                                 {
                                     return x is >/* a */5;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(int x)
                                    {
                                        return x is >/* a */ 5;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment the operand carries keeps the whitespace in front of it, and the operand
    /// follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-023")]
    [Requirement("FMT-SPACE-014")]
    public void DocumentationCommentInFrontOfTheOperandKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int x)
                                 {
                                     return x is > /** a */5;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(int x)
                                    {
                                        return x is > /** a */ 5;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}