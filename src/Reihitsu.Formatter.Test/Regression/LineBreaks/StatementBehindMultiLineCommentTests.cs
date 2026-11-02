using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Tests the output for a statement that stands behind the closing line of a comment that spans several lines: where a
/// blank-line requirement separates it from the statement in front, it starts the line below the comment and gets the blank
/// line in one run, in a block and in a switch section alike
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

    #endregion // Tests
}