using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.Structural;

namespace Reihitsu.Formatter.Test.Unit.Rules.Structural;

/// <summary>
/// Unit tests for <see cref="ExpressionBodiedConstructorRule"/>
/// </summary>
[TestClass]
public class ExpressionBodiedConstructorRuleTests
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
        const string input = "class C\n{\n    private int _x;\n    C() => _x = 1;\n}";
        const string expected = "class C\n{\n    private int _x;\n    C() \n{\n_x = 1;\n}\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedConstructorRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a constructor with an existing block body remains unchanged.
    /// </summary>
    [TestMethod]
    public void ConstructorWithBlockBodyRemainsUnchanged()
    {
        // Arrange
        const string input = "class C\n{\n    private int _x;\n    C()\n    {\n        _x = 1;\n    }\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedConstructorRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that constructor parameters are preserved during conversion.
    /// </summary>
    [TestMethod]
    public void ConstructorWithParametersConvertsCorrectly()
    {
        // Arrange
        const string input = "class C\n{\n    private int _x;\n    C(int x) => _x = x;\n}";
        const string expected = "class C\n{\n    private int _x;\n    C(int x) \n{\n_x = x;\n}\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedConstructorRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a constructor with a this initializer is converted correctly.
    /// </summary>
    [TestMethod]
    public void ConstructorWithThisOrBaseConvertsCorrectly()
    {
        // Arrange
        const string input = "class C\n{\n    private int _x;\n    C(int x) : this() => _x = x;\n}";
        const string expected = "class C\n{\n    private int _x;\n    C(int x) : this() \n{\n_x = x;\n}\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedConstructorRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that multiple expression-bodied constructors in the same class are all converted.
    /// </summary>
    [TestMethod]
    public void MultipleConstructorsAllConverted()
    {
        // Arrange
        const string input = "class C\n{\n    private int _x;\n    C() => _x = 0;\n    C(int x) => _x = x;\n}";
        const string expected = "class C\n{\n    private int _x;\n    C() \n{\n_x = 0;\n}\n    C(int x) \n{\n_x = x;\n}\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedConstructorRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
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

    #endregion // Methods
}