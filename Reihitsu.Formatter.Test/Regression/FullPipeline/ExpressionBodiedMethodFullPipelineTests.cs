using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class ExpressionBodiedMethodFullPipelineTests
{
    #region Constants

    private const string TestData = """
        internal class ExpressionBodiedMethodTestData
        {
            public int GetValue() => 42;

            public void DoWork() => System.Console.WriteLine("hello");

            public string GetName() => "test";

            // Already block body — should not change
            public int GetOther()
            {
                return 1;
            }
        }
        """;

    private const string ResultData = """
        internal class ExpressionBodiedMethodTestData
        {
            public int GetValue()
            {
                return 42;
            }

            public void DoWork()
            {
                System.Console.WriteLine("hello");
            }

            public string GetName()
            {
                return "test";
            }

            // Already block body — should not change
            public int GetOther()
            {
                return 1;
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
    /// Verifies that expression-bodied methods are converted to block bodies.
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedMethodsToBlockBodies()
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