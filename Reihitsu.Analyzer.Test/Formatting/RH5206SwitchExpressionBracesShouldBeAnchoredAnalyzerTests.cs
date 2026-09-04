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
public class RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzerTests : BatchCodeFixTestsBase<RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer, RH5206SwitchExpressionBracesShouldBeAnchoredCodeFixProvider>
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

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    private static string Outer(int a, int b)
                                    {
                                        var value = {|#0:a switch
                                        {
                                            0 => {|#1:b switch
                                            {
                                                0 => "Idle",
                                                    _ => "Unknown"
                                            }|},
                                                _ => "Fallback"
                                        }|};

                                        return value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     private static string Outer(int a, int b)
                                     {
                                         var value = a switch
                                                     {
                                                         0 => b switch
                                                              {
                                                                  0 => "Idle",
                                                                  _ => "Unknown"
                                                              },
                                                         _ => "Fallback"
                                                     };

                                         return value;
                                     }
                                 }
                                 """;

        // Both switch expressions resolve to the same fix target: the enclosing equals-value clause of the
        // "var value = ..." declaration, since walking up from the nested switch expression passes straight
        // through the outer one without matching a statement, initializer, or arrow clause of its own. The batch
        // fixer discards the second, overlapping action, and the surviving single fix already anchors both the
        // outer and the nested switch expression's braces and arms in one pass
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, AnalyzerResources.RH5206MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}