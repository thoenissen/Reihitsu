using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0375CodeMustNotContainMultipleStatementsOnOneLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0375CodeMustNotContainMultipleStatementsOnOneLineFormatterTests : FormatterTestsBase<RH0375CodeMustNotContainMultipleStatementsOnOneLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter splits multiple statements onto separate lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method()
                                 {
                                     int first = 1; {|#0:int second = 2;|}
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method()
                                     {
                                         int first = 1;
                                         int second = 2;
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0375CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH0375MessageFormat));
    }

    #endregion // Tests
}