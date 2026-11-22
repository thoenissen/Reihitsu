using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5302LogicalExpressionsShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter aligns wrapped logical operators
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     var result = true
                                         && false
                                         && true;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var result = true
                                                      && false
                                                      && true;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 6, 13, 6, 15, MessageFormat),
                              ExpectedDiagnostic(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 7, 13, 7, 15, MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter moves trailing operators to the beginning of the continuation line, matching
    /// the code fix, and remains stable on a second pass under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterMovesTrailingOperatorsAndIsIdempotent()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     var result = true &&
                                         false ||
                                         true;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var result = true
                                                      && false
                                                      || true;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 5, 27, 5, 29, MessageFormat),
                              ExpectedDiagnostic(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 6, 19, 6, 21, MessageFormat));
    }

    /// <summary>
    /// Verifies that an operator behind a comment that starts its line in the operator column is not reported and stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5302-002")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyOperatorBehindLeadingCommentInOperatorColumnIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    bool M(bool a, bool b)
                                    {
                                        return a
                                               /* and */ && b;
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    #endregion // Tests
}