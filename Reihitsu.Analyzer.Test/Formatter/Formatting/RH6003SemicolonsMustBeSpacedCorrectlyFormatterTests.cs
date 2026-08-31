using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6003SemicolonsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer>
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
                                        var value = 0{|#0: |};
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

        await VerifyFormatterFixAndIdempotency(testData,
                                               fixedData,
                                               Diagnostics(RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6003MessageFormat));
    }

    /// <summary>
    /// Verifies that continuation-line indentation is stable and analyzer-clean with LF and CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineSemicolonStaysAnalyzerClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 1
                                        ;
                                    }
                                }
                                """;

        await VerifyFormatterStability(testData);
    }

    /// <summary>
    /// Verifies that a documentation comment in front of the terminating semicolon leaves the formatter output analyzer-clean. The
    /// horizontal spacing rewriter exempts the gap in front of such a comment from normalization, so the exemption
    /// must not be wide enough to leave a space this analyzer reports (issues #591, #625)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationCommentBeforeSemicolonStaysAnalyzerClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _second /** Trailing note. */;
                                }
                                """;

        await VerifyFormatterStability(testData);
    }

    #endregion // Tests
}