using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6012ClosingGenericBracketsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer>
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
                                        _ = new List<int{|#0: |}>();
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
                                               Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat));
    }

    /// <summary>
    /// Verifies that a wrapped continuation-line closing generic bracket is joined onto the
    /// declaration line by the formatter's angle-bracket list join (issue #693), and that the joined
    /// result remains analyzer-clean with LF and CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineClosingGenericBracketJoinsAndStaysAnalyzerClean()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int
                                    > Method()
                                    {
                                        return new();
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 internal class TestClass
                                 {
                                     List<int> Method()
                                     {
                                         return new();
                                     }
                                 }
                                 """;

        await VerifyFormatterTransformStaysAnalyzerClean(testData, fixedData);
    }

    #endregion // Tests
}