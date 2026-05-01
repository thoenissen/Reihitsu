using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0332ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer"/>
/// </summary>
[TestClass]
public class RH0332ArgumentsShouldBeOnSingleOrSeparateLinesFormatterTests : FormatterTestsBase<RH0332ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer>
{
    #region Members

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
                                 ExpectedDiagnostic(RH0332ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId, 7, 26, 8, 35, AnalyzerResources.RH0332MessageFormat));
    }

    #endregion // Members
}