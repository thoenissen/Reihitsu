using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer"/>
/// </summary>
[TestClass]
public class RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasFormatterTests : FormatterTestsBase<RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer>
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

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH5410MessageFormat));
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

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH5410MessageFormat));
    }

    /// <summary>
    /// Verifies that the trailing comma before a conditional-compilation block carrying a further element stays
    /// analyzer-clean and formatter-stable under LF and CRLF, since the formatter and the analyzer withhold in
    /// lock-step
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyArrayWithConditionalBlockAfterFinalItemIsFormatterStable()
    {
        const string input = """
                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     var values = new[]
                                                  {
                                                      1,
                             #if SYMBOL
                                         2,
                             #endif
                                                  };
                                 }
                             }
                             """;

        await VerifyFormatterStability(input);
    }

    #endregion // Tests
}