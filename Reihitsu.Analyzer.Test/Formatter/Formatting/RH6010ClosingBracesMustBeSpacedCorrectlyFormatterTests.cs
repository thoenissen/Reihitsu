using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6010ClosingBracesMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer>
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
                                 Diagnostics(RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6010MessageFormat));
    }

    #endregion // Tests
}