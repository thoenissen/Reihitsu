using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class IndentationFullPipelineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Input source used for indentation formatting scenarios
    /// </summary>
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

    /// <summary>
    /// Expected formatter output for indentation scenarios
    /// </summary>
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

    #region Methods

    /// <summary>
    /// Verifies that indentation is normalized correctly
    /// </summary>
    [TestMethod]
    public void NormalizesIndentation()
    {
        AssertRuleResult(TestData, ResultData);
    }

    #endregion // Methods
}