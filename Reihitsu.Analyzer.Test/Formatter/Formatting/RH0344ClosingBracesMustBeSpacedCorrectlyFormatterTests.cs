using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0344ClosingBracesMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0344ClosingBracesMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH0344ClosingBracesMustBeSpacedCorrectlyAnalyzer>
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
                                    int Property { get; set;{|#0:}|}
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
                                 Diagnostics(RH0344ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0344MessageFormat));
    }

    #endregion // Tests
}