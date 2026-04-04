using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Integration.Rules.Structural.Resources;

namespace Reihitsu.Formatter.Test.Integration.Rules.Structural;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Rules.Structural.ExpressionBodiedMethodRule"/>
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
    /// Verifies that expression-bodied methods are converted to block bodies.
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedMethodsToBlockBodies()
    {
        // Arrange
        var input = TestData.ExpressionBodiedMethodTestData;
        var expected = TestData.ExpressionBodiedMethodResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}