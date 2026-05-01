using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0309ForStatementsShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter inserts a blank line before for statements
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
                                     var limit = 1;
                                     for (var index = 0; index < limit; index++)
                                     {
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var limit = 1;

                                         for (var index = 0; index < limit; index++)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 6, 9, 6, 12, AnalyzerResources.RH0309MessageFormat));
    }

    #endregion // Members
}