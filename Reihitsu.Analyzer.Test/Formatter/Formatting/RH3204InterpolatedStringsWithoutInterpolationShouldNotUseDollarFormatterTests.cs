using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer"/>
/// </summary>
[TestClass]
public class RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarFormatterTests : FormatterTestsBase<RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes the interpolation marker from standard interpolated strings without interpolation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesStandardInterpolatedStringWithoutInterpolation()
    {
        const string input = """
                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     var message = {|#0:$"System ready"|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var message = "System ready";
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer.DiagnosticId, AnalyzerResources.RH3204MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes the interpolation marker from raw interpolated strings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesRawInterpolatedStringWithoutInterpolation()
    {
        const string input = """"
                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     var text = {|#0:$"""
                                     hello
                                     world
                                     """|};
                                 }
                             }
                             """";
        const string fixedData = """"
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var text = """
                                                    hello
                                                    world
                                                    """;
                                     }
                                 }
                                 """";

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer.DiagnosticId, AnalyzerResources.RH3204MessageFormat));
    }

    #endregion // Tests
}