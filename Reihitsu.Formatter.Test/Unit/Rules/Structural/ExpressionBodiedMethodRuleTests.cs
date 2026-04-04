using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.Structural;

namespace Reihitsu.Formatter.Test.Unit.Rules.Structural;

/// <summary>
/// Unit tests for <see cref="ExpressionBodiedMethodRule"/>
/// </summary>
[TestClass]
public class ExpressionBodiedMethodRuleTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that an expression-bodied void method is converted to a block body with an expression statement.
    /// </summary>
    [TestMethod]
    public void VoidMethodConvertsToExpressionStatement()
    {
        // Arrange
        const string input = "class C\n{\n    void M() => Console.WriteLine();\n}";
        const string expected = "class C\n{\n    void M() \n{\nConsole.WriteLine();\n}\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied returning method is converted to a block body with a return statement.
    /// </summary>
    [TestMethod]
    public void ReturningMethodConvertsToReturnStatement()
    {
        // Arrange
        const string input = "class C\n{\n    int M() => 42;\n}";
        const string expected = "class C\n{\n    int M() \n{\nreturn42;\n}\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a method with an existing block body remains unchanged.
    /// </summary>
    [TestMethod]
    public void MethodWithBlockBodyRemainsUnchanged()
    {
        // Arrange
        const string input = "class C\n{\n    int M()\n    {\n        return 42;\n    }\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that an async expression-bodied method is converted correctly.
    /// </summary>
    [TestMethod]
    public void AsyncMethodConvertsCorrectly()
    {
        // Arrange
        const string input = "class C\n{\n    async Task<int> M() => await Task.FromResult(1);\n}";
        const string expected = "class C\n{\n    async Task<int> M() \n{\nreturnawait Task.FromResult(1);\n}\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that method parameters are preserved during conversion.
    /// </summary>
    [TestMethod]
    public void MethodWithParametersConvertsCorrectly()
    {
        // Arrange
        const string input = "class C\n{\n    int Add(int a, int b) => a + b;\n}";
        const string expected = "class C\n{\n    int Add(int a, int b) \n{\nreturna + b;\n}\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a generic expression-bodied method is converted correctly.
    /// </summary>
    [TestMethod]
    public void GenericMethodConvertsCorrectly()
    {
        // Arrange
        const string input = "class C\n{\n    T M<T>() => default;\n}";
        const string expected = "class C\n{\n    T M<T>() \n{\nreturndefault;\n}\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a static expression-bodied method is converted correctly.
    /// </summary>
    [TestMethod]
    public void StaticMethodConvertsCorrectly()
    {
        // Arrange
        const string input = "class C\n{\n    static int M() => 1;\n}";
        const string expected = "class C\n{\n    static int M() \n{\nreturn1;\n}\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

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
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var phase = rule.Phase;

        // Assert
        Assert.AreEqual(FormattingPhase.StructuralTransform, phase);
    }

    #endregion // Methods
}