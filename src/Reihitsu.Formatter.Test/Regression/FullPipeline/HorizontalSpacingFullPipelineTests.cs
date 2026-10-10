using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class HorizontalSpacingFullPipelineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Input source used for horizontal-spacing formatting scenarios
    /// </summary>
    private const string TestData = """
                                    internal class HorizontalSpacingTestData
                                    {
                                        public void Method()
                                        {
                                            var x=1;
                                            var y = x+2;
                                            var z = x  +  y;
                                            var list = new int[] { 1,2,3 };

                                            if(x == 1)
                                            {
                                                System.Console.WriteLine( x );
                                            }

                                            for (var i=0; i<10; i++)
                                            {
                                            }
                                        }
                                    }
                                    """;

    /// <summary>
    /// Expected formatter output for horizontal-spacing scenarios
    /// </summary>
    private const string ResultData = """
                                      internal class HorizontalSpacingTestData
                                      {
                                          public void Method()
                                          {
                                              var x = 1;
                                              var y = x + 2;
                                              var z = x + y;
                                              var list = new int[] { 1, 2, 3 };

                                              if (x == 1)
                                              {
                                                  System.Console.WriteLine(x);
                                              }

                                              for (var i = 0; i < 10; i++)
                                              {
                                              }
                                          }
                                      }
                                      """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that horizontal spacing is normalized correctly
    /// </summary>
    [TestMethod]
    public void NormalizesHorizontalSpacing()
    {
        AssertRuleResult(TestData, ResultData);
    }

    #endregion // Methods
}