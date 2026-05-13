using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0383DoNotPlaceRegionsWithinElementsAnalyzer"/>
/// </summary>
[TestClass]
public class RH0383DoNotPlaceRegionsWithinElementsFormatterTests : FormatterTestsBase<RH0383DoNotPlaceRegionsWithinElementsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes regions placed inside elements
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
                                     {|#0:#region Helper|}
                                     var value = 1;
                                     {|#1:#endregion|}
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0383DoNotPlaceRegionsWithinElementsAnalyzer.DiagnosticId, AnalyzerResources.RH0383MessageFormat, 2));
    }

    #endregion // Tests
}