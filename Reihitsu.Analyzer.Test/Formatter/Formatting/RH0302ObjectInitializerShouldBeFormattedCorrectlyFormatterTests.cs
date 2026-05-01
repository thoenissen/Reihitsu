using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0302ObjectInitializerShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer>
{
    #region Members

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
                                 ExpectedDiagnostic(RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 5, 21, 8, 10, AnalyzerResources.RH0302MessageFormat));
    }

    #endregion // Members
}