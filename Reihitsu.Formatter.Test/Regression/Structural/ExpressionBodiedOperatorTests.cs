using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for the conversion of expression-bodied operators to block bodies
/// </summary>
[TestClass]
public class ExpressionBodiedOperatorTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a value-returning operator is converted to a block body with a return statement
    /// </summary>
    [TestMethod]
    public void ReturningOperatorConvertsToReturnStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public static C operator +(C left, C right) => left;
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
    /// Verifies that a void compound assignment operator is converted to a block body with an expression statement, because <c>return</c> with a value does not compile in a void member
    /// </summary>
    [TestMethod]
    public void VoidCompoundAssignmentOperatorConvertsToExpressionStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int _value;

                                 public void operator +=(int amount) => _value += amount;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int _value;

                                    public void operator +=(int amount)
                                    {
                                        _value += amount;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a void instance increment operator is converted to a block body with an expression statement
    /// </summary>
    [TestMethod]
    public void VoidIncrementOperatorConvertsToExpressionStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int _value;

                                 public void operator ++() => _value++;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int _value;

                                    public void operator ++()
                                    {
                                        _value++;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a void operator inside an extension block is converted to a block body with an expression statement
    /// </summary>
    [TestMethod]
    public void VoidOperatorInExtensionBlockConvertsToExpressionStatement()
    {
        // Arrange
        const string input = """
                             static class E
                             {
                                 extension(System.Collections.Generic.List<int> list)
                                 {
                                     public void operator +=(int value) => list.Add(value);
                                 }
                             }
                             """;
        const string expected = """
                                static class E
                                {
                                    extension(System.Collections.Generic.List<int> list)
                                    {
                                        public void operator +=(int value)
                                        {
                                            list.Add(value);
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a void operator whose body is a throw expression is converted to a throw statement
    /// </summary>
    [TestMethod]
    public void VoidOperatorWithThrowExpressionConvertsToThrowStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public void operator +=(int amount) => throw new System.NotSupportedException();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public void operator +=(int amount)
                                    {
                                        throw new System.NotSupportedException();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}