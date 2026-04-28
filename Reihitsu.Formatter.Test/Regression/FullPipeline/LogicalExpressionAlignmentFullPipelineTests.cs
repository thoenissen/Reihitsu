using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — logical-expression alignment
/// </summary>
[TestClass]
public class LogicalExpressionAlignmentFullPipelineTests
{
    #region Constants

    /// <summary>
    /// Input source used for logical-expression-alignment formatting scenarios
    /// </summary>
    private const string TestData = """
                                    internal class LogicalExpressionLayoutTestData
                                    {
                                        // --- Multi-line && expression with misaligned operators ---

                                        public bool LogicalAndMisaligned(int x, int y)
                                        {
                                            return x > 0
                                                    && y > 0
                                                && x < 100;
                                        }

                                        // --- Multi-line || expression ---

                                        public bool LogicalOrMisaligned(int x, int y)
                                        {
                                            return x == 0
                                                        || y == 0
                                                || x == 100;
                                        }

                                        // --- Single-line logical expression (should stay unchanged) ---

                                        public bool SingleLineExpression(int x, int y)
                                        {
                                            return x > 0 && y > 0;
                                        }

                                        // --- Non-logical binary expression (should stay unchanged) ---

                                        public int NonLogicalExpression(int x, int y)
                                        {
                                            return x + y;
                                        }

                                        // --- Mixed && and || in multi-line ---

                                        public bool MixedLogicalOperators(int a, int b, int c)
                                        {
                                            return a > 0
                                                        && b > 0
                                                    || c > 0;
                                        }

                                        // --- Nested logical expression ---

                                        public bool NestedLogicalExpression(int x, int y, int z)
                                        {
                                            var result = x > 0
                                                            && y > 0
                                                    && z > 0;

                                            return result;
                                        }

                                        // --- Already correctly aligned ---

                                        public bool AlreadyAligned(int x, int y)
                                        {
                                            return x > 0
                                                   && y > 0;
                                        }
                                    }
                                    """;

    /// <summary>
    /// Expected formatter output for logical-expression-alignment scenarios
    /// </summary>
    private const string ResultData = """
                                      internal class LogicalExpressionLayoutTestData
                                      {
                                          // --- Multi-line && expression with misaligned operators ---

                                          public bool LogicalAndMisaligned(int x, int y)
                                          {
                                              return x > 0
                                                     && y > 0
                                                     && x < 100;
                                          }

                                          // --- Multi-line || expression ---

                                          public bool LogicalOrMisaligned(int x, int y)
                                          {
                                              return x == 0
                                                     || y == 0
                                                     || x == 100;
                                          }

                                          // --- Single-line logical expression (should stay unchanged) ---

                                          public bool SingleLineExpression(int x, int y)
                                          {
                                              return x > 0 && y > 0;
                                          }

                                          // --- Non-logical binary expression (should stay unchanged) ---

                                          public int NonLogicalExpression(int x, int y)
                                          {
                                              return x + y;
                                          }

                                          // --- Mixed && and || in multi-line ---

                                          public bool MixedLogicalOperators(int a, int b, int c)
                                          {
                                              return a > 0
                                                     && b > 0
                                                     || c > 0;
                                          }

                                          // --- Nested logical expression ---

                                          public bool NestedLogicalExpression(int x, int y, int z)
                                          {
                                              var result = x > 0
                                                           && y > 0
                                                           && z > 0;

                                              return result;
                                          }

                                          // --- Already correctly aligned ---

                                          public bool AlreadyAligned(int x, int y)
                                          {
                                              return x > 0
                                                     && y > 0;
                                          }
                                      }
                                      """;

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that logical expressions are formatted correctly
    /// </summary>
    [TestMethod]
    public void FormatsLogicalExpressionLayout()
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