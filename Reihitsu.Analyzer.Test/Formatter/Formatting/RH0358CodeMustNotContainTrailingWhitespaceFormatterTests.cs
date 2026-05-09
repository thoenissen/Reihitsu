using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0358CodeMustNotContainTrailingWhitespaceAnalyzer"/>
/// </summary>
[TestClass]
public class RH0358CodeMustNotContainTrailingWhitespaceFormatterTests : FormatterTestsBase<RH0358CodeMustNotContainTrailingWhitespaceAnalyzer>
{
    #region Members

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
                                        var value = 0;{|#0:    |}
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var value = 0;
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH0358CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, AnalyzerResources.RH0358MessageFormat));
    }

    #endregion // Members
}