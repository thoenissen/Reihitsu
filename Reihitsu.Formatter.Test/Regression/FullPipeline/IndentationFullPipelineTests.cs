using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class IndentationFullPipelineTests
{
    #region Constants

    private const string TestData = """
        internal class IndentationTestData
        {
          public void Method()
          {
              var x = 1;

                    if (x == 1)
          {
                    x = 2;
          }
          }
        }
        """;

    private const string ResultData = """
        internal class IndentationTestData
        {
            public void Method()
            {
                var x = 1;

                if (x == 1)
                {
                    x = 2;
                }
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
    /// Verifies that indentation is normalized correctly.
    /// </summary>
    [TestMethod]
    public void NormalizesIndentation()
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