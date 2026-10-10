using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class TrailingTriviaCleanupFullPipelineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Input source used for trailing-trivia-cleanup formatting scenarios
    /// </summary>
    private const string TestData = """
                                    internal class TrailingTriviaCleanupTestData   
                                    {
                                        public void Method()   
                                        {



                                            var x = 1;
                                        }
                                    }


                                    """;

    /// <summary>
    /// Expected formatter output for trailing-trivia-cleanup scenarios
    /// </summary>
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

    #region Methods

    /// <summary>
    /// Verifies that trailing trivia and blank lines are cleaned up
    /// </summary>
    [TestMethod]
    public void CleansTrailingTriviaAndBlankLines()
    {
        AssertRuleResult(TestData, ResultData);
    }

    #endregion // Methods
}