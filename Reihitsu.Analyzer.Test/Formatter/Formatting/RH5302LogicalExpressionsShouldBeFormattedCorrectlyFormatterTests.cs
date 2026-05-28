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

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 6, 13, 6, 15, AnalyzerResources.RH5302MessageFormat),
                                 ExpectedDiagnostic(RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 7, 13, 7, 15, AnalyzerResources.RH5302MessageFormat));
    }

    #endregion // Tests
}