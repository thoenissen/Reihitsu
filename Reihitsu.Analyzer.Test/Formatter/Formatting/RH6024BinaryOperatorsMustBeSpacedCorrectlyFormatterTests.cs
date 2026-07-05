using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6024BinaryOperatorsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer>
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
                                    int Method(int a, int b)
                                    {
                                        return a  {|#0:+|}  b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int a, int b)
                                     {
                                         return a + b;
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6024MessageFormat));
    }

    #endregion // Tests
}