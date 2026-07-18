using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Regression tests for issue #426: joining a line-broken assignment operator onto the target line
/// must not leave a double space once the horizontal-spacing phase adds its own separating space
/// </summary>
[TestClass]
public class AssignmentLineJoinSpacingFullPipelineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Input source with a variable-declarator initializer and a simple assignment whose
    /// operator is line-broken away from the target
    /// </summary>
    private const string TestData = """
                                    public class C
                                    {
                                        void M()
                                        {
                                            int x
                                                = 1;

                                            x
                                                = 2;
                                        }
                                    }
                                    """;

    /// <summary>
    /// Expected formatter output with the operator joined back onto the target line using a
    /// single separating space
    /// </summary>
    private const string ResultData = """
                                      public class C
                                      {
                                          void M()
                                          {
                                              int x = 1;

                                              x = 2;
                                          }
                                      }
                                      """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that joining a line-broken assignment operator onto the target line produces a
    /// single separating space instead of two
    /// </summary>
    [TestMethod]
    public void JoinedAssignmentOperatorDoesNotDoubleSpace()
    {
        AssertRuleResult(TestData, ResultData);
    }

    #endregion // Methods
}