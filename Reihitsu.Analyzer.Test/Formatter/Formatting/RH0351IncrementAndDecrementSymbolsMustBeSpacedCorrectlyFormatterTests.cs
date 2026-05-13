using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0351IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0351IncrementAndDecrementSymbolsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH0351IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter fixes the targeted violation and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 0;
                                        value{|#0: |}++;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = 0;

                                         value++;
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH0351IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0351MessageFormat));
    }

    #endregion // Tests
}