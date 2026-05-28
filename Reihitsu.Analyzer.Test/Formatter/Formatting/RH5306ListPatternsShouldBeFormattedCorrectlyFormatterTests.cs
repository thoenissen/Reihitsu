using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5306ListPatternsShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter separates multiline list-pattern entries onto individual lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesMultipleEntriesOnSameLineInMultilineListPattern()
    {
        const string input = """
                             internal class Example
                             {
                                 private static bool Method(int[] values)
                                 {
                                     return values is {|#0:[
                                         1, 2,
                                         3
                                               ]|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static bool Method(int[] values)
                                     {
                                         return values is [
                                                              1,
                                                              2,
                                                              3
                                                          ];
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5306MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter aligns multiline list-pattern brackets
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesMisalignedBracketsInMultilineListPattern()
    {
        const string input = """
                             internal class Example
                             {
                                 private static bool Method(int[] values)
                                 {
                                     return values is {|#0:[
                                         1,
                                         .. var rest
                                               ]|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static bool Method(int[] values)
                                     {
                                         return values is [
                                                              1,
                                                              .. var rest
                                                          ];
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5306MessageFormat));
    }

    #endregion // Tests
}