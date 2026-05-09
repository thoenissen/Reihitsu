using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0366ClosingBraceMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0366ClosingBraceMustNotBePrecededByBlankLineFormatterTests : FormatterTestsBase<RH0366ClosingBraceMustNotBePrecededByBlankLineAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter removes blank lines before closing braces
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
                                     int value = 0;
                             {|#0:
                             |}    }
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
                                 Diagnostics(RH0366ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0366MessageFormat));
    }

    #endregion // Members
}