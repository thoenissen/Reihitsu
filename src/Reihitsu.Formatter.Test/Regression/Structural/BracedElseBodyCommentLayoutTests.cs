using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Full-pipeline tests for the layout of the comments an <c>else</c> body carries when the formatter braces it
/// </summary>
[TestClass]
public class BracedElseBodyCommentLayoutTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that a blank line between two comments leading the statement of an <c>else</c> body stays inside the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    public void BlankLineBetweenCommentsLeadingAnElseBodyIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                     {
                                         A();
                                     }
                                     else
                                         // a

                                         // b
                                         B();
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
                                            A();
                                        }
                                        else
                                        {
                                            // a

                                            // b
                                            B();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment written in front of the statement of an <c>else</c> body on its line stays on that
    /// line inside the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    public void CommentInFrontOfAnElseBodyStaysOnTheStatementsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                     {
                                         A();
                                     }
                                     else
                                         /* lead */ B();
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
                                            A();
                                        }
                                        else
                                        {
                                            /* lead */ B();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two delimited comments sharing their own line above the statement of an <c>else</c> body keep that line
    /// and the space between them inside the new block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-013")]
    public void TwoCommentsOnTheirOwnLineAboveAnElseBodyKeepTheSpaceBetweenThem()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                     {
                                         A();
                                     }
                                     else
                                         /* one */ /* two */
                                         B();
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
                                            A();
                                        }
                                        else
                                        {
                                            /* one */ /* two */
                                            B();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}