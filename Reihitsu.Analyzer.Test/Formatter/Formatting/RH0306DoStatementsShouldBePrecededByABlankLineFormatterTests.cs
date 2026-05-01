using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0306DoStatementsShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter inserts a blank line before do statements
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
                                     var value = 1;
                                     do
                                     {
                                         value--;
                                     }
                                     while (value > 0);
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var value = 1;

                                         do
                                         {
                                             value--;
                                         }
                                         while (value > 0);
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 6, 9, 6, 11, AnalyzerResources.RH0306MessageFormat));
    }

    #endregion // Members
}