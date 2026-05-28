using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5303CollectionInitializerShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter aligns explicit collection initializer braces and members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesExplicitCollectionInitializerViolation()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     var values = {|#0:new List<int>
                                     {
                                     1,
                                     2
                                     }|};
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
                                 Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5303MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter aligns target-typed collection initializer braces and members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesTargetTypedCollectionInitializerViolation()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 private static void Method()
                                 {
                                     List<int> values = {|#0:new()
                                     {
                                     1,
                                     2
                                     }|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         List<int> values = new()
                                                            {
                                                                1,
                                                                2
                                                            };
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5303MessageFormat));
    }

    #endregion // Tests
}