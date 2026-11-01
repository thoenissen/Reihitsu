using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.SwitchCaseBraces;

/// <summary>
/// Full-pipeline tests for the layout of the comments around a switch section the formatter braces: the comments behind
/// the last label, in front of the first statement, behind the last statement, and around the terminal <c>break</c> keep
/// the layout the author gave them
/// </summary>
[TestClass]
public class BracedSwitchSectionCommentLayoutTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the reported switch: the blank line between the comments leading the first statement of a section and a
    /// comment written in front of the first statement on its line are kept when the formatter braces the sections
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    [Requirement("FMT-SWITCH-007")]
    public void SectionsOfTheReportedSwitchKeepTheirCommentLayoutWhenBraced()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             // a

                                             // b
                                             A();
                                             A();
                                             break;
                                         case 2:
                                             /* lead */ C();
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    // a

                                                    // b
                                                    A();
                                                    A();
                                                }
                                                break;

                                            case 2:
                                                {
                                                    /* lead */ C();
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line between two comments leading the first statement of a section stays inside the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    [Requirement("FMT-SWITCH-007")]
    public void BlankLineBetweenCommentsLeadingTheFirstStatementIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             // a

                                             // b
                                             A();
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    // a

                                                    // b
                                                    A();
                                                    B();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment written in front of the first statement on its line stays on that line inside the new
    /// block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    [Requirement("FMT-SWITCH-007")]
    public void CommentInFrontOfTheFirstStatementStaysOnTheStatementsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             /* lead */ A();
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    /* lead */ A();
                                                    B();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two delimited comments sharing their own line above the first statement keep that line and the space
    /// between them inside the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    [Requirement("FMT-SWITCH-007")]
    public void TwoCommentsOnTheirOwnLineAboveTheFirstStatementKeepTheSpaceBetweenThem()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             /* one */ /* two */
                                             A();
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    /* one */ /* two */
                                                    A();
                                                    B();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>/** */</c> comment written in front of the first statement on its line stays on that line inside the
    /// new block, as it does in a braced <c>if</c> body
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    [Requirement("FMT-SWITCH-007")]
    public void DocumentationBlockCommentInFrontOfTheFirstStatementStaysOnTheStatementsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             /** doc */ A();
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    /** doc */ A();
                                                    B();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment spanning two lines and ending in front of the first statement keeps the statement on
    /// its closing line inside the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    [Requirement("FMT-SWITCH-007")]
    public void MultiLineCommentEndingInFrontOfTheFirstStatementStaysOnTheStatementsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             /* multi
                                                line */ A();
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    /* multi
                                                   line */ A();
                                                    B();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two comments behind the last wrapped statement keep the space between them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    [Requirement("FMT-SWITCH-007")]
    public void TwoCommentsBehindTheLastStatementKeepTheSpaceBetweenThem()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B(); /* x */ // y
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B(); /* x */ // y
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a gap of four spaces between the last wrapped statement and the comment behind it is carried into the
    /// new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void GapInFrontOfTheCommentBehindTheLastStatementIsCarried()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();    // four
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();    // four
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment written directly behind the last wrapped statement, with no whitespace in front of it, keeps
    /// that missing gap in the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void MissingGapInFrontOfTheCommentBehindTheLastStatementStaysMissing()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();// none
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();// none
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two comments behind the last <c>case</c> label of a braced section keep the space between them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    [Requirement("FMT-SWITCH-007")]
    public void TwoCommentsBehindACaseLabelKeepTheSpaceBetweenThem()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 4: /* p */ /* q */
                                             A();
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 4: /* p */ /* q */
                                                {
                                                    A();
                                                    B();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two comments behind a <c>default</c> label of a braced section keep the space between them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    [Requirement("FMT-SWITCH-007")]
    public void TwoCommentsBehindADefaultLabelKeepTheSpaceBetweenThem()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         default: /* p */ /* q */
                                             A();
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            default: /* p */ /* q */
                                                {
                                                    A();
                                                    B();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two comments behind a pattern label with a <c>when</c> clause keep the space between them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    [Requirement("FMT-SWITCH-007")]
    public void TwoCommentsBehindAPatternLabelKeepTheSpaceBetweenThem()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object x)
                                 {
                                     switch (x)
                                     {
                                         case int n when n > 0: /* p */ /* q */
                                             A();
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object x)
                                    {
                                        switch (x)
                                        {
                                            case int n when n > 0: /* p */ /* q */
                                                {
                                                    A();
                                                    B();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two comments behind the last of two labels of a braced section keep the space between them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    [Requirement("FMT-SWITCH-007")]
    public void CommentsBehindTheLastOfTwoLabelsKeepTheSpaceBetweenThem()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                         case 2: /* p */ /* q */
                                             A();
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                            case 2: /* p */ /* q */
                                                {
                                                    A();
                                                    B();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a gap of four spaces between the last label and the comment behind it is carried when the section is
    /// braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void GapInFrontOfTheCommentBehindTheLabelIsCarried()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:    // label
                                             A();
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:    // label
                                                {
                                                    A();
                                                    B();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>///</c> comment leading the first statement moves into the new block without a blank line behind
    /// it, so the pass that braces the section already gives the final output
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-004")]
    [Requirement("FMT-PIPE-013")]
    public void SingleLineDocumentationCommentLeadingTheFirstStatementIsStableInThePassThatBracesTheSection()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 2:
                                             /// doc
                                             A();
                                             A();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 2:
                                                {
                                                    /// doc
                                                    A();
                                                    A();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two <c>///</c> lines leading the first statement move into the new block without a blank line behind
    /// them, so the pass that braces the section already gives the final output
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-004")]
    [Requirement("FMT-PIPE-013")]
    public void TwoLineDocumentationCommentLeadingTheFirstStatementIsStableInThePassThatBracesTheSection()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 2:
                                             /// doc
                                             /// more
                                             A();
                                             A();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 2:
                                                {
                                                    /// doc
                                                    /// more
                                                    A();
                                                    A();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two delimited comments written in front of the terminal <c>break</c> on its line stay on that line, with
    /// the space between them, behind the new block and its blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void TwoCommentsInFrontOfTheTerminalBreakStayOnTheBreaksLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();
                                             /* b1 */ /* b2 */ break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();
                                                }

                                                /* b1 */ /* b2 */ break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment written in front of the terminal <c>break</c> on its line stays on that line behind the
    /// new block and its blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void CommentInFrontOfTheTerminalBreakStaysOnTheBreaksLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();
                                             /* b2 */ break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();
                                                }

                                                /* b2 */ break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two comments behind the terminal <c>break</c> keep the space between them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void TwoCommentsBehindTheTerminalBreakKeepTheSpaceBetweenThem()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();
                                             break; /* x */ // y
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();
                                                }
                                                break; /* x */ // y
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a gap of four spaces between the terminal <c>break</c> and the comment behind it is carried
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void GapInFrontOfTheCommentBehindTheTerminalBreakIsCarried()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();
                                             break;    // gap
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();
                                                }
                                                break;    // gap
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of FMT-SWITCH-007 that combines the gaps in front of trailing comments, a comment in front of its
    /// statement, and the comments around the terminal <c>break</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void SpecificationExampleKeepsEveryCommentLayoutOfTheBracedSections()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:    // label
                                             /* lead */ A();
                                             B();    // four
                                             /* b1 */ /* b2 */ break; /* x */ // y

                                         case 2:
                                             C();
                                             break;    // gap
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:    // label
                                                {
                                                    /* lead */ A();
                                                    B();    // four
                                                }

                                                /* b1 */ /* b2 */ break; /* x */ // y

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;    // gap
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a switch which is the unbraced body of an <c>if</c> keeps the comment layout of its sections when both
    /// the body and the sections get braces in the same pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    [Requirement("FMT-SWITCH-007")]
    public void SwitchInAnUnbracedIfBodyKeepsTheCommentLayoutOfItsSections()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool a, int x)
                                 {
                                     if (a)
                                         switch (x)
                                         {
                                             case 1:    // label
                                                 /* one */ /* two */
                                                 A();
                                                 B(); /* x */ // y
                                                 break;

                                             case 2:
                                                 /* lead */ C();
                                                 break;
                                         }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a, int x)
                                    {
                                        if (a)
                                        {
                                            switch (x)
                                            {
                                                case 1:    // label
                                                    {
                                                        /* one */ /* two */
                                                        A();
                                                        B(); /* x */ // y
                                                    }
                                                    break;

                                                case 2:
                                                    {
                                                        /* lead */ C();
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that braced sections that already carry these comment layouts are left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void AlreadyBracedSectionsWithTheseCommentLayoutsStayUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:    // label
                                             {
                                                 // a

                                                 // b
                                                 /* lead */ A();
                                                 B();    // four
                                             }

                                             /* b1 */ /* b2 */ break; /* x */ // y

                                         case 2: /* p */ /* q */
                                             {
                                                 /* one */ /* two */
                                                 C();// none
                                             }
                                             break;    // gap
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a section carrying a directive stays unbraced with its comments as written, while the other section of
    /// the switch is braced and keeps the layout of its comments
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-010")]
    [Requirement("FMT-SWITCH-007")]
    public void SectionWithADirectiveKeepsItsCommentsUnbracedWhileTheOtherSectionIsBracedWithItsLayout()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                             #if DEBUG
                                             A();
                             #endif
                                             /* lead */ B();    // four
                                             break;

                                         case 2:
                                             /* lead */ C();
                                             D();    // four
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                #if DEBUG
                                                A();
                                #endif
                                                /* lead */ B();    // four
                                                break;

                                            case 2:
                                                {
                                                    /* lead */ C();
                                                    D();    // four
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies FMT-SWITCH-007's example for the blank line behind the last leading comment: it is kept at the first
    /// statement, inside the new block, and at the terminal <c>break</c>, behind the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void SpecificationExampleKeepsTheBlankLineBehindTheLastLeadingCommentAtTheFirstStatementAndTheTerminalBreak()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             // a

                                             A();
                                             B();
                                             // b

                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    // a

                                                    A();
                                                    B();
                                                }

                                                // b

                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line between the <c>//</c> comment that leads the first statement and that statement is carried
    /// into the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void BlankLineBetweenTheLastLineCommentAndTheFirstStatementIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             // a

                                             A();
                                             B();
                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    // a

                                                    A();
                                                    B();
                                                }
                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line between the <c>//</c> comment that leads the terminal <c>break</c> and the <c>break</c> is
    /// kept behind the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void BlankLineBetweenTheLastLineCommentAndTheTerminalBreakIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();
                                             // b

                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();
                                                }

                                                // b

                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line between the <c>/* */</c> comment that leads the first statement and that statement is
    /// carried into the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void BlankLineBetweenTheLastBlockCommentAndTheFirstStatementIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             /* a */

                                             A();
                                             B();
                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    /* a */

                                                    A();
                                                    B();
                                                }
                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line between the <c>/* */</c> comment that leads the terminal <c>break</c> and the <c>break</c>
    /// is kept behind the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void BlankLineBetweenTheLastBlockCommentAndTheTerminalBreakIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();
                                             /* b */

                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();
                                                }

                                                /* b */

                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several blank lines between the comment that leads the first statement and that statement reach the
    /// new block as the one blank line the wrap finds after the earlier phases collapsed them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    [Requirement("FMT-BLANK-014")]
    public void SeveralBlankLinesBetweenTheLastCommentAndTheFirstStatementAreCarriedAsOne()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             // a



                                             A();
                                             B();
                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    // a

                                                    A();
                                                    B();
                                                }
                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several blank lines between the comment that leads the terminal <c>break</c> and the <c>break</c> are
    /// kept as the one blank line the wrap finds after the earlier phases collapsed them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    [Requirement("FMT-BLANK-014")]
    public void SeveralBlankLinesBetweenTheLastCommentAndTheTerminalBreakAreCarriedAsOne()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();
                                             // b



                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();
                                                }

                                                // b

                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between two comments that lead the first statement and the blank line behind the last
    /// of them are both carried into the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void BlankLinesBetweenAndBehindTheCommentsLeadingTheFirstStatementAreKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             // a

                                             // b

                                             A();
                                             B();
                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    // a

                                                    // b

                                                    A();
                                                    B();
                                                }
                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between two comments that lead the terminal <c>break</c> and the blank line behind the
    /// last of them are both kept behind the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void BlankLinesBetweenAndBehindTheCommentsLeadingTheTerminalBreakAreKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();
                                             // b1

                                             // b2

                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();
                                                }

                                                // b1

                                                // b2

                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line behind a <c>///</c> comment that leads the first statement is gone before the wrap, so the
    /// new block holds the comment directly above the statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    [Requirement("FMT-BLANK-010")]
    public void BlankLineBehindADocumentationCommentLeadingTheFirstStatementIsNotCarried()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             /// doc

                                             A();
                                             B();
                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    /// doc
                                                    A();
                                                    B();
                                                }
                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line behind a <c>///</c> comment that leads the terminal <c>break</c> is gone before the wrap, so
    /// the comment stays directly above the <c>break</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    [Requirement("FMT-BLANK-010")]
    public void BlankLineBehindADocumentationCommentLeadingTheTerminalBreakIsNotCarried()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();
                                             /// doc

                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();
                                                }

                                                /// doc
                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a section with a single statement keeps the blank line between the comment that leads it and the
    /// statement, together with the gap in front of the comment behind it, when it is braced with the rest of the switch
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void BlankLineBehindTheCommentLeadingTheOnlyStatementOfASectionIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();
                                             break;

                                         case 3:
                                             // only comment

                                             A();    /* x */
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();
                                                }
                                                break;

                                            case 3:
                                                {
                                                    // only comment

                                                    A();    /* x */
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}