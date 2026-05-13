using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0343OpeningBracesMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0343OpeningBracesMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH0343OpeningBracesMustBeSpacedCorrectlyAnalyzer>
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
                                    int Property{|#0:{|} get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Property { get; set; }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH0343OpeningBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0343MessageFormat));
    }

    #endregion // Tests
}