using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for the blank lines above a delimited comment that starts on an earlier line and ends on the line of
/// the token behind it, and for the comments that stay on their own line although a token follows their closing <c>*/</c>
/// </summary>
[TestClass]
public class MultiLineGluedCommentBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the example of the comment-position classification: comments that end on the token's line get no blank
    /// line, while a comment in front of a chain link the formatter moves off its line and a comment in front of the end of
    /// the file get the blank line of a comment on its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void ClassifiesCommentsEndingOnTokenLineAsGluedAndOthersAsOwnLine()
    {
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     A();
                                     /* a */ B();
                                     /* b
                                      */ B();
                                     var r = a.B()
                                              .C()
                                              /* c
                                               */ .D();
                                 }

                                 void N(int a)

                                 /* d
                                  */ {
                                     a++;

                                     /* e */ /* f
                                      */ }
                             }
                             // trailing
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        A();
                                        /* a */ B();
                                        /* b
                                         */ B();

                                        var r = a.B()
                                                 .C()

                                                 /* c
                                                  */
                                                 .D();
                                    }

                                    void N(int a)
                                    /* d
                                     */ {
                                        a++;
                                        /* e */ /* f
                                         */ }
                                }

                                // trailing
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a multi-line comment glued to a method body's opening brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentGluedToMethodBodyBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 /* x
                                  */ {
                                     a++;
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank line above a multi-line comment glued to a method body's opening brace is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveMultiLineCommentGluedToMethodBodyBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)

                                 /* x
                                  */ {
                                     a++;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    /* x
                                     */ {
                                        a++;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a multi-line comment glued to a type's opening brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentGluedToTypeBrace()
    {
        const string input = """
                             class C
                             /* x
                              */ {
                                 int F;
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank line above a multi-line comment glued to a type's opening brace is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveMultiLineCommentGluedToTypeBrace()
    {
        const string input = """
                             class C

                             /* x
                              */ {
                                 int F;
                             }
                             """;
        const string expected = """
                                class C
                                /* x
                                 */ {
                                    int F;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a multi-line comment glued to a method body's closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentGluedToClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                                     /* x
                                      */ }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank line above a multi-line comment glued to a method body's closing brace is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveMultiLineCommentGluedToClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;

                                     /* x
                                      */ }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        /* x
                                         */ }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a comment that stands in front of a multi-line comment on its first
    /// line, when the multi-line comment ends on the line of a closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveCommentInFrontOfMultiLineCommentGluedToClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                                     /* a */ /* x
                                      */ }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank line above a comment that stands in front of a multi-line comment on its first line
    /// is removed, when the multi-line comment ends on the line of a closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveCommentInFrontOfMultiLineCommentGluedToClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;

                                     /* a */ /* x
                                      */ }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        /* a */ /* x
                                         */ }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above multi-line comments glued to an object initializer's opening and
    /// closing braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentsGluedToObjectInitializerBraces()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             /* x
                                      */ {
                                                 X = 1
                                                 /* y
                                      */ };
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank lines above multi-line comments glued to an object initializer's opening and
    /// closing braces are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLinesAboveMultiLineCommentsGluedToObjectInitializerBraces()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P

                                             /* x
                                      */ {
                                                 X = 1

                                                 /* y
                                      */ };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var p = new P
                                                /* x
                                         */ {
                                                    X = 1
                                                    /* y
                                         */ };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a multi-line comment glued to the opening brace of a collection
    /// initializer assigned to a member
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentGluedToAssignedCollectionInitializerBrace()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 L =
                                                 /* x
                                                  */ {
                                                         1
                                                     }
                                             };
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank line above a multi-line comment glued to the opening brace of a collection
    /// initializer assigned to a member is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveMultiLineCommentGluedToAssignedCollectionInitializerBrace()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 L =

                                                 /* x
                                                  */ {
                                                         1
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
                                                    L =
                                                    /* x
                                                     */ {
                                                            1
                                                        }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a multi-line comment glued to the closing bracket of a collection
    /// expression
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentGluedToCollectionExpressionBracket()
    {
        const string input = """
                             class C
                             {
                                 int[] F = [
                                               1
                                           /* x
                                      */ ];
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank line above a multi-line comment glued to the closing bracket of a collection
    /// expression is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveMultiLineCommentGluedToCollectionExpressionBracket()
    {
        const string input = """
                             class C
                             {
                                 int[] F = [
                                               1

                                           /* x
                                      */ ];
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] F = [
                                                  1
                                              /* x
                                         */ ];
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a multi-line comment glued to the closing bracket of a list pattern
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentGluedToListPatternBracket()
    {
        const string input = """
                             class C
                             {
                                 bool M(int[] a)
                                 {
                                     return a is [
                                                     1
                                                 /* y
                                                 */ ];
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank line above a multi-line comment glued to the closing bracket of a list pattern is
    /// removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveMultiLineCommentGluedToListPatternBracket()
    {
        const string input = """
                             class C
                             {
                                 bool M(int[] a)
                                 {
                                     return a is [
                                                     1

                                                 /* y
                                                 */ ];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(int[] a)
                                    {
                                        return a is [
                                                        1
                                                    /* y
                                                    */ ];
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a multi-line comment glued to the closing parenthesis of a
    /// parenthesized pattern. Only the blank lines are asserted: no requirement decides the columns of a parenthesized
    /// pattern's lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentGluedToParenthesizedPatternParenthesis()
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is (
                                         1 or 2
                                         /* z
                                          */ );
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* z", false));
    }

    /// <summary>
    /// Verifies that the author's blank line above a multi-line comment glued to the closing parenthesis of a
    /// parenthesized pattern is removed. Only the blank lines are asserted: no requirement decides the columns of a
    /// parenthesized pattern's lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveMultiLineCommentGluedToParenthesizedPatternParenthesis()
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is (
                                         1 or 2

                                         /* z
                                          */ );
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* z", false));
    }

    /// <summary>
    /// Verifies that an own-line comment in front of a multi-line comment glued to a closing brace gets the blank line of
    /// the first comment of the gap, and that the blank line between the two comments is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    [Requirement("FMT-BLANK-020")]
    public void SeparatesOwnLineCommentInFrontOfMultiLineCommentGluedToClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                                     // c

                                     /* x
                                      */ }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;

                                        // c
                                        /* x
                                         */ }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that multi-line comments glued to an array initializer's brace, a member, a statement, and <c>else</c>
    /// stay without a blank line above them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentsGluedToArrayBraceMemberStatementOrElse()
    {
        const string input = """
                             class C
                             {
                                 int[] G =
                                 /* g
                                  */ { 1, 2 };
                                 int H;
                                 /* m
                                  */ int I;

                                 void M(int a)
                                 {
                                     a++;
                                     /* s
                                      */ a++;

                                     if (a > 0)
                                     {
                                         a++;
                                     }
                                     /* k
                                      */ else
                                     {
                                         a--;
                                     }
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank lines above multi-line comments glued to an array initializer's brace and to
    /// <c>else</c> are removed, and the ones above multi-line comments glued to a member and a statement are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void DecidesAuthorBlankLinesAboveMultiLineCommentsGluedToArrayBraceMemberStatementOrElse()
    {
        const string input = """
                             class C
                             {
                                 int[] G =

                                 /* g
                                  */ { 1, 2 };
                                 int H;

                                 /* m
                                  */ int I;

                                 void M(int a)
                                 {
                                     a++;

                                     /* s
                                      */ a++;

                                     if (a > 0)
                                     {
                                         a++;
                                     }

                                     /* k
                                      */ else
                                     {
                                         a--;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] G =
                                    /* g
                                     */ { 1, 2 };
                                    int H;

                                    /* m
                                     */ int I;

                                    void M(int a)
                                    {
                                        a++;

                                        /* s
                                         */ a++;

                                        if (a > 0)
                                        {
                                            a++;
                                        }
                                        /* k
                                         */ else
                                        {
                                            a--;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above multi-line comments glued to <c>catch</c>, <c>finally</c>, and the
    /// <c>while</c> of a do statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentsGluedToCatchFinallyOrDoWhile()
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
                                     /* c
                                      */ catch
                                     {
                                         a--;
                                     }
                                     /* f
                                      */ finally
                                     {
                                         a++;
                                     }

                                     do
                                     {
                                         a--;
                                     }
                                     /* w
                                      */ while (a > 0);
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank lines above multi-line comments glued to <c>catch</c>, <c>finally</c>, and the
    /// <c>while</c> of a do statement are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLinesAboveMultiLineCommentsGluedToCatchFinallyOrDoWhile()
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

                                     /* c
                                      */ catch
                                     {
                                         a--;
                                     }

                                     /* f
                                      */ finally
                                     {
                                         a++;
                                     }

                                     do
                                     {
                                         a--;
                                     }

                                     /* w
                                      */ while (a > 0);
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
                                        /* c
                                         */ catch
                                        {
                                            a--;
                                        }
                                        /* f
                                         */ finally
                                        {
                                            a++;
                                        }

                                        do
                                        {
                                            a--;
                                        }
                                        /* w
                                         */ while (a > 0);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a multi-line comment glued to the opening brace of a complex element
    /// initializer. Only the blank lines are asserted: the requirement decides no column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentGluedToComplexElementInitializerBrace()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 Dictionary<int, int> D = new()
                                                          {
                                                              { 1, 2 },
                                                              /* c
                                                               */ { 5, 6 }
                                                          };
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* c", false));
    }

    /// <summary>
    /// Verifies that the author's blank line above a multi-line comment glued to the opening brace of a complex element
    /// initializer is removed. Only the blank lines are asserted: the requirement decides no column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveMultiLineCommentGluedToComplexElementInitializerBrace()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 Dictionary<int, int> D = new()
                                                          {
                                                              { 1, 2 },

                                                              /* c
                                                               */ { 5, 6 }
                                                          };
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* c", false));
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a multi-line comment glued to the closing brace of a collection
    /// initializer assigned to a member. Only the blank lines are asserted: the requirement decides no column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentGluedToClosingBraceOfAssignedCollectionInitializer()
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
                                                     /* l
                                                      */ }
                                             };
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* l", false));
    }

    /// <summary>
    /// Verifies that the author's blank line above a multi-line comment glued to the closing brace of a collection
    /// initializer assigned to a member is removed. Only the blank lines are asserted: the requirement decides no column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveMultiLineCommentGluedToClosingBraceOfAssignedCollectionInitializer()
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

                                                     /* l
                                                      */ }
                                             };
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("/* l", false));
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a multi-line comment glued to a type's opening brace through a
    /// documentation comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentGluedThroughDocumentationComment()
    {
        const string input = """
                             public class TestClass
                             /* x
                              */ /** Doc */ {
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank line above a multi-line comment glued to a type's opening brace through a
    /// documentation comment is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesAuthorBlankLineAboveMultiLineCommentGluedThroughDocumentationComment()
    {
        const string input = """
                             public class TestClass

                             /* x
                              */ /** Doc */ {
                             }
                             """;
        const string expected = """
                                public class TestClass
                                /* x
                                 */ /** Doc */ {
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above multi-line comments glued to the closing parenthesis of an argument
    /// list and to the first link of a chain
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void InsertsNoBlankLineAboveMultiLineCommentsGluedToArgumentListParenthesisOrFirstChainLink()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1,
                                         2
                                         /* e
                                          */ );
                                 }

                                 object N(dynamic a)
                                 {
                                     return a
                                            /* x
                                             */ .B();
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank lines above multi-line comments glued to the closing parenthesis of an argument
    /// list and to the first link of a chain are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void KeepsAuthorBlankLinesAboveMultiLineCommentsGluedToArgumentListParenthesisOrFirstChainLink()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1,
                                         2

                                         /* e
                                          */ );
                                 }

                                 object N(dynamic a)
                                 {
                                     return a

                                            /* x
                                             */ .B();
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the blank line a statement rule asks for goes above a multi-line comment glued to the statement: a
    /// <c>return</c> after another statement and a <c>case</c> label after <c>break</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsStatementBlankLineAboveMultiLineCommentGluedToReturnOrCaseLabel()
    {
        const string input = """
                             class C
                             {
                                 int M(dynamic a, int b)
                                 {
                                     switch (b)
                                     {
                                         case 1:
                                             a.B();
                                             break;
                                         /* y
                                          */ case 2:
                                             a.C();
                                             break;
                                     }

                                     a.B();
                                     /* x
                                      */ return 1;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(dynamic a, int b)
                                    {
                                        switch (b)
                                        {
                                            case 1:
                                                a.B();
                                                break;

                                            /* y
                                             */ case 2:
                                                a.C();
                                                break;
                                        }

                                        a.B();

                                        /* x
                                         */ return 1;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line required after a closing brace goes above a multi-line comment glued to the following
    /// statement, and that above a multi-line comment glued to a member after a closing brace none is inserted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void InsertsBlankLineAfterClosingBraceAboveMultiLineCommentGluedToStatementButNotMember()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     if (true)
                                     {
                                     }
                                     /* z
                                      */ int i = 0;
                                 }
                                 /* x
                                  */ int I;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        if (true)
                                        {
                                        }

                                        /* z
                                         */ int i = 0;
                                    }
                                    /* x
                                     */ int I;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that above a multi-line comment glued to the brace of a block statement, the blank line after a closing
    /// brace is inserted, and after any other statement none is inserted and the author's is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    [Requirement("FMT-BLANK-018")]
    public void DecidesBlankLinesAboveMultiLineCommentGluedToBlockStatementBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                     {
                                     }
                                     /* x
                                      */ {
                                         B();
                                     }

                                     A();
                                     /* y
                                      */ {
                                         B();
                                     }

                                     A();

                                     /* w
                                      */ {
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

                                        /* x
                                         */ {
                                            B();
                                        }

                                        A();
                                        /* y
                                         */ {
                                            B();
                                        }

                                        A();

                                        /* w
                                         */ {
                                            B();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line after <c>break</c> goes above a multi-line comment glued to the brace of a block
    /// statement that follows the <c>break</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    [Requirement("FMT-BLANK-018")]
    public void InsertsBlankLineAfterBreakAboveMultiLineCommentGluedToBlockStatementBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     while (a > 0)
                                     {
                                         break;
                                         /* c
                                          */ {
                                             a++;
                                         }
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (a > 0)
                                        {
                                            break;

                                            /* c
                                             */ {
                                                a++;
                                            }
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line documentation comment that starts the line of a method body's opening brace is not glued
    /// to the brace, so the blank line of a comment is inserted above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void InsertsBlankLineAboveMultiLineDocumentationCommentStartingBraceLine()
    {
        const string input = """
                             class C
                             {
                                 void N(int a)
                                 /** d
                                  */ {
                                     a++;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void N(int a)

                                    /** d
                                     */ {
                                        a++;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line above a multi-line documentation comment that starts the line of a method body's
    /// opening brace is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void KeepsBlankLineAboveMultiLineDocumentationCommentStartingBraceLine()
    {
        const string input = """
                             class C
                             {
                                 void N(int a)

                                 /** d
                                  */ {
                                     a++;
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a multi-line comment followed by a line comment on its last line is on its own line: the blank line of
    /// a comment is inserted above it, and the blank line between it and the closing brace is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-020")]
    public void SeparatesMultiLineCommentEndingInLineCommentAndRemovesBlankLineBeforeClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                                     /* x
                                      */ // c

                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;

                                        /* x
                                         */ // c
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment in front of a chain link the formatter moves onto its own line behind the comment's
    /// closing <c>*/</c> is on its own line: the blank line of a comment is inserted above it, and the output is stable
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void InsertsBlankLineAboveMultiLineCommentInFrontOfChainLinkMovedOffItsLine()
    {
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()
                                              /* c
                                               */ .D();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var r = a.B()
                                                 .C()

                                                 /* c
                                                  */
                                                 .D();
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the author's blank line above a comment in front of a chain link the formatter moves onto its own line
    /// behind the comment's closing <c>*/</c> is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void KeepsBlankLineAboveMultiLineCommentInFrontOfChainLinkMovedOffItsLine()
    {
        const string input = """
                             class C
                             {
                                 void M(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()

                                              /* c
                                               */ .D();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(dynamic a)
                                    {
                                        var r = a.B()
                                                 .C()

                                                 /* c
                                                  */
                                                 .D();
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment in front of a prefix chain link the formatter moves onto its own line behind the comment's
    /// closing <c>*/</c> is on its own line: the blank line of a comment is inserted above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-018")]
    public void InsertsBlankLineAboveMultiLineCommentInFrontOfPrefixChainLinkMovedOffItsLine()
    {
        const string input = """
                             class C
                             {
                                 object M(dynamic a)
                                 {
                                     var r = a.B
                                              /* x
                                               */ .B
                                              .C()
                                              .D();

                                     return r;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M(dynamic a)
                                    {
                                        var r = a.B

                                                 /* x
                                                  */
                                                 .B
                                                 .C()
                                                 .D();

                                        return r;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}