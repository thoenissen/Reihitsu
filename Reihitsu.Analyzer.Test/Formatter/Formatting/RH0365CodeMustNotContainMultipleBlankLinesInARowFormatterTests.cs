using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0365CodeMustNotContainMultipleBlankLinesInARowAnalyzer"/>
/// </summary>
[TestClass]
public class RH0365CodeMustNotContainMultipleBlankLinesInARowFormatterTests : FormatterTestsBase<RH0365CodeMustNotContainMultipleBlankLinesInARowAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter reduces multiple blank lines to a single blank line
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
                                     int first = 0;
                             
                             {|#0:
                             |}        int second = 1;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method()
                                     {
                                         int first = 0;
                                 
                                         int second = 1;
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0365CodeMustNotContainMultipleBlankLinesInARowAnalyzer.DiagnosticId, AnalyzerResources.RH0365MessageFormat));
    }

    #endregion // Tests
}