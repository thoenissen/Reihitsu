using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0335CommasMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0335CommasMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH0335CommasMustBeSpacedCorrectlyAnalyzer>
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
                                    void Method(int x{|#0:,|}int y)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x, int y)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH0335CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0335MessageFormat));
    }

    #endregion // Tests
}