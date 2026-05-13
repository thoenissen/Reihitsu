using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0345OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0345OpeningGenericBracketsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH0345OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer>
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
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = new List{|#0: |}<int>();
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         _ = new List<int>();
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(testData,
                                 fixedData,
                                 Diagnostics(RH0345OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0345MessageFormat));
    }

    #endregion // Tests
}