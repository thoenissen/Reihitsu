using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0329LogicalExpressionsShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Members

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
                                 ExpectedDiagnostic(RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 6, 13, 6, 15, AnalyzerResources.RH0329MessageFormat),
                                 ExpectedDiagnostic(RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 7, 13, 7, 15, AnalyzerResources.RH0329MessageFormat));
    }

    #endregion // Members
}