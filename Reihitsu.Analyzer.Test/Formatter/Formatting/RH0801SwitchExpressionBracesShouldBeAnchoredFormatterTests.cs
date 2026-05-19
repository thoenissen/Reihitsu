using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0801SwitchExpressionBracesShouldBeAnchoredAnalyzer"/>
/// </summary>
[TestClass]
public class RH0801SwitchExpressionBracesShouldBeAnchoredFormatterTests : FormatterTestsBase<RH0801SwitchExpressionBracesShouldBeAnchoredAnalyzer>
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
                                 Diagnostics(RH0801SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, AnalyzerResources.RH0801MessageFormat));
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
                                 Diagnostics(RH0801SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, AnalyzerResources.RH0801MessageFormat));
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
                                 Diagnostics(RH0801SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, AnalyzerResources.RH0801MessageFormat));
    }

    #endregion // Tests
}