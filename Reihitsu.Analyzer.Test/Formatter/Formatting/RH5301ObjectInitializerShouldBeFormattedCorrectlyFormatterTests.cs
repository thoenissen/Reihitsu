using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5301ObjectInitializerShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter aligns object initializer braces and members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     var value = new Example
                                     {
                                     Name = "Test"
                                     };
                                 }

                                 internal string Name { get; set; }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var value = new Example
                                                     {
                                                         Name = "Test"
                                                     };
                                     }

                                     internal string Name { get; set; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 5, 21, 8, 10, AnalyzerResources.RH5301MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter aligns target-typed object initializer braces and members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesTargetTypedViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     Example value = new()
                                     {
                                     Name = "Test"
                                     };
                                 }

                                 internal string Name { get; set; }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Example value = new()
                                                         {
                                                             Name = "Test"
                                                         };
                                     }

                                     internal string Name { get; set; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 5, 25, 8, 10, AnalyzerResources.RH5301MessageFormat));
    }

    #endregion // Tests
}