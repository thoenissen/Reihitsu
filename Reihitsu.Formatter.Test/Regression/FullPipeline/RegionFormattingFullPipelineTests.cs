using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class RegionFormattingFullPipelineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Input source used for region-formatting scenarios
    /// </summary>
    private const string TestData = """
                                    internal class RegionFormattingTestData
                                    {
                                        #region fields

                                        private int _value;

                                        #endregion

                                        #region Constructor

                                        public RegionFormattingTestData()
                                        {
                                            _value = 0;
                                        }

                                        #endregion // constructor

                                        #region methods

                                        public int GetValue()
                                        {
                                            return _value;
                                        }

                                        #endregion // Methods
                                    }
                                    """;

    /// <summary>
    /// Expected formatter output for region-formatting scenarios
    /// </summary>
    private const string ResultData = """
                                      internal class RegionFormattingTestData
                                      {
                                          #region Fields

                                          private int _value;

                                          #endregion // Fields

                                          #region Constructor

                                          public RegionFormattingTestData()
                                          {
                                              _value = 0;
                                          }

                                          #endregion // Constructor

                                          #region Methods

                                          public int GetValue()
                                          {
                                              return _value;
                                          }

                                          #endregion // Methods
                                      }
                                      """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that region directives are formatted correctly under both LF and CRLF line endings
    /// </summary>
    [TestMethod]
    public void FormatsRegionDirectives()
    {
        AssertRuleResult(TestData, ResultData);
    }

    #endregion // Methods
}