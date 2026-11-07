using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5305CollectionExpressionsShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer>
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

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5305MessageFormat));
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

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5305MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter aligns the elements and the closing bracket of a collection expression whose opening bracket
    /// stands behind a comment that leads its line to that bracket, where the analyzer measures it, and clears the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5305-001")]
    [Requirement("FMT-INDENT-025")]
    [Requirement("FMT-INDENT-030")]
    public async Task VerifyFormatterAlignsCollectionExpressionToItsBracketBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     List<int> x =

                                     /* c */ {|#0:[
                                         1,
                                         2
                                     ]|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         List<int> x =

                                         /* c */ [
                                                     1,
                                                     2
                                                 ];
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5305MessageFormat));
    }

    #endregion // Tests
}