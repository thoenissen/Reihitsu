using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0359BracesForMultiLineStatementsMustNotShareLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0359BracesForMultiLineStatementsMustNotShareLineFormatterTests : FormatterTestsBase<RH0359BracesForMultiLineStatementsMustNotShareLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter moves opening braces for multi-line statements onto their own line
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
                                     if (true) {|#0:{|}
                                         int value = 0;
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         {
                                             int value = 0;
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0359BracesForMultiLineStatementsMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH0359MessageFormat));
    }

    #endregion // Tests
}