using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for the blank lines above a comment that starts its line and shares it with the brace, bracket, or
/// parenthesis behind it
/// </summary>
[TestClass]
public class GluedCommentBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the author's blank line above a comment glued to an object initializer's opening brace is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveCommentGluedToInitializerBrace()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 Child =

                                                 /* c */ {
                                                             X = 2
                                                         }
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var p = new P
                                                {
                                                    Child =
                                                    /* c */ {
                                                                X = 2
                                                            }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment glued to an object initializer's opening brace gets no blank line above it, so the output
    /// is the formatter's fixed point and no phase, run alone, changes it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveCommentGluedToInitializerBrace()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 Child =
                                                 /* c */ {
                                                     X = 2
                                                 }
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var p = new P
                                                {
                                                    Child =
                                                    /* c */ {
                                                                X = 2
                                                            }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that comments glued to the brace of a block statement after another statement, to an initializer's opening
    /// brace, and to an initializer's closing brace stay without a blank line above them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void KeepsCommentsGluedToBracesWithoutBlankLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     A();
                                     /* a */ {
                                         B();
                                     }

                                     var p = new P
                                             {
                                                 Child =
                                                 /* c */ {
                                                             X = 2
                                                         }
                                             /* d */ };
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment glued to an array initializer's brace or to the closing parenthesis of an argument list is
    /// separated by a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void SeparatesCommentGluedToArrayInitializerBraceAndArgumentListParenthesis()
    {
        const string input = """
                             class C
                             {
                                 int[] G =
                                 /* b */ { 1, 2 };

                                 void M()
                                 {
                                     Foo(1,
                                         2
                                         /* e */ );
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] G =

                                    /* b */ { 1, 2 };

                                    void M()
                                    {
                                        Foo(1,
                                            2

                                            /* e */ );
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment glued to the opening brace of a complex element initializer is separated by a blank line,
    /// and that the author's blank line there is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void SeparatesCommentGluedToComplexElementInitializerBrace()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 Dictionary<int, int> D = new()
                                                          {
                                                              { 1, 2 },
                                                              /* c */ { 3, 4 }
                                                          };

                                 Dictionary<int, int> E = new()
                                                          {
                                                              { 1, 2 },

                                                              /* c */ { 3, 4 }
                                                          };
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    Dictionary<int, int> D = new()
                                                             {
                                                                 { 1, 2 },

                                                                 /* c */ { 3, 4 }
                                                             };

                                    Dictionary<int, int> E = new()
                                                             {
                                                                 { 1, 2 },

                                                                 /* c */ { 3, 4 }
                                                             };
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that comments glued to the braces of an extension block are separated by a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void SeparatesCommentsGluedToExtensionBlockBraces()
    {
        const string input = """
                             static class E
                             {
                                 extension(string s)
                                 /* c */ {
                                     public int L => s.Length;
                                 /* d */ }
                             }
                             """;
        const string expected = """
                                static class E
                                {
                                    extension(string s)

                                    /* c */ {
                                        public int L => s.Length;

                                    /* d */ }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment glued to the closing brace of an interpolation is separated by a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void SeparatesCommentGluedToInterpolationClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 string I(int x)
                                 {
                                     return $"{x
                                         /* d */ }";
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* d */ }", true));
    }

    /// <summary>
    /// Verifies that the author's blank line above a comment glued to a block's closing brace is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveCommentGluedToClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;

                                 /* d */ }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                    /* d */ }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the author's blank line above a comment glued to the closing bracket of a collection expression or a
    /// list pattern, or to the closing brace of a switch expression, is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveCommentGluedToClosingBracketOrSwitchExpressionBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(object o, int a)
                                 {
                                     int[] f = [
                                         1,

                                     /* d */ ];
                                     var b = o is [
                                             1

                                         /* e */ ];
                                     var s = a switch
                                     {
                                         _ => 0

                                     /* f */ };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o, int a)
                                    {
                                        int[] f = [
                                                      1,
                                                  /* d */ ];
                                        var b = o is [
                                                         1
                                                     /* e */ ];
                                        var s = a switch
                                                {
                                                    _ => 0
                                                /* f */ };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the author's blank line above a comment glued to the closing parenthesis of a parenthesized pattern
    /// is removed. Only the blank lines are asserted: no requirement decides the columns of a parenthesized pattern's lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveCommentGluedToParenthesizedPatternParenthesis()
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is (
                                         1 or 2

                                         /* d */ );
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* d */ )", false));
    }

    /// <summary>
    /// Verifies that below a region directive the blank line the region rule requires is inserted above a glued comment,
    /// and that below another directive the author's blank line is kept and none is inserted in front of the directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void KeepsBlankLinesOfGapWithDirectiveInFrontOfGluedComment()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     #region R

                                     a++;

                                     #endregion // R
                                 /* d */ }

                                 void N(int a)
                                 {
                                     a++;
                             #pragma warning disable CS0168

                                 /* e */ }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        #region R

                                        a++;

                                        #endregion // R

                                    /* d */ }

                                    void N(int a)
                                    {
                                        a++;
                                #pragma warning disable CS0168

                                    /* e */ }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line below an <c>#endregion</c> in front of a comment glued to the closing bracket of a list
    /// pattern is kept. Only the blank lines are asserted: no requirement decides the column of a region directive inside
    /// a pattern
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void KeepsBlankLineBelowEndRegionInFrontOfCommentGluedToListPatternBracket()
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is [
                                             1,
                                             #region R
                                             2
                                             #endregion // R

                                         /* d */ ];
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* d */ ]", true));
    }

    /// <summary>
    /// Verifies that the author's blank line between an <c>#endif</c> and a comment glued to a closing brace is kept and
    /// that none is inserted in front of the <c>#if</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void KeepsBlankLineBelowEndIfInFrontOfGluedCommentAndInsertsNoneAboveIf()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #if DEBUG
                                     a--;
                             #endif

                                 /* d */ }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank lines on both sides of a conditional block in front of a glued comment are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void KeepsAuthorBlankLinesOnBothSidesOfConditionalBlockInFrontOfGluedComment()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;

                             #if DEBUG
                                     a--;
                             #endif

                                 /* d */ }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that no blank line is inserted directly below an <c>#endif</c> in front of a glued comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineDirectlyBelowEndIfInFrontOfGluedComment()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #if DEBUG
                                     a--;
                             #endif
                                 /* d */ }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that in an initializer whose closing brace carries a glued comment behind a conditional block and an
    /// own-line comment, the author's blank line between the comments is kept and none is inserted in front of the
    /// <c>#if</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void KeepsBlankLineOfInitializerGapWithDirectiveAndCommentsInFrontOfGluedComment()
    {
        const string input = """
                             class C
                             {
                                 void O()
                                 {
                                     var p = new P
                                             {
                                                 X = 1
                             #if NEVER
                                                 , Y = 2
                             #endif
                                                 // c

                                             /* d */ };
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment glued to the brace of a block statement after a closing brace gets the blank line that
    /// follows a closing brace, and that the author's blank line above a comment glued to the brace of a block statement
    /// after another statement is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    [Requirement("FMT-BLANK-018")]
    public void SeparatesCommentGluedToBlockStatementBraceAfterStatement()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                     {
                                     }
                                     /* c */ {
                                         B();
                                     }

                                     A();

                                     /* d */ {
                                         B();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a)
                                        {
                                        }

                                        /* c */ {
                                            B();
                                        }

                                        A();

                                        /* d */ {
                                            B();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line comment in front of a comment glued to an initializer's closing brace gets the blank line
    /// above it, and the glued comment gets none
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void InsertsBlankLineAboveOwnLineCommentInFrontOfGluedComment()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var q = new P
                                             {
                                                 X = 1
                                                 // note
                                             /* d */ };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var q = new P
                                                {
                                                    X = 1

                                                    // note
                                                /* d */ };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that with blank lines on both sides of an own-line comment in front of a glued comment, only the one above
    /// the own-line comment stays
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void KeepsOnlyBlankLineAboveOwnLineCommentInFrontOfGluedComment()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var q = new P
                                             {
                                                 X = 1

                                                 // note

                                             /* d */ };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var q = new P
                                                {
                                                    X = 1

                                                    // note
                                                /* d */ };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line comment in front of a comment glued to an initializer's opening brace gets the blank line
    /// above it, and the blank line between the two comments is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void MovesBlankLineAboveOwnLineCommentInFrontOfCommentGluedToOpeningBrace()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var q = new P
                                             {
                                                 Child =
                                                 // note

                                                 /* c */ {
                                                             X = 2
                                                         }
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var q = new P
                                                {
                                                    Child =

                                                    // note
                                                    /* c */ {
                                                                X = 2
                                                            }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}