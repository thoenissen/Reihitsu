using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0333MultiLineArgumentsShouldBeAlignedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0333MultiLineArgumentsShouldBeAlignedFormatterTests : FormatterTestsBase<RH0333MultiLineArgumentsShouldBeAlignedAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter aligns wrapped arguments to a shared anchor
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             using System;

                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     Console.WriteLine("first",
                                       "second",
                                             "third");
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Console.WriteLine("first",
                                                           "second",
                                                           "third");
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0333MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, 8, 11, 8, 19, AnalyzerResources.RH0333MessageFormat),
                                 ExpectedDiagnostic(RH0333MultiLineArgumentsShouldBeAlignedAnalyzer.DiagnosticId, 9, 17, 9, 24, AnalyzerResources.RH0333MessageFormat));
    }

    #endregion // Members
}