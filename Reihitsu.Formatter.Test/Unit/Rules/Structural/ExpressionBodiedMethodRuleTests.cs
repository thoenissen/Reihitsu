using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Unit.Rules.Base;

namespace Reihitsu.Formatter.Test.Unit.Rules.Structural;

/// <summary>
/// Unit tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class ExpressionBodiedMethodRuleTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that an expression-bodied void method is converted to a block body with an expression statement.
    /// </summary>
    [TestMethod]
    public void VoidMethodConvertsToExpressionStatement()
    {
        // Arrange
        var input = """
            class C
            {
                void M() => Console.WriteLine();
            }
            """;
        var expected = """
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
    /// Verifies that an expression-bodied returning method is converted to a block body with a return statement.
    /// </summary>
    [TestMethod]
    public void ReturningMethodConvertsToReturnStatement()
    {
        // Arrange
        var input = """
            class C
            {
                int M() => 42;
            }
            """;
        var expected = """
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
    /// Verifies that a method with an existing block body remains unchanged.
    /// </summary>
    [TestMethod]
    public void MethodWithBlockBodyRemainsUnchanged()
    {
        // Arrange
        var input = """
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
    /// Verifies that an async expression-bodied method is converted correctly.
    /// </summary>
    [TestMethod]
    public void AsyncMethodConvertsCorrectly()
    {
        // Arrange
        var input = """
            class C
            {
                async Task<int> M() => await Task.FromResult(1);
            }
            """;
        var expected = """
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
    /// Verifies that method parameters are preserved during conversion.
    /// </summary>
    [TestMethod]
    public void MethodWithParametersConvertsCorrectly()
    {
        // Arrange
        var input = """
            class C
            {
                int Add(int a, int b) => a + b;
            }
            """;
        var expected = """
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
    /// Verifies that a generic expression-bodied method is converted correctly.
    /// </summary>
    [TestMethod]
    public void GenericMethodConvertsCorrectly()
    {
        // Arrange
        var input = """
            class C
            {
                T M<T>() => default;
            }
            """;
        var expected = """
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
    /// Verifies that a static expression-bodied method is converted correctly.
    /// </summary>
    [TestMethod]
    public void StaticMethodConvertsCorrectly()
    {
        // Arrange
        var input = """
            class C
            {
                static int M() => 1;
            }
            """;
        var expected = """
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