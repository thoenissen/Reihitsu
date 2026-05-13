using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0348ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0348ClosingAttributeBracketsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH0348ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer>
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
                                [System.Obsolete{|#0: |}]
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 [System.Obsolete]
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH0348ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0348MessageFormat));
    }

    #endregion // Tests
}