using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the colon of a name colon or an expression colon: no space before it and one space behind it, for
/// named arguments, named attribute arguments, tuple element names, and subpattern names
/// </summary>
[TestClass]
public class NameColonSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that the colons of named arguments get no space before and one space behind them, however the author spaced them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-019")]
    public void NamedArgumentColonsAreNormalized()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool b, int c, int d, object o)
                                 {
                                     M(b:b, c :1, d  :  2, o: o);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool b, int c, int d, object o)
                                    {
                                        M(b: b, c: 1, d: 2, o: o);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the colons of named attribute arguments get no space before and one space behind them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-019")]
    public void NamedAttributeArgumentColonsAreNormalized()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 [Obsolete(message:"x", error :true)]
                                 void M()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    [Obsolete(message: "x", error: true)]
                                    void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the colons of named tuple elements get no space before and one space behind them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-019")]
    public void TupleElementNameColonsAreNormalized()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var t = (A:1, B :2, C  :  3);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var t = (A: 1, B: 2, C: 3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the colons of property, extended property, and positional subpatterns get no space before and one space
    /// behind them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-019")]
    public void SubpatternNameColonsAreNormalized()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object o)
                                 {
                                     var a = o is string { Length:1 };
                                     var b = o is C { P.Q :2 };
                                     var c = o is C(X:1, Y :2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o)
                                    {
                                        var a = o is string { Length: 1 };
                                        var b = o is C { P.Q: 2 };
                                        var c = o is C(X: 1, Y: 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that name colons inside a nested argument list and an interpolation hole are spaced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-019")]
    public void NestedNameColonsAreNormalized()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a, int b)
                                 {
                                     M(a:N(x:1), b:2);

                                     var s = $"{N(x:1)}";
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a, int b)
                                    {
                                        M(a: N(x: 1), b: 2);

                                        var s = $"{N(x: 1)}";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the rule: the colons of a <c>case</c> and <c>default</c> label, a conditional, a base list,
    /// and an interpolation's format clause are not name colons and keep their spacing
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-019")]
    public void ColonsThatAreNotNameColonsKeepTheirSpacing()
    {
        // Arrange
        const string input = """
                             class D : C
                             {
                                 string M(int c, bool b)
                                 {
                                     switch (c)
                                     {
                                         case 1:
                                             break;

                                         default:
                                             break;
                                     }

                                     var r = b ? 1 : 2;

                                     return $"{c:N2}{c,5:N2}{r}";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a named argument wrapped behind its tight colon is joined and spaced like the same argument on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-019")]
    [Requirement("FMT-BREAK-031")]
    public void JoinedNamedArgumentIsSpacedLikeTheSameArgumentOnOneLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N(name:
                                       1);
                                     N(name:1);
                                     N(name
                                       :1);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N(name: 1);
                                        N(name: 1);
                                        N(name: 1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a named tuple element wrapped behind its tight colon is joined and spaced like the same element on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-019")]
    [Requirement("FMT-BREAK-031")]
    public void JoinedTupleElementNameIsSpacedLikeTheSameNameOnOneLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var t = (A:
                                              1, B:2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var t = (A: 1, B: 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the comment example of the rule: a delimited documentation comment the colon carries in front of it keeps the
    /// whitespace in front of it and loses the whitespace between it and the colon
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-019")]
    [Requirement("FMT-SPACE-014")]
    public void DocumentationCommentInFrontOfTheColonKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool b)
                                 {
                                     M(b /** x */ :b);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                        M(b /** x */: b);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the comment example of the rule: a comment the colon carries behind it keeps its place, and the value follows it
    /// after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-019")]
    [Requirement("FMT-SPACE-013")]
    public void CommentBehindTheColonKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool b)
                                 {
                                     M(b:/* x */b);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                        M(b:/* x */ b);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment the name carries in front of the colon keeps the whitespace in front of it, and the colon
    /// follows the comment without a space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-019")]
    [Requirement("FMT-SPACE-013")]
    public void CommentInFrontOfTheColonKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool b)
                                 {
                                     M(b /* x */ :b);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                        M(b /* x */: b);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}