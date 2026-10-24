using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Full-pipeline tests for comments on lines of their own around the arrow of an expression body the
/// formatter converts: they lead the created statement inside the block, and the expression joins the
/// created <c>return</c> keyword
/// </summary>
[TestClass]
public class ExpressionBodyArrowCommentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the specified example: comments on lines of their own around the arrow lead the created
    /// statement, and a comment in front of the expression on its line stays in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentsAroundTheArrowLeadTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Other()
                                 // before arrow
                                 => 1;

                                 int Get() =>
                                     // after arrow
                                     42;

                                 void Run() =>
                                     // run
                                     Foo();

                                 int Value() =>
                                     /* c */ 42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Other()
                                    {
                                        // before arrow
                                        return 1;
                                    }

                                    int Get()
                                    {
                                        // after arrow
                                        return 42;
                                    }

                                    void Run()
                                    {
                                        // run
                                        Foo();
                                    }

                                    int Value()
                                    {
                                        return /* c */ 42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the reported shape: a comment on its own line before the arrow is carried into the block
    /// instead of standing between the signature and the opening brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBeforeArrowLeadsTheCreatedReturnStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Other()
                                 // before arrow
                                 => 1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Other()
                                    {
                                        // before arrow
                                        return 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the reported shape: a comment on its own line after the arrow leads the created
    /// statement, and <c>return 42;</c> stays on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentAfterArrowLeadsTheCreatedReturnStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Get() =>
                                 // c
                                 42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Get()
                                    {
                                        // c
                                        return 42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment on its own line after the arrow leads a created expression statement on a
    /// line of its own instead of being joined to the opening brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentAfterArrowLeadsTheCreatedExpressionStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void Run() =>
                                     // run
                                     Foo();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void Run()
                                    {
                                        // run
                                        Foo();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment on its own line after the arrow leads a created <c>throw</c> statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentAfterArrowLeadsTheCreatedThrowStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Fail() =>
                                     // never
                                     throw new System.InvalidOperationException();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Fail()
                                    {
                                        // never
                                        throw new System.InvalidOperationException();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment that starts the line after the arrow and is followed by the expression
    /// stays in front of the expression, which joins the created <c>return</c> keyword with it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentInFrontOfExpressionOnItsLineJoinsTheReturnKeyword()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Value() =>
                                     /* c */ 42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Value()
                                    {
                                        return /* c */ 42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that comments before and after the arrow both lead the created statement, in the order
    /// they were written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentsBeforeAndAfterArrowLeadTheStatementInSourceOrder()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Both()
                                     // before
                                     =>
                                     // after
                                     42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Both()
                                    {
                                        // before
                                        // after
                                        return 42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between two comments after the arrow is kept in front of the created
    /// statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    [Requirement("FMT-PIPE-013")]
    public void BlankLineBetweenTwoCommentsAfterArrowIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Two() =>
                                     // a

                                     // b
                                     42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Two()
                                    {
                                        // a

                                        // b
                                        return 42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment spanning several lines before the arrow leads the created
    /// statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void MultiLineDelimitedCommentBeforeArrowLeadsTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Multi()
                                     /* first
                                        second */
                                     => 1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Multi()
                                    {
                                        /* first
                                           second */
                                        return 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment before the arrow of a local function leads the created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBeforeArrowOfLocalFunctionLeadsTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int Local()
                                         // note
                                         => 1;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int Local()
                                        {
                                            // note
                                            return 1;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment after the arrow of a local function leads the created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentAfterArrowOfLocalFunctionLeadsTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int Local() =>
                                         // note
                                         1;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int Local()
                                        {
                                            // note
                                            return 1;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment before the arrow of a finalizer leads the created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBeforeArrowOfFinalizerLeadsTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 ~C()
                                     // note
                                     => Release();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    ~C()
                                    {
                                        // note
                                        Release();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment before the arrow of an operator leads the created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBeforeArrowOfOperatorLeadsTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public static C operator +(C a, C b)
                                     // note
                                     => a;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public static C operator +(C a, C b)
                                    {
                                        // note
                                        return a;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment before the arrow of a conversion operator leads the created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBeforeArrowOfConversionOperatorLeadsTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public static implicit operator int(C c)
                                     // note
                                     => 1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public static implicit operator int(C c)
                                    {
                                        // note
                                        return 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment between a constraint clause and the arrow leads the created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void CommentBeforeArrowOfMethodWithConstraintClauseLeadsTheCreatedStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 T Get<T>()
                                     where T : new()
                                     // note
                                     => new T();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    T Get<T>()
                                        where T : new()
                                    {
                                        // note
                                        return new T();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}