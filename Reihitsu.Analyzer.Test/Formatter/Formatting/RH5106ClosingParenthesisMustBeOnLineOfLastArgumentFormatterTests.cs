using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer"/>
/// </summary>
[TestClass]
public class RH5106ClosingParenthesisMustBeOnLineOfLastArgumentFormatterTests : FormatterTestsBase<RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter moves the closing parenthesis onto the final parameter line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method(int first,
                                             int second
                                 {|#0:)|}
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method(int first,
                                                 int second)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer.DiagnosticId, AnalyzerResources.RH5106MessageFormat));
    }

    #endregion // Tests
}