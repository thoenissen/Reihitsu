using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.Structural;
using Reihitsu.Formatter.Test;

namespace Reihitsu.Formatter.Test.Unit.Rules.Structural;

/// <summary>
/// Unit tests for <see cref="ExpressionBodiedMethodRule"/>
/// </summary>
[TestClass]
public class ExpressionBodiedMethodRuleTests : FormatterTestsBase
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
        var input = Lf("""
            class C
            {
                void M() => Console.WriteLine();
            }
            """);
        var expected = Lf("""
            class C
            {
                void M() 
            {
            Console.WriteLine();
            }
            }
            """);

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        AssertNormalized(expected, actual);
    }

    /// <summary>
    /// Verifies that an expression-bodied returning method is converted to a block body with a return statement.
    /// </summary>
    [TestMethod]
    public void ReturningMethodConvertsToReturnStatement()
    {
        // Arrange
        var input = Lf("""
            class C
            {
                int M() => 42;
            }
            """);
        var expected = Lf("""
            class C
            {
                int M() 
            {
            return42;
            }
            }
            """);

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        AssertNormalized(expected, actual);
    }

    /// <summary>
    /// Verifies that a method with an existing block body remains unchanged.
    /// </summary>
    [TestMethod]
    public void MethodWithBlockBodyRemainsUnchanged()
    {
        // Arrange
        var input = Lf("""
            class C
            {
                int M()
                {
                    return 42;
                }
            }
            """);

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        AssertNormalized(input, actual);
    }

    /// <summary>
    /// Verifies that an async expression-bodied method is converted correctly.
    /// </summary>
    [TestMethod]
    public void AsyncMethodConvertsCorrectly()
    {
        // Arrange
        var input = Lf("""
            class C
            {
                async Task<int> M() => await Task.FromResult(1);
            }
            """);
        var expected = Lf("""
            class C
            {
                async Task<int> M() 
            {
            returnawait Task.FromResult(1);
            }
            }
            """);

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        AssertNormalized(expected, actual);
    }

    /// <summary>
    /// Verifies that method parameters are preserved during conversion.
    /// </summary>
    [TestMethod]
    public void MethodWithParametersConvertsCorrectly()
    {
        // Arrange
        var input = Lf("""
            class C
            {
                int Add(int a, int b) => a + b;
            }
            """);
        var expected = Lf("""
            class C
            {
                int Add(int a, int b) 
            {
            returna + b;
            }
            }
            """);

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        AssertNormalized(expected, actual);
    }

    /// <summary>
    /// Verifies that a generic expression-bodied method is converted correctly.
    /// </summary>
    [TestMethod]
    public void GenericMethodConvertsCorrectly()
    {
        // Arrange
        var input = Lf("""
            class C
            {
                T M<T>() => default;
            }
            """);
        var expected = Lf("""
            class C
            {
                T M<T>() 
            {
            returndefault;
            }
            }
            """);

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

        // Act
        var result = rule.Apply(tree.GetRoot(TestContext.CancellationTokenSource.Token));
        var actual = result.ToFullString();

        // Assert
        AssertNormalized(expected, actual);
    }

    /// <summary>
    /// Verifies that a static expression-bodied method is converted correctly.
    /// </summary>
    [TestMethod]
    public void StaticMethodConvertsCorrectly()
    {
        // Arrange
        var input = Lf("""
            class C
            {
                static int M() => 1;
            }
            """);
        var expected = Lf("""
            class C
            {
                static int M() 
            {
            return1;
            }
            }
            """);

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n");
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

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
        var rule = new ExpressionBodiedMethodRule(context, TestContext.CancellationTokenSource.Token);

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