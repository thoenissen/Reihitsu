using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0814FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer"/>
/// </summary>
[TestClass]
public class RH0814FinalCollectionInitializerItemsMustNotHaveTrailingCommasFormatterTests : FormatterTestsBase<RH0814FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer>
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

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0814FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH0814MessageFormat));
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

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0814FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH0814MessageFormat));
    }

    #endregion // Tests
}