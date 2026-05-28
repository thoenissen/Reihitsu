using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer"/>
/// </summary>
[TestClass]
public class RH5023CodeMustNotContainMultipleBlankLinesInARowFormatterTests : FormatterTestsBase<RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer>
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
                                 Diagnostics(RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer.DiagnosticId, AnalyzerResources.RH5023MessageFormat));
    }

    #endregion // Tests
}