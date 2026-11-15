using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5308ConditionalExpressionsShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter aligns a misaligned multi-line conditional expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 string Method(bool condition)
                                 {
                                     var value = condition
                             {|#0:?|} "1"
                             : "2";

                                     return value;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     string Method(bool condition)
                                     {
                                         var value = condition
                                                         ? "1"
                                                         : "2";

                                         return value;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 6, 1, 6, 2, AnalyzerResources.RH5308MessageFormat));
    }

    /// <summary>
    /// Verifies that a conditional operator behind a comment that starts its line in the operator column is not reported and stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5308-002")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyOperatorBehindLeadingCommentInOperatorColumnIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    int M(bool a)
                                    {
                                        return a
                                                   /* t */ ? 1
                                                   : 2;
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    #endregion // Tests
}