using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class TrailingTriviaCleanupFullPipelineTests
{
    #region Constants

    private const string TestData = """
                                    internal class TrailingTriviaCleanupTestData   
                                    {
                                        public void Method()   
                                        {



                                            var x = 1;
                                        }
                                    }


                                    """;

    private const string ResultData = """
                                      internal class TrailingTriviaCleanupTestData
                                      {
                                          public void Method()
                                          {
                                              var x = 1;
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
    /// Verifies that trailing trivia and blank lines are cleaned up.
    /// </summary>
    [TestMethod]
    public void CleansTrailingTriviaAndBlankLines()
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