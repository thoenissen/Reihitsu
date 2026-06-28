using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class ExpressionBodiedConstructorFullPipelineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Input source used for expression-bodied-constructor formatting scenarios
    /// </summary>
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

    /// <summary>
    /// Expected formatter output for expression-bodied-constructor scenarios
    /// </summary>
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

    #region Methods

    /// <summary>
    /// Verifies that expression-bodied constructors are converted to block bodies
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedConstructorsToBlockBodies()
    {
        AssertRuleResult(TestData, ResultData);
    }

    #endregion // Methods
}