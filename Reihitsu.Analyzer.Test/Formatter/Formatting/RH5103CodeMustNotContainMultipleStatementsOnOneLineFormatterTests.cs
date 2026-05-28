using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5103CodeMustNotContainMultipleStatementsOnOneLineFormatterTests : FormatterTestsBase<RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer>
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
                                 Diagnostics(RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH5103MessageFormat));
    }

    #endregion // Tests
}