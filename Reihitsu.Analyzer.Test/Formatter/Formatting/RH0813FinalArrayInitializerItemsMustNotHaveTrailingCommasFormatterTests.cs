using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0813FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer"/>
/// </summary>
[TestClass]
public class RH0813FinalArrayInitializerItemsMustNotHaveTrailingCommasFormatterTests : FormatterTestsBase<RH0813FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes a trailing comma from the final array initializer item
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesTrailingCommaOnFinalArrayInitializerItem()
    {
        const string input = """
                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     var values = new[] { 1, 2{|#0:,|} };
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var values = new[] { 1, 2 };
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0813FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH0813MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter preserves a same-line comment attached to the final array initializer item
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterPreservesSameLineCommentOnFinalArrayInitializerItem()
    {
        const string input = """
                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     var values = new[] { 2{|#0:,|} // Final value
                                     };
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var values = new[] { 2 // Final value
                                                      };
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0813FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH0813MessageFormat));
    }

    #endregion // Tests
}