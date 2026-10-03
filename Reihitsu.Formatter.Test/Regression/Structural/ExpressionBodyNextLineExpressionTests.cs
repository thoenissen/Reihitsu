using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for expression bodies whose expression starts on the line after the arrow. A converted
/// <c>return</c> statement must already read <c>return expression;</c> after a single formatter pass, and the
/// gaps that carry a blank line or a comment must keep their current layout
/// </summary>
[TestClass]
public class ExpressionBodyNextLineExpressionTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a returning method whose expression is indented on the next line converts to a single-spaced return statement in one pass
    /// </summary>
    [TestMethod]
    public void ReturningMethodWithExpressionOnNextLineConvertsInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() =>
                                     42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        return 42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab-indented expression on the next line converts to a single-spaced return statement in one pass
    /// </summary>
    [TestMethod]
    public void ReturningMethodWithTabIndentedExpressionOnNextLineConvertsInOnePass()
    {
        // Arrange
        const string input = "class C\n{\n    int M() =>\n\t\t42;\n}";
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        return 42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an operator whose expression is on the next line converts to a single-spaced return statement in one pass
    /// </summary>
    [TestMethod]
    public void OperatorWithExpressionOnNextLineConvertsInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public static C operator +(C left, C right) =>
                                     left;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public static C operator +(C left, C right)
                                    {
                                        return left;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conversion operator whose expression is on the next line converts to a single-spaced return statement in one pass
    /// </summary>
    [TestMethod]
    public void ConversionOperatorWithExpressionOnNextLineConvertsInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public static implicit operator int(C value) =>
                                     0;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public static implicit operator int(C value)
                                    {
                                        return 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a returning local function whose expression is on the next line converts to a single-spaced return statement in one pass
    /// </summary>
    [TestMethod]
    public void LocalFunctionWithExpressionOnNextLineConvertsInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int Local() =>
                                         42;
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
                                            return 42;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment trailing the arrow stays on the open brace while the next-line expression joins the return keyword in one pass
    /// </summary>
    [TestMethod]
    public void LineCommentTrailingArrowWithExpressionOnNextLineConvertsInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() => // why
                                     42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {// why
                                        return 42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment ending the arrow line stays on the open brace while the next-line expression joins the return keyword in one pass
    /// </summary>
    [TestMethod]
    public void BlockCommentTrailingArrowWithExpressionOnNextLineConvertsInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() => /* why */
                                     42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {/* why */
                                        return 42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a ref-returning method whose expression is on the next line converts to a single-spaced return ref statement in one pass
    /// </summary>
    [TestMethod]
    public void RefReturningMethodWithExpressionOnNextLineConvertsInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 ref int M(int[] values) =>
                                     ref values[0];
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    ref int M(int[] values)
                                    {
                                        return ref values[0];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an awaited expression on the next line converts to a single-spaced return statement in one pass
    /// </summary>
    [TestMethod]
    public void AwaitedExpressionOnNextLineConvertsInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 async System.Threading.Tasks.Task<int> M() =>
                                     await System.Threading.Tasks.Task.FromResult(1);
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    async System.Threading.Tasks.Task<int> M()
                                    {
                                        return await System.Threading.Tasks.Task.FromResult(1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line call on the next line joins the return keyword and aligns its continuation in one pass
    /// </summary>
    [TestMethod]
    public void MultiLineCallOnNextLineConvertsInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() =>
                                     Compute(1,
                                             2);

                                 int Compute(int a, int b)
                                 {
                                     return a + b;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        return Compute(1,
                                                       2);
                                    }

                                    int Compute(int a, int b)
                                    {
                                        return a + b;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a next-line expression followed by a directive before the semicolon joins the return keyword in one pass and keeps the directive
    /// </summary>
    [TestMethod]
    public void DirectiveAfterNextLineExpressionConvertsInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() =>
                                     1
                             #pragma warning disable CS0618
                                     ;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        return 1
                                #pragma warning disable CS0618
                                        ;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a void method whose expression is on the next line keeps converting to an expression statement
    /// </summary>
    [TestMethod]
    public void VoidMethodWithExpressionOnNextLineConvertsToExpressionStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M() =>
                                     Foo();

                                 void Foo()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo();
                                    }

                                    void Foo()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a finalizer whose expression is on the next line keeps converting to an expression statement
    /// </summary>
    [TestMethod]
    public void FinalizerWithExpressionOnNextLineConvertsToExpressionStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 ~C() =>
                                     Foo();

                                 void Foo()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    ~C()
                                    {
                                        Foo();
                                    }

                                    void Foo()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a constructor whose expression is on the next line keeps converting to an expression statement
    /// </summary>
    [TestMethod]
    public void ConstructorWithExpressionOnNextLineConvertsToExpressionStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 C() =>
                                     Foo();

                                 void Foo()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    C()
                                    {
                                        Foo();
                                    }

                                    void Foo()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a throw expression on the next line keeps converting to a throw statement
    /// </summary>
    [TestMethod]
    public void ThrowExpressionOnNextLineConvertsToThrowStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() =>
                                     throw new System.Exception();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        throw new System.Exception();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line between the arrow and the expression keeps the expression on its own line below the return keyword
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeExpressionKeepsExpressionOnOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() =>

                                     42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        return
                                        42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment leading the next-line expression keeps its own line below the return keyword
    /// </summary>
    [TestMethod]
    public void BlockCommentLeadingNextLineExpressionKeepsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() =>
                                     /* c */ 42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        return

                                        /* c */ 42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment between the arrow and the expression keeps its own line below the return keyword
    /// </summary>
    [TestMethod]
    public void LineCommentBeforeNextLineExpressionKeepsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() =>
                                     // c
                                     42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        return

                                        // c
                                        42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}