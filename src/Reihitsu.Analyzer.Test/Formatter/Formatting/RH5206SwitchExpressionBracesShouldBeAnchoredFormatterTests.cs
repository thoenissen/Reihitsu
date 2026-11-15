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

        await VerifyFormatter(input,
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

        await VerifyFormatter(input,
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

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.DiagnosticId, AnalyzerResources.RH5206MessageFormat));
    }

    /// <summary>
    /// Verifies that a switch expression arm behind a comment that starts its line in the arm column is not reported and stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyArmBehindLeadingCommentInArmColumnIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    string M(int status)
                                    {
                                        return status switch
                                               {
                                                   /* idle */ 0 => "Idle",
                                                   _ => "Other"
                                               };
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    /// <summary>
    /// Verifies that a switch expression opening brace behind a comment that starts its line in the column of the governing expression is not reported and stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5206-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyOpeningBraceBehindLeadingCommentInAnchorColumnIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    string M(int status)
                                    {
                                        return status switch
                                               /* map */ {
                                                   0 => "Idle",
                                                   _ => "Other"
                                               };
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    #endregion // Tests
}