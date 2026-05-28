using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5014SwitchStatementsShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter inserts a blank line before switch statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(int value)
                                 {
                                     var number = value;
                                     switch (number)
                                     {
                                         default:
                                             break;
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method(int value)
                                     {
                                         var number = value;

                                         switch (number)
                                         {
                                             default:
                                                 break;
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 6, 9, 6, 15, AnalyzerResources.RH5014MessageFormat));
    }

    #endregion // Tests
}