using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH7303DoNotPlaceRegionsWithinElementsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7303DoNotPlaceRegionsWithinElementsFormatterTests : FormatterTestsBase<RH7303DoNotPlaceRegionsWithinElementsAnalyzer>
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
                                 Diagnostics(RH7303DoNotPlaceRegionsWithinElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7303MessageFormat, 2));
    }

    #endregion // Tests
}