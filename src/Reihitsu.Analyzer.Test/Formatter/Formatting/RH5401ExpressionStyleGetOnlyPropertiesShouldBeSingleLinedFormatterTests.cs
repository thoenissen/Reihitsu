using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer"/>
/// </summary>
[TestClass]
public class RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedFormatterTests : FormatterTestsBase<RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter collapses multi-line expression-bodied properties
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:internal int Value
                                     => 42;|}
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Value => 42;
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5401MessageFormat));
    }

    /// <summary>
    /// Verifies that the expression-bodied property the formatter produces from a get-only accessor list is single-lined
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterProducesSingleLinedPropertyFromGetOnlyAccessorList()
    {
        const string input = """
                             internal class Example
                             {
                                 internal int Value
                                 {
                                     get
                                     {
                                         return 42;
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Value => 42;
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    #endregion // Tests
}