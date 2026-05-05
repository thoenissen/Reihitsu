using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Unit tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class ExpressionBodiedLocalFunctionTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that an expression-bodied void local function is converted to a block body with an expression statement
    /// </summary>
    [TestMethod]
    public void VoidLocalFunctionConvertsToExpressionStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     void Local() => Console.WriteLine();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        void Local()
                                        {
                                            Console.WriteLine();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an expression-bodied returning local function is converted to a block body with a return statement
    /// </summary>
    [TestMethod]
    public void ReturningLocalFunctionConvertsToReturnStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int Local() => 42;
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
    /// Verifies that a local function with an existing block body remains unchanged
    /// </summary>
    [TestMethod]
    public void LocalFunctionWithBlockBodyRemainsUnchanged()
    {
        // Arrange
        const string input = """
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
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an async expression-bodied local function is converted correctly
    /// </summary>
    [TestMethod]
    public void AsyncLocalFunctionConvertsCorrectly()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     async Task<int> Local() => await Task.FromResult(1);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        async Task<int> Local()
                                        {
                                            return await Task.FromResult(1);
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an async Task expression-bodied local function is converted to a block body with an expression statement
    /// </summary>
    [TestMethod]
    public void AsyncTaskLocalFunctionConvertsToExpressionStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     async Task DoWorkAsync() => await Task.CompletedTask;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        async Task DoWorkAsync()
                                        {
                                            await Task.CompletedTask;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that local-function parameters are preserved during conversion
    /// </summary>
    [TestMethod]
    public void LocalFunctionWithParametersConvertsCorrectly()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int Add(int a, int b) => a + b;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int Add(int a, int b)
                                        {
                                            return a + b;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a generic expression-bodied local function is converted correctly
    /// </summary>
    [TestMethod]
    public void GenericLocalFunctionConvertsCorrectly()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     T Local<T>() => default;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        T Local<T>()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a static expression-bodied local function is converted correctly
    /// </summary>
    [TestMethod]
    public void StaticLocalFunctionConvertsCorrectly()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     static int Local() => 1;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        static int Local()
                                        {
                                            return 1;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}