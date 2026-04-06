using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.Structural;
using Reihitsu.Formatter.Test.Unit.Rules.Base;

namespace Reihitsu.Formatter.Test.Unit.Rules.Structural;

/// <summary>
/// Unit tests for <see cref="ExpressionBodiedConstructorRule"/>
/// </summary>
[TestClass]
public class ExpressionBodiedConstructorRuleTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that an expression-bodied constructor is converted to a block body.
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedConstructorConvertsToBlockBody()
    {
        // Arrange
        var input = """
            class C
            {
                private int _x;
                C() => _x = 1;
            }
            """;
        var expected = """
            class C
            {
                private int _x;
                C()
                {
                    _x = 1;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a constructor with an existing block body remains unchanged.
    /// </summary>
    [TestMethod]
    public void ConstructorWithBlockBodyRemainsUnchanged()
    {
        // Arrange
        var input = """
            class C
            {
                private int _x;
                C()
                {
                    _x = 1;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that constructor parameters are preserved during conversion.
    /// </summary>
    [TestMethod]
    public void ConstructorWithParametersConvertsCorrectly()
    {
        // Arrange
        var input = """
            class C
            {
                private int _x;
                C(int x) => _x = x;
            }
            """;
        var expected = """
            class C
            {
                private int _x;
                C(int x)
                {
                    _x = x;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a constructor with a this initializer is converted correctly.
    /// </summary>
    [TestMethod]
    public void ConstructorWithThisOrBaseConvertsCorrectly()
    {
        // Arrange
        var input = """
            class C
            {
                private int _x;
                C(int x) : this() => _x = x;
            }
            """;
        var expected = """
            class C
            {
                private int _x;
                C(int x)
                    : this()
                {
                    _x = x;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that multiple expression-bodied constructors in the same class are all converted.
    /// </summary>
    [TestMethod]
    public void MultipleConstructorsAllConverted()
    {
        // Arrange
        var input = """
            class C
            {
                private int _x;
                C() => _x = 0;
                C(int x) => _x = x;
            }
            """;
        var expected = """
            class C
            {
                private int _x;
                C()
                {
                    _x = 0;
                }
                C(int x)
                {
                    _x = x;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the rule's Phase property returns <see cref="FormattingPhase.StructuralTransform"/>.
    /// </summary>
    [TestMethod]
    public void PhaseReturnsStructuralTransform()
    {
        // Arrange
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedConstructorRule(context, CancellationToken.None);

        // Act
        var phase = rule.Phase;

        // Assert
        Assert.AreEqual(FormattingPhase.StructuralTransform, phase);
    }

    #endregion // Methods
}