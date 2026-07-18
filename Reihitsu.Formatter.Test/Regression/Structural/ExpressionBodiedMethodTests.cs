using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Unit tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class ExpressionBodiedMethodTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that an expression-bodied void method is converted to a block body with an expression statement
    /// </summary>
    [TestMethod]
    public void VoidMethodConvertsToExpressionStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M() => Console.WriteLine();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Console.WriteLine();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an expression-bodied returning method is converted to a block body with a return statement
    /// </summary>
    [TestMethod]
    public void ReturningMethodConvertsToReturnStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() => 42;
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
    /// Verifies that a method with an existing block body remains unchanged
    /// </summary>
    [TestMethod]
    public void MethodWithBlockBodyRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M()
                                 {
                                     return 42;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an async expression-bodied method is converted correctly
    /// </summary>
    [TestMethod]
    public void AsyncMethodConvertsCorrectly()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 async Task<int> M() => await Task.FromResult(1);
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    async Task<int> M()
                                    {
                                        return await Task.FromResult(1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an async Task expression-bodied method is converted to a block body with an expression statement
    /// </summary>
    [TestMethod]
    public void AsyncTaskMethodConvertsToExpressionStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 async Task DoWorkAsync() => await Task.CompletedTask;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    async Task DoWorkAsync()
                                    {
                                        await Task.CompletedTask;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that method parameters are preserved during conversion
    /// </summary>
    [TestMethod]
    public void MethodWithParametersConvertsCorrectly()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Add(int a, int b) => a + b;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Add(int a, int b)
                                    {
                                        return a + b;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a generic expression-bodied method is converted correctly
    /// </summary>
    [TestMethod]
    public void GenericMethodConvertsCorrectly()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 T M<T>() => default;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    T M<T>()
                                    {
                                        return default;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an expression-bodied method throwing an exception is converted to a throw statement (not <c>return throw</c>)
    /// </summary>
    [TestMethod]
    public void ThrowingMethodConvertsToThrowStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() => throw new System.Exception();
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
    /// Verifies that an async ValueTask expression-bodied method is converted to a block body with an expression statement
    /// </summary>
    [TestMethod]
    public void AsyncValueTaskMethodConvertsToExpressionStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 async ValueTask DoWorkAsync() => await Task.CompletedTask;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    async ValueTask DoWorkAsync()
                                    {
                                        await Task.CompletedTask;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an async ValueTask{T} expression-bodied method keeps the return statement
    /// </summary>
    [TestMethod]
    public void AsyncGenericValueTaskMethodConvertsToReturnStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 async ValueTask<int> M() => await Task.FromResult(1);
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    async ValueTask<int> M()
                                    {
                                        return await Task.FromResult(1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment placed before the arrow token is preserved during conversion
    /// </summary>
    [TestMethod]
    public void PreservesCommentBeforeArrow()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M()
                                     // note
                                     => 42;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    // note
                                    {
                                        return 42;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment trailing the arrow token is preserved during conversion instead of being
    /// silently dropped (issue #422). The expression is intentionally flush left: an indented multi-line
    /// expression body collapses onto the <c>return</c> keyword with extra whitespace regardless of any
    /// comment, a separate, pre-existing formatter gap this test does not exercise
    /// </summary>
    [TestMethod]
    public void PreservesCommentAfterArrow()
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
    /// Verifies that a comment placed before the semicolon token is preserved during conversion instead of
    /// being silently dropped (issue #422)
    /// </summary>
    [TestMethod]
    public void PreservesCommentBeforeSemicolon()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() => 42
                                     // why
                                     ;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        return 42

                                        // why
                                        ;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a static expression-bodied method is converted correctly
    /// </summary>
    [TestMethod]
    public void StaticMethodConvertsCorrectly()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 static int M() => 1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    static int M()
                                    {
                                        return 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}