using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5024ClosingBraceMustNotBePrecededByBlankLineFormatterTests : FormatterTestsBase<RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer>
{
    #region Tests

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

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter removes the blank line between a line that holds two delimited comments and a closing
    /// brace, so its output is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public async Task VerifyFormatterRemovesBlankLineBehindLineOfTwoDelimitedComments()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method()
                                 {
                                     int value = 0;

                                     /* a */ /* b */
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

                                         /* a */ /* b */
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat));
    }

    #endregion // Tests
}