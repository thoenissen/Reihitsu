using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0822CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0822CollectionExpressionsShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH0822CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter separates multiline collection expression elements onto individual lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesMultipleElementsOnSameLineInMultilineCollectionExpression()
    {
        const string input = """
                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     int[] values = {|#0:[
                                         1, 2,
                                         3
                                               ]|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         int[] values = [
                                                            1,
                                                            2,
                                                            3
                                                        ];
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0822CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0822MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter fixes collection expression layout in return statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesCollectionExpressionInReturnStatement()
    {
        const string input = """
                             internal class Example
                             {
                                 private static int[] Method()
                                 {
                                     return {|#0:[
                                         1, 2,
                                         3
                                               ]|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static int[] Method()
                                     {
                                         return [
                                                    1,
                                                    2,
                                                    3
                                                ];
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0822CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0822MessageFormat));
    }

    #endregion // Tests
}