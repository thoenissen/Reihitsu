using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer"/>
/// </summary>
[TestClass]
public class RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasFormatterTests : FormatterTestsBase<RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes a trailing comma from the final collection initializer item
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesTrailingCommaOnFinalCollectionInitializerItem()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     var values = new List<int>
                                                  {
                                                      1,
                                                      2{|#0:,|}
                                                  };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var values = new List<int>
                                                      {
                                                          1,
                                                          2
                                                      };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter preserves a same-line comment attached to the final collection initializer item
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterPreservesSameLineCommentOnFinalCollectionInitializerItem()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     var values = new List<int>
                                                  {
                                                      2{|#0:,|} // Final value
                                                  };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var values = new List<int>
                                                      {
                                                          2 // Final value
                                                      };
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the trailing comma before a conditional-compilation block carrying a further item is reported,
    /// and that the formatter keeps it as written under LF and CRLF, since another build can need it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TRAIL-002")]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("FMT-STRUCT-020")]
    public async Task VerifyCollectionWithConditionalBlockAfterFinalItemIsReportedAndKeptByTheFormatter()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     var values = new List<int>
                                                  {
                                                      1{|RH5411:,|}
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
    /// Verifies the change's example of a collection initializer: the comma in front of the <c>#endif</c> behind an active
    /// <c>#else</c> branch is reported, and the formatter keeps it as written under LF and CRLF, although the code fix
    /// removes it, so formatting does not clear the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-020")]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifyCommaAtTheEndOfAnActiveElseBranchIsReportedAndKeptByTheFormatter()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 private List<int> _numbers = new List<int>
                                                              {
                                                                  1,
                             #if X
                                                                  2,
                             #else
                                                                  3{|RH5411:,|}
                             #endif
                                                              };
                             }
                             """;

        await VerifyFormatterKeepsReportedShape(input);
    }

    #endregion // Tests
}