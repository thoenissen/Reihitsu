using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer"/> and <see cref="RH5206SwitchExpressionBracesShouldBeAnchoredCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzerTests : AnalyzerTestsBase<RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer, RH5206SwitchExpressionBracesShouldBeAnchoredCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a misaligned switch expression in a variable assignment reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForVariableAssignment()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static string GetState(int status)
                                    {
                                        var state = {|#0:status switch
                                            {
                                              0 => "Idle",
                                                1 => "Running",
                                                _ => "Unknown"
                                            }|};

                                        return state;
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class Example
                                  {
                                      private static string GetState(int status)
                                      {
                                          var state = status switch
                                                      {
                                                          0 => "Idle",
                                                          1 => "Running",
                                                          _ => "Unknown"
                                                      };

                                          return state;
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, AnalyzerResources.RH5206MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned switch expression in a return statement reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForReturnStatement()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static string GetState(int status)
                                    {
                                        return {|#0:status switch
                                                {
                                                    0 => "Idle",
                                                  1 => "Running",
                                                _ => "Unknown"
                                            }|};
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class Example
                                  {
                                      private static string GetState(int status)
                                      {
                                          return status switch
                                                 {
                                                     0 => "Idle",
                                                     1 => "Running",
                                                     _ => "Unknown"
                                                 };
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, AnalyzerResources.RH5206MessageFormat));
    }

    /// <summary>
    /// Verifies that a correctly anchored switch expression does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCorrectSwitchExpression()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static string GetState(int status)
                                    {
                                        return status switch
                                               {
                                                   0 => "Idle",
                                                   1 => "Running",
                                                   _ => "Unknown"
                                               };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a same-line opening brace reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForSameLineOpeningBrace()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static string GetState(int status)
                                    {
                                        return {|#0:status switch {
                                                   0 => "Idle",
                                                   1 => "Running",
                                                   _ => "Unknown"
                                               }|};
                                    }
                                }
                                """;
        const string resultData = """
                                  internal class Example
                                  {
                                      private static string GetState(int status)
                                      {
                                          return status switch
                                                 {
                                                     0 => "Idle",
                                                     1 => "Running",
                                                     _ => "Unknown"
                                                 };
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, AnalyzerResources.RH5206MessageFormat));
    }

    /// <summary>
    /// Verifies that arms sharing a line with a previous arm are not flagged, because no formatter phase splits
    /// switch-expression arms onto separate lines (issue #247)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForArmsSharingALine()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static string GetState(int status)
                                    {
                                        return status switch
                                               {
                                                   0 => "Idle", 1 => "Running",
                                                   _ => "Unknown"
                                               };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}