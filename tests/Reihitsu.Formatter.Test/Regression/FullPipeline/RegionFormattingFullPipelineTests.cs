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

    /// <summary>
    /// Verifies that an inactive region inside an accessor list retains the existing naming behavior
    /// </summary>
    [TestMethod]
    public void InactiveRegionInsideAccessorListIsStillNamed()
    {
        // Arrange
        const string input = """
                             internal class T
                             {
                                 public int V
                                 {
                             #if false
                             #region lower
                             #endregion
                             #endif
                                     get;
                                     set;
                                 }
                             }
                             """;
        const string expected = """
                                internal class T
                                {
                                    public int V
                                    {
                                #if false
                                #region Lower
                                #endregion // Lower
                                #endif
                                        get;
                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region between the arrow of a lambda and its expression refuses the input as unsupported: the input is
    /// returned unchanged, and its <c>#region</c> is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    public void RegionBehindTheArrowOfALambdaIsUnsupported()
    {
        // Arrange
        const string input = """
                             internal class T
                             {
                                 public int M()
                                 {
                                     System.Func<int> value = () =>
                             #region inner
                                     1
                             #endregion
                                     ;

                                     return value();
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(LambdaArrowCommentReason, input, "#region inner");
    }

    #endregion // Methods
}