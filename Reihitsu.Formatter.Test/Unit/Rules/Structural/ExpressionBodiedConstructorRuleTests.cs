using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.Structural;
using Reihitsu.Formatter.Test;

namespace Reihitsu.Formatter.Test.Unit.Rules.Structural;

/// <summary>
/// Unit tests for <see cref="ExpressionBodiedConstructorRule"/>
/// </summary>
[TestClass]
public class ExpressionBodiedConstructorRuleTests : FormatterTestsBase
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that an expression-bodied constructor is converted to a block body.
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedConstructorConvertsToBlockBody()
    {
        // Arrange
        var input = Lf("""
            class C
            {
                private int _x;
                C() => _x = 1;
            }
            """);
        var expected = Lf("""
            class C
            {
                private int _x;
                C() 
            {
            _x = 1;
            }
            }
            """);

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedConstructorRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        AssertNormalized(expected, actual);
    }

    /// <summary>
    /// Verifies that a constructor with an existing block body remains unchanged.
    /// </summary>
    [TestMethod]
    public void ConstructorWithBlockBodyRemainsUnchanged()
    {
        // Arrange
        var input = Lf("""
            class C
            {
                private int _x;
                C()
                {
                    _x = 1;
                }
            }
            """);

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedConstructorRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        AssertNormalized(input, actual);
    }

    /// <summary>
    /// Verifies that constructor parameters are preserved during conversion.
    /// </summary>
    [TestMethod]
    public void ConstructorWithParametersConvertsCorrectly()
    {
        // Arrange
        var input = Lf("""
            class C
            {
                private int _x;
                C(int x) => _x = x;
            }
            """);
        var expected = Lf("""
            class C
            {
                private int _x;
                C(int x) 
            {
            _x = x;
            }
            }
            """);

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedConstructorRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        AssertNormalized(expected, actual);
    }

    /// <summary>
    /// Verifies that a constructor with a this initializer is converted correctly.
    /// </summary>
    [TestMethod]
    public void ConstructorWithThisOrBaseConvertsCorrectly()
    {
        // Arrange
        var input = Lf("""
            class C
            {
                private int _x;
                C(int x) : this() => _x = x;
            }
            """);
        var expected = Lf("""
            class C
            {
                private int _x;
                C(int x) : this() 
            {
            _x = x;
            }
            }
            """);

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedConstructorRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        AssertNormalized(expected, actual);
    }

    /// <summary>
    /// Verifies that multiple expression-bodied constructors in the same class are all converted.
    /// </summary>
    [TestMethod]
    public void MultipleConstructorsAllConverted()
    {
        // Arrange
        var input = Lf("""
            class C
            {
                private int _x;
                C() => _x = 0;
                C(int x) => _x = x;
            }
            """);
        var expected = Lf("""
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
            """);

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedConstructorRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        AssertNormalized(expected, actual);
    }

    /// <summary>
    /// Verifies that the rule's Phase property returns <see cref="FormattingPhase.StructuralTransform"/>.
    /// </summary>
    [TestMethod]
    public void PhaseReturnsStructuralTransform()
    {
        // Arrange
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedConstructorRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var phase = rule.Phase;

        // Assert
        Assert.AreEqual(FormattingPhase.StructuralTransform, phase);
    }

    /// <summary>
    /// Normalizes line endings in the provided text to LF.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <returns>The text with LF line endings.</returns>
    private static string Lf(string text)
    {
        return text.Replace("\r\n", "\n");
    }

    #endregion // Methods
}