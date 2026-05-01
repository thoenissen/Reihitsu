using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0331FirstArgumentShouldBeOnSameLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0331FirstArgumentShouldBeOnSameLineFormatterTests : FormatterTestsBase<RH0331FirstArgumentShouldBeOnSameLineAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter moves the first argument onto the invocation line
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
                                     Console.WriteLine(
                                         "first",
                                         "second");
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
                                                           "second");
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0331FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId, 8, 13, 8, 20, AnalyzerResources.RH0331MessageFormat));
    }

    #endregion // Members
}