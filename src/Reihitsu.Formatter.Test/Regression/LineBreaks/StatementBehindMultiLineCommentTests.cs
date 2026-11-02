using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Tests the output for a statement that stands behind the closing line of a comment that spans several lines: it stays
/// behind the comment, unless the comment starts behind the previous statement and a blank-line requirement separates the
/// two, or the comment starts behind the opening brace or the section's last label in front of a first statement; then it
/// starts the line below the comment, in a block and in a switch section alike
/// </summary>
[TestClass]
public class StatementBehindMultiLineCommentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that a <c>return</c> behind the closing line of a comment that spans several lines, which trails an
    /// expression statement, starts the line below the comment and gets the blank line in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void ReturnBehindCommentTailIsSeparatedByABlankLine()
    {
        const string input = """
                             class C
                             {
                                 int M(int a)
                                 {
                                     a++; /* x
                                     */ return a;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        a++; /* x
                                        */

                                        return a;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>return</c> behind the closing line of a comment that spans several lines, which trails an empty
    /// statement, starts the line below the comment and gets the blank line in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void ReturnBehindCommentTailOfEmptyStatementIsSeparatedByABlankLine()
    {
        const string input = """
                             class C
                             {
                                 int M(int a)
                                 {
                                     a++;
                                     ; /* x
                                     */ return a;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        a++;
                                        ; /* x
                                        */

                                        return a;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an expression statement behind the closing line of a comment that spans several lines, which trails a
    /// statement ending with a closing brace, starts the line below the comment and gets the blank line in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void StatementBehindCommentTailOfClosingBraceIsSeparatedByABlankLine()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     if (a > 0) { A(); } /* q
                                     */ B();
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
                                            A();
                                        } /* q
                                        */

                                        B();
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>break</c> in a loop body behind the closing line of a comment that spans several lines starts the
    /// line below the comment and gets the blank line in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void BreakInLoopBehindCommentTailIsSeparatedByABlankLine()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     while (a > 0) { A(); /* x
                                     */ break; }
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
                                            A(); /* x
                                        */

                                            break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an expression statement behind the closing line of a comment that spans several lines, which trails a
    /// local declaration, starts the line below the comment and gets the blank line in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void ExpressionBehindCommentTailOfLocalDeclarationIsSeparatedByABlankLine()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     int b = a; /* x
                                     */ Foo(b);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        int b = a; /* x
                                        */

                                        Foo(b);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>return</c> behind the closing line of a comment that spans several lines, which trails an empty
    /// statement on a section label's line, gets the blank line in front of it, and the section is braced in the first run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void ReturnBehindCommentTailOfEmptyStatementInSwitchSectionIsBraced()
    {
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: ; /* a
                                         */ return 1;
                                         default:
                                             return 0;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    ; /* a
                                            */

                                                    return 1;
                                                }
                                            default:
                                                {
                                                    return 0;
                                                }
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>return</c> behind the closing line of a comment that spans several lines, which trails the first
    /// statement of a section, gets the blank line in front of it, and the section is braced in the first run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void ReturnBehindCommentTailInSwitchSectionIsBraced()
    {
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: A(); /* x
                                         */ return 1;
                                         default:
                                             return 0;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A(); /* x
                                            */

                                                    return 1;
                                                }
                                            default:
                                                {
                                                    return 0;
                                                }
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the multi-line comment example of FMT-BREAK-006: a statement behind the comment's closing line stays there,
    /// unless a blank-line requirement separates it from the statement whose line the comment starts on, or it is the first
    /// statement of a section whose label line the comment starts on
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void StatementsBehindCommentTailFollowTheExampleOfOneStatementPerLine()
    {
        const string input = """
                             class C
                             {
                                 int M(int a)
                                 {
                                     A(); /* x
                                     */ B();
                                     a++; /* y
                                     */ return a;
                                 }

                                 int N(int a)
                                 {
                                     a++;
                                     /* z
                                     */ return a;
                                 }

                                 void O(int a)
                                 {
                                     switch (a)
                                     {
                                         case 1: /* p
                                         */ A();
                                             break;
                                         case 2:
                                             /* q
                                             */ B();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        A(); /* x
                                        */ B();
                                        a++; /* y
                                        */

                                        return a;
                                    }

                                    int N(int a)
                                    {
                                        a++;

                                        /* z
                                        */ return a;
                                    }

                                    void O(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1: /* p
                                            */
                                                A();
                                                break;

                                            case 2:
                                                /* q
                                                */ B();
                                                break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a statement behind the closing line of a comment that trails the previous statement stays behind the
    /// comment when no blank-line requirement separates the two statements
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void StatementBehindCommentTailWithoutBlankLineRequirementStaysBehindTheComment()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     A(); /* x
                                     */ B();
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a statement behind the closing line of a comment that starts on a line of its own stays behind the
    /// comment although a blank-line requirement applies, and the blank line goes above the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void ReturnBehindTailOfCommentOnItsOwnLineStaysAndGetsTheBlankLineAboveTheComment()
    {
        const string input = """
                             class C
                             {
                                 int M(int a)
                                 {
                                     a++;
                                     /* z
                                     */ return a;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        a++;

                                        /* z
                                        */ return a;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the first statement of a block behind the closing line of a comment that starts behind the opening
    /// brace starts the line below the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void FirstStatementBehindTailOfCommentBehindOpeningBraceStartsTheLineBelow()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 { /* f
                                   */ A();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    { /* f
                                      */
                                        A();
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the first statement of a block behind the closing line of a comment that starts on a line of its own
    /// stays behind the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void FirstStatementBehindTailOfCommentOnItsOwnLineStaysBehindTheComment()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     /* f
                                     */ A();
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the first statement of a switch section behind the closing line of a comment that starts behind the
    /// section's label starts the line below the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void FirstStatementBehindTailOfCommentBehindLabelStartsTheLineBelow()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     switch (a)
                                     {
                                         case 1: /* p
                                         */ A();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1: /* p
                                            */
                                                A();
                                                break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the first statement of a switch section behind the closing line of a comment that starts on a line of
    /// its own stays behind the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-COMMENT-012")]
    public void FirstStatementOfSectionBehindTailOfCommentOnItsOwnLineStaysBehindTheComment()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     switch (a)
                                     {
                                         case 2:
                                             /* q
                                             */ B();
                                             break;
                                     }
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    #endregion // Tests
}