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
    /// Verifies that no blank line is inserted above a comment glued to an array initializer's brace or to the closing
    /// parenthesis of an argument list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveCommentGluedToArrayInitializerBraceOrArgumentListParenthesis()
    {
        const string input = """
                             class C
                             {
                                 int[] G { get; } =
                                 /* b */ { 1, 2 };

                                 void M()
                                 {
                                     Foo(1,
                                         2
                                         /* e */ );
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a comment glued to the opening brace of a complex element initializer
    /// on one line, and that the author's blank line there is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesBlankLineAboveCommentGluedToComplexElementInitializerBrace()
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
    /// Verifies that no blank line is inserted above comments glued to the braces of an extension block, and that the
    /// author's blank lines there are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesBlankLinesAboveCommentsGluedToExtensionBlockBraces()
    {
        const string input = """
                             static class E
                             {
                                 extension(string s)
                                 /* c */ {
                                     public int L => s.Length;
                                 /* d */ }

                                 extension(int i)

                                 /* e */ {
                                     public int M => i;

                                 /* f */ }
                             }
                             """;
        const string expected = """
                                static class E
                                {
                                    extension(string s)
                                    /* c */ {
                                        public int L => s.Length;
                                    /* d */ }

                                    extension(int i)
                                    /* e */ {
                                        public int M => i;
                                    /* f */ }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a comment glued to the closing brace of an interpolation
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveCommentGluedToInterpolationClosingBrace()
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

        AssertBlankLinesAbove(input, ("/* d */ }", false));
    }

    /// <summary>
    /// Verifies that the author's blank line above a comment glued to the closing brace of an interpolation is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveCommentGluedToInterpolationClosingBrace()
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

        AssertBlankLinesAbove(input, ("/* d */ }", false));
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

    /// <summary>
    /// Verifies the spec example: the opening brace of a complex element initializer is decided like any other opening
    /// brace, whether the element stays on one line or spans several, so no blank line stands above a comment glued to it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void DecidesCommentGluedToComplexElementInitializerBraceIndependentOfLayout()
    {
        const string input = """
                             class C
                             {
                                 Dictionary<int, int> D = new()
                                                          {
                                                              { 1, 2 },
                                                              /* c1 */ {
                                                                  3,
                                                                  4
                                                              },

                                                              /* c2 */ { 5, 6 }
                                                          };
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    Dictionary<int, int> D = new()
                                                             {
                                                                 { 1, 2 },
                                                                 /* c1 */ {
                                                                              3,
                                                                              4
                                                                          },
                                                                 /* c2 */ { 5, 6 }
                                                             };
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the author's blank line above a comment glued to the opening brace of a complex element initializer
    /// that spans several lines is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveCommentGluedToMultiLineComplexElementInitializer()
    {
        const string input = """
                             class C
                             {
                                 Dictionary<int, int> D = new()
                                                          {
                                                              { 1, 2 },

                                                              /* c1 */ {
                                                                           3,
                                                                           4
                                                                       }
                                                          };
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    Dictionary<int, int> D = new()
                                                             {
                                                                 { 1, 2 },
                                                                 /* c1 */ {
                                                                              3,
                                                                              4
                                                                          }
                                                             };
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a comment glued to the closing brace of a collection initializer,
    /// whether or not the initializer is assigned to a member
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveCommentGluedToClosingBraceOfCollectionInitializer()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 L = {
                                                         1
                                                     /* l */ }
                                             };
                                     var q = new List<int>
                                             {
                                                 1
                                             /* m */ };
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the spec example: the author's blank line above a comment glued to the closing brace of a collection
    /// initializer is removed, whether or not the initializer is assigned to a member
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveCommentGluedToClosingBraceOfCollectionInitializer()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 L = {
                                                         1

                                                     /* l */ }
                                             };
                                     var q = new List<int>
                                             {
                                                 1

                                             /* m */ };
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
                                                    L = {
                                                            1
                                                        /* l */ }
                                                };
                                        var q = new List<int>
                                                {
                                                    1
                                                /* m */ };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a documentation comment between a glued comment and the brace does not change what the
    /// comment is glued to, so the author's blank line above it is removed, while a documentation comment that starts the
    /// brace's line is not glued and is separated by a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void RemovesBlankLineAboveCommentGluedThroughDocumentationCommentAndSeparatesDocumentationCommentLeadingBraceLine()
    {
        const string input = """
                             public class TestClass

                             /* x */ /** Doc */ {
                             }

                             public class TestClass2
                             /** Doc */ {
                             }
                             """;
        const string expected = """
                                public class TestClass
                                /* x */ /** Doc */ {
                                }

                                public class TestClass2

                                /** Doc */ {
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a comment glued to a brace through a documentation comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveCommentGluedThroughDocumentationComment()
    {
        const string input = """
                             public class TestClass
                             /* x */ /** Doc */ {
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the spec example: the author's blank line above a comment glued to an array initializer's brace or to a
    /// chained keyword is removed, and above a comment glued to a statement, a member, or the closing parenthesis of an
    /// argument list none is inserted and the author's is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void DecidesBlankLinesAboveGluedCommentAsDirectlyBeforeItsToken()
    {
        const string input = """
                             class C
                             {
                                 int[] G { get; } =

                                 /* b */ { 1, 2 };
                                 int H;
                                 /* m */ int I;

                                 void M(int a)
                                 {
                                     a++;

                                     /* s */ a++;
                                     Foo(1,
                                         2
                                         /* e */ );

                                     if (a > 0)
                                     {
                                         a++;
                                     }

                                     /* k */ else
                                     {
                                         a--;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] G { get; } =
                                    /* b */ { 1, 2 };
                                    int H;
                                    /* m */ int I;

                                    void M(int a)
                                    {
                                        a++;

                                        /* s */ a++;
                                        Foo(1,
                                            2
                                            /* e */ );

                                        if (a > 0)
                                        {
                                            a++;
                                        }
                                        /* k */ else
                                        {
                                            a--;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a comment glued to a chained keyword
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveCommentGluedToChainedKeyword()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     if (a > 0)
                                     {
                                         a++;
                                     }
                                     /* k */ else
                                     {
                                         a--;
                                     }

                                     try
                                     {
                                         a++;
                                     }
                                     /* c */ catch
                                     {
                                         a--;
                                     }
                                     /* f */ finally
                                     {
                                         a++;
                                     }

                                     do
                                     {
                                         a--;
                                     }
                                     /* w */ while (a > 0);
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank line above a comment glued to <c>catch</c>, <c>finally</c>, or the
    /// <c>while</c> of a do statement is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveCommentGluedToCatchFinallyOrDoWhile()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     try
                                     {
                                         a++;
                                     }

                                     /* c */ catch
                                     {
                                         a--;
                                     }

                                     /* f */ finally
                                     {
                                         a++;
                                     }

                                     do
                                     {
                                         a--;
                                     }

                                     /* w */ while (a > 0);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        try
                                        {
                                            a++;
                                        }
                                        /* c */ catch
                                        {
                                            a--;
                                        }
                                        /* f */ finally
                                        {
                                            a++;
                                        }

                                        do
                                        {
                                            a--;
                                        }
                                        /* w */ while (a > 0);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a comment glued to the opening brace of a property pattern
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveCommentGluedToPropertyPatternBrace()
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is string
                                            /* p */ { Length: 1 };
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* p */", false));
    }

    /// <summary>
    /// Verifies that the author's blank line above a comment glued to the opening brace of a property pattern is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveCommentGluedToPropertyPatternBrace()
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is string

                                            /* p */ { Length: 1 };
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* p */", false));
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a comment glued to a statement after a statement, to a member, or to
    /// the closing parenthesis of an argument list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveCommentGluedToStatementMemberOrArgumentListParenthesis()
    {
        const string input = """
                             class C
                             {
                                 int H;
                                 /* m */ int I;

                                 void M(int a)
                                 {
                                     a++;
                                     /* s */ a++;
                                     Foo(1,
                                         2
                                         /* e */ );
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank line above a comment glued to a member or to the closing parenthesis of an argument
    /// list is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void KeepsAuthorBlankLineAboveCommentGluedToMemberOrArgumentListParenthesis()
    {
        const string input = """
                             class C
                             {
                                 int H;

                                 /* m */ int I;

                                 void M(int a)
                                 {
                                     Foo(1,
                                         2

                                         /* e */ );
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the blank line a statement rule asks for goes above a comment glued to the statement: after a statement
    /// that ends with a closing brace, the blank line of SHARED-BLANK-003 is inserted above the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsStatementBlankLineAboveCommentGluedToStatementAfterClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     if (a > 0)
                                     {
                                         a--;
                                     }
                                     /* s */ a++;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        if (a > 0)
                                        {
                                            a--;
                                        }

                                        /* s */ a++;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line stands above a comment glued to the opening brace of a complex element initializer
    /// whose lambda block body the line-break phase expands, in the first pass and in the second
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveCommentGluedToComplexElementWithLambdaBlockBody()
    {
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 object M()
                                 {
                                     return new Dictionary<int, Action>
                                            {
                                                { 1, null },
                                                /* c */ { 2, () => { Console.WriteLine(); } }
                                            };
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* c */", false));
    }

    /// <summary>
    /// Verifies that no blank line stands above a comment glued to the opening brace of a complex element initializer
    /// whose object initializer the line-break phase expands, in the first pass and in the second
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveCommentGluedToComplexElementWithObjectInitializer()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 object M()
                                 {
                                     return new Dictionary<int, P>
                                            {
                                                { 1, null },
                                                /* c */ { 2, new P { A = 2 } }
                                            };
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* c */", false));
    }

    /// <summary>
    /// Verifies that no blank line stands above a comment glued to the opening brace of a complex element initializer
    /// whose collection initializer the line-break phase expands, in the first pass and in the second
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveCommentGluedToComplexElementWithCollectionInitializer()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 object M()
                                 {
                                     return new Dictionary<int, List<int>>
                                            {
                                                { 1, null },
                                                /* c */ { 2, new List<int> { 3 } }
                                            };
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* c */", false));
    }

    /// <summary>
    /// Verifies that no blank line stands above a comment glued to the opening brace of a complex element initializer
    /// whose anonymous object the line-break phase expands, in the first pass and in the second
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveCommentGluedToComplexElementWithAnonymousObject()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 object M()
                                 {
                                     return new Dictionary<int, object>
                                            {
                                                { 1, null },
                                                /* c */ { 2, new { A = 2 } }
                                            };
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* c */", false));
    }

    /// <summary>
    /// Verifies that the author's blank line above a comment glued to the opening brace of a complex element initializer
    /// whose object initializer the line-break phase expands is removed in the first pass and stays removed in the second
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveCommentGluedToComplexElementWithObjectInitializer()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 object M()
                                 {
                                     return new Dictionary<int, P>
                                            {
                                                { 1, null },

                                                /* c */ { 2, new P { A = 2 } }
                                            };
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* c */", false));
    }

    /// <summary>
    /// Verifies that the author's blank line above a comment glued to the opening brace of a complex element initializer
    /// whose lambda block body the line-break phase expands is removed in the first pass and stays removed in the second
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveCommentGluedToComplexElementWithLambdaBlockBody()
    {
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 object M()
                                 {
                                     return new Dictionary<int, Action>
                                            {
                                                { 1, null },

                                                /* c */ { 2, () => { Console.WriteLine(); } }
                                            };
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* c */", false));
    }

    #endregion // Methods
}