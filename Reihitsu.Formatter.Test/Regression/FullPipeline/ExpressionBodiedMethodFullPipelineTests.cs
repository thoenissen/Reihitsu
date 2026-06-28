using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class ExpressionBodiedMethodFullPipelineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Input source used for expression-bodied-method formatting scenarios
    /// </summary>
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

    /// <summary>
    /// Expected formatter output for expression-bodied-method scenarios
    /// </summary>
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

    #region Methods

    /// <summary>
    /// Verifies that expression-bodied methods are converted to block bodies
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedMethodsToBlockBodies()
    {
        AssertRuleResult(TestData, ResultData);
    }

    #endregion // Methods
}