using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer"/>
/// </summary>
[TestClass]
public class RH5206SwitchExpressionBracesShouldBeAnchoredFormatterTests : FormatterTestsBase<RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter anchors switch expressions in variable assignments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesVariableAssignmentViolation()
    {
        const string input = """
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
        const string fixedData = """
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

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, AnalyzerResources.RH5206MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter anchors switch expressions in return statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesReturnStatementViolation()
    {
        const string input = """
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
        const string fixedData = """
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

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, AnalyzerResources.RH5206MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter moves a same-line opening brace to the anchor line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesSameLineOpeningBraceViolation()
    {
        const string input = """
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
        const string fixedData = """
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

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, AnalyzerResources.RH5206MessageFormat));
    }

    #endregion // Tests
}