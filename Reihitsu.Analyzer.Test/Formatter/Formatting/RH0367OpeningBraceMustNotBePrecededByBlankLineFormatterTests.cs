using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0367OpeningBraceMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0367OpeningBraceMustNotBePrecededByBlankLineFormatterTests : FormatterTestsBase<RH0367OpeningBraceMustNotBePrecededByBlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes blank lines before opening braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method()
                             {|#0:
                             |}    {
                                     int value = 0;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method()
                                     {
                                         int value = 0;
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0367OpeningBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0367MessageFormat));
    }

    #endregion // Tests
}