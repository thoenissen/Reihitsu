using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class BlankLineAfterStatementFullPipelineTests
{
    #region Constants

    /// <summary>
    /// Input source used for blank-line-after-statement formatting scenarios
    /// </summary>
    private const string TestData = """
                                    internal class BlankLineAfterStatementTestData
                                    {
                                        public void SwitchWithBreakFollowedByCase()
                                        {
                                            switch (1)
                                            {
                                                case 1:
                                                    System.Console.WriteLine();
                                                    break;
                                                case 2:
                                                    System.Console.WriteLine();
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }

                                        public void BreakInLoopFollowedByStatement()
                                        {
                                            for (var i = 0; i < 10; i++)
                                            {
                                                break;
                                                var x = 1;
                                            }
                                        }

                                        // --- Cases that should NOT be modified ---

                                        public void BreakLastInBlock()
                                        {
                                            while (true)
                                            {
                                                break;
                                            }
                                        }

                                        public void BreakAlreadyFollowedByBlankLine()
                                        {
                                            for (var i = 0; i < 10; i++)
                                            {
                                                break;

                                                var x = 1;
                                            }
                                        }

                                        public void SwitchBreakLastInSection()
                                        {
                                            switch (1)
                                            {
                                                case 1:
                                                    break;
                                            }
                                        }
                                    }
                                    """;

    /// <summary>
    /// Expected formatter output for blank-line-after-statement scenarios
    /// </summary>
    private const string ResultData = """
                                      internal class BlankLineAfterStatementTestData
                                      {
                                          public void SwitchWithBreakFollowedByCase()
                                          {
                                              switch (1)
                                              {
                                                  case 1:
                                                      System.Console.WriteLine();
                                                      break;

                                                  case 2:
                                                      System.Console.WriteLine();
                                                      break;

                                                  default:
                                                      break;
                                              }
                                          }

                                          public void BreakInLoopFollowedByStatement()
                                          {
                                              for (var i = 0; i < 10; i++)
                                              {
                                                  break;

                                                  var x = 1;
                                              }
                                          }

                                          // --- Cases that should NOT be modified ---

                                          public void BreakLastInBlock()
                                          {
                                              while (true)
                                              {
                                                  break;
                                              }
                                          }

                                          public void BreakAlreadyFollowedByBlankLine()
                                          {
                                              for (var i = 0; i < 10; i++)
                                              {
                                                  break;

                                                  var x = 1;
                                              }
                                          }

                                          public void SwitchBreakLastInSection()
                                          {
                                              switch (1)
                                              {
                                                  case 1:
                                                      break;
                                              }
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
    /// Verifies that the formatter inserts blank lines after break statements where required
    /// </summary>
    [TestMethod]
    public void InsertsBlankLinesAfterBreakStatements()
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