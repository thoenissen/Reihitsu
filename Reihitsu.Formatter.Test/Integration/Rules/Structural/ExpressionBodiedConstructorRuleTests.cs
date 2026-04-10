using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Integration.Rules.Structural;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class ExpressionBodiedConstructorRuleTests
{
    #region Constants

    private const string TestData = """
        internal class ExpressionBodiedConstructorTestData
        {
            private int _value;

            public ExpressionBodiedConstructorTestData() => _value = 0;

            public ExpressionBodiedConstructorTestData(int value) => _value = value;

            // Already block body — should not change
            public ExpressionBodiedConstructorTestData(string text)
            {
                _value = text.Length;
            }
        }
        """;

    private const string ResultData = """
        internal class ExpressionBodiedConstructorTestData
        {
            private int _value;

            public ExpressionBodiedConstructorTestData()
            {
                _value = 0;
            }

            public ExpressionBodiedConstructorTestData(int value)
            {
                _value = value;
            }

            // Already block body — should not change
            public ExpressionBodiedConstructorTestData(string text)
            {
                _value = text.Length;
            }
        }
        """;

    #endregion // Constants

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
        var input = TestData;
        var expected = ResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}