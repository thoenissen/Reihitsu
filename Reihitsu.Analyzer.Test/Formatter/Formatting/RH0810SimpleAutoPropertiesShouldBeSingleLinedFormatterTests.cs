using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0810SimpleAutoPropertiesShouldBeSingleLinedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0810SimpleAutoPropertiesShouldBeSingleLinedFormatterTests : FormatterTestsBase<RH0810SimpleAutoPropertiesShouldBeSingleLinedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter collapses a multi-line get/set auto-property to one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesGetSetAutoProperty()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:internal int Value
                                 {
                                     get;
                                     set;
                                 }|}
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Value { get; set; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0810SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH0810MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses a multi-line get-only auto-property to one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesGetOnlyAutoProperty()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:internal int Value
                                 {
                                     get;
                                 }|}
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Value { get; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0810SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH0810MessageFormat));
    }

    #endregion // Tests
}