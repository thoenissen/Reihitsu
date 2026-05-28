using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5027WhileDoFooterMustNotBePrecededByBlankLineFormatterTests : FormatterTestsBase<RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes blank lines before do-while footers
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
                                     do
                                     {
                                     }
                             {|#0:
                             |}        while (true);
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method()
                                     {
                                         do
                                         {
                                         }
                                         while (true);
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5027MessageFormat));
    }

    #endregion // Tests
}