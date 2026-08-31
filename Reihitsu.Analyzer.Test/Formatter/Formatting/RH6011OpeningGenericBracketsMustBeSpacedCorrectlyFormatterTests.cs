using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6011OpeningGenericBracketsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer>
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

        await VerifyFormatterFixAndIdempotency(testData,
                                               fixedData,
                                               Diagnostics(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6011MessageFormat));
    }

    /// <summary>
    /// Verifies that a continuation-line opening generic bracket remains analyzer-clean with LF and CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineOpeningGenericBracketStaysAnalyzerClean()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List
                                    <int> Method()
                                    {
                                        return new();
                                    }
                                }
                                """;

        await VerifyFormatterStability(testData);
    }

    #endregion // Tests
}