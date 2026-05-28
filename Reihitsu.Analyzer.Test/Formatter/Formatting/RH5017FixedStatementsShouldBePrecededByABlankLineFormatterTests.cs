using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5017FixedStatementsShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter inserts a blank line before fixed statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal unsafe void Method()
                                 {
                                     var values = new[] { 1 };
                                     fixed (int* pointer = values)
                                     {
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal unsafe void Method()
                                     {
                                         var values = new[] { 1 };

                                         fixed (int* pointer = values)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 6, 9, 6, 14, AnalyzerResources.RH5017MessageFormat));
    }

    #endregion // Tests
}