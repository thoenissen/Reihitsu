using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6023AssignmentOperatorsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer>
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
                                        var a   {|#0:=|} 2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var a = 2;
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6023MessageFormat));
    }

    #endregion // Tests
}