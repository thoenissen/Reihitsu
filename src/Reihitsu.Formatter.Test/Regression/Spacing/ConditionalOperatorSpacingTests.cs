using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the <c>?</c> and <c>:</c> of a conditional expression: one space on each side of both, whether the
/// author wrote them on one line or the line-break phase joined a branch behind them
/// </summary>
[TestClass]
public class ConditionalOperatorSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that a conditional written without spaces gets one space on each side of <c>?</c> and <c>:</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-018")]
    public void TightConditionalGetsOneSpaceOnEachSideOfBothOperators()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(bool b)
                                 {
                                     return b?1:2;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(bool b)
                                    {
                                        return b ? 1 : 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional spaced on one side only, or with runs of whitespace, gets exactly one space on each side of
    /// both operators
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-018")]
    public void UnevenlySpacedConditionalsAreNormalized()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool b, int n)
                                 {
                                     var r = b ? 1:2;
                                     var s = b?1 : 2;
                                     var t = b  ?  1  :  2;
                                     var u = b ?-1:n;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool b, int n)
                                    {
                                        var r = b ? 1 : 2;
                                        var s = b ? 1 : 2;
                                        var t = b ? 1 : 2;
                                        var u = b ? -1 : n;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that both conditionals of a nested conditional get one space on each side of their operators, inside parentheses
    /// on one line and when the nested conditional is laid out over several lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-018")]
    public void NestedConditionalsAreSpaced()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(bool b, bool c)
                                 {
                                     var x = b?(c?1:2):3;

                                     return b?c?1:2:3;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(bool b, bool c)
                                    {
                                        var x = b ? (c ? 1 : 2) : 3;

                                        return b
                                                   ? c
                                                       ? 1
                                                       : 2
                                                   : 3;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that conditionals inside an argument, a nested argument list, an interpolation hole, and an array initializer are
    /// spaced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-018")]
    public void NestedConditionalsInArgumentsInterpolationsAndInitializersAreSpaced()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool b)
                                 {
                                     N(O(b?1:2), b?3:4);

                                     var s = $"{(b?1:2)}";
                                     var a = new[] { b?1:2 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                        N(O(b ? 1 : 2), b ? 3 : 4);

                                        var s = $"{(b ? 1 : 2)}";
                                        var a = new[] { b ? 1 : 2 };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the rule: a nullable type's <c>?</c>, a conditional access, and the <c>??</c> operator are not
    /// a conditional operator and keep their spacing, also next to a conditional
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-018")]
    public void NullableTypesConditionalAccessAndCoalescingAreNotConditionalOperators()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int? n, C c, object o, int[] a)
                                 {
                                     int? m = n;
                                     var s = c?.M(n, c, o, a) ?? 0;
                                     var t = a?[0];
                                     var u = o is int? ? 1 : 2;

                                     return m ?? 0;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a conditional whose branches the line-break phase joins behind tight operators is spaced like the same
    /// conditional written with spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-018")]
    [Requirement("FMT-BREAK-031")]
    public void JoinedBranchesBehindTightOperatorsAreSpacedLikeSpacedOperators()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(bool b)
                                 {
                                     var r = b?
                                             1:
                                             2;
                                     var s = b ?
                                             1 :
                                             2;

                                     return r;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(bool b)
                                    {
                                        var r = b
                                                    ? 1
                                                    : 2;
                                        var s = b
                                                    ? 1
                                                    : 2;

                                        return r;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment the condition carries in front of <c>?</c> keeps its place, and the operator follows it after one
    /// space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-018")]
    [Requirement("FMT-SPACE-013")]
    public void CommentInFrontOfTheQuestionMarkKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(bool b)
                                 {
                                     return b/* x */?1:2;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(bool b)
                                    {
                                        return b/* x */ ? 1 : 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment the colon carries behind it keeps its place, and the second branch follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-018")]
    [Requirement("FMT-SPACE-013")]
    public void CommentBehindTheColonKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(bool b)
                                 {
                                     return b ? 1 :/* z */2;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(bool b)
                                    {
                                        return b ? 1 :/* z */ 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}