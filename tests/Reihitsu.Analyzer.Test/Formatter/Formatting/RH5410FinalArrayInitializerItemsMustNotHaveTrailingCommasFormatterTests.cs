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

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, MessageFormat));
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

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the trailing comma before a conditional-compilation block carrying a further element is reported,
    /// and that the formatter keeps it as written under LF and CRLF, since another build can need it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("FMT-STRUCT-020")]
    public async Task VerifyArrayWithConditionalBlockAfterFinalItemIsReportedAndKeptByTheFormatter()
    {
        const string input = """
                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     var values = new[]
                                                  {
                                                      1{|RH5410:,|}
                             #if SYMBOL
                                         2,
                             #endif
                                                  };
                                 }
                             }
                             """;

        await VerifyFormatterKeepsReportedShape(input);
    }

    /// <summary>
    /// Verifies the change's example of an array initializer: the comma in front of the <c>#endif</c> of an active block is
    /// reported, and the formatter keeps it as written under LF and CRLF, although the code fix removes it, so formatting
    /// does not clear the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-020")]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifyCommaInFrontOfEndIfOfAnActiveBlockIsReportedAndKeptByTheFormatter()
    {
        const string input = """
                             class C
                             {
                                 private int[] _numbers = new[]
                                                          {
                                                              1,
                             #if !X
                                                              2{|RH5410:,|}
                             #endif
                                                          };
                             }
                             """;

        await VerifyFormatterKeepsReportedShape(input);
    }

    #endregion // Tests
}