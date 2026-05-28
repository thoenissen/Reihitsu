using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5402BracesForMultiLineStatementsMustNotShareLineFormatterTests : FormatterTestsBase<RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer>
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
                                 Diagnostics(RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5402MessageFormat));
    }

    #endregion // Tests
}