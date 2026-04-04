using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Integration.Rules.Structural.Resources;

namespace Reihitsu.Formatter.Test.Integration.Rules.Structural;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Rules.Structural.ExpressionBodiedConstructorRule"/>
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
    /// Verifies that expression-bodied constructors are converted to block bodies.
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedConstructorsToBlockBodies()
    {
        // Arrange
        var input = TestData.ExpressionBodiedConstructorTestData;
        var expected = TestData.ExpressionBodiedConstructorResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}