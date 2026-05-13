using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0340OpeningParenthesisMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0340OpeningParenthesisMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH0340OpeningParenthesisMustBeSpacedCorrectlyAnalyzer>
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
                                    int Method()
                                    {
                                        return ({|#0: |}0);
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method()
                                     {
                                         return (0);
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH0340OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0340MessageFormat));
    }

    #endregion // Tests
}