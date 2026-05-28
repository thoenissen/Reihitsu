using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer"/>
/// </summary>
[TestClass]
public class RH5102ArgumentsShouldBeOnSingleOrSeparateLinesFormatterTests : FormatterTestsBase<RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter splits mixed-line argument lists into one argument per line
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
                                     Console.WriteLine("first", "second",
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
                                 ExpectedDiagnostic(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, 7, 26, 8, 35, AnalyzerResources.RH5102MessageFormat));
    }

    #endregion // Tests
}