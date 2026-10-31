using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6001KeywordsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6001KeywordsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6001KeywordsMustBeSpacedCorrectlyAnalyzer>
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
                                        {|#0:if|}(true)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6001MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter's output for an <c>if</c> keyword directly followed by a documentation comment stays
    /// unchanged and raises no diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6001-002")]
    public async Task VerifyFormatterOutputWithStrayDocumentationCommentBehindKeywordIsNotReported()
    {
        const string testData = """
                                internal class C
                                {
                                    internal void M(bool c, int[] items)
                                    {
                                        if/// x
                                        (c)
                                        {
                                        }
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    /// <summary>
    /// Verifies that the formatter's output for an <c>if</c> keyword followed by a block comment and a documentation comment
    /// stays unchanged and raises no diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6001-002")]
    public async Task VerifyFormatterOutputWithBlockCommentAndStrayDocumentationCommentBehindKeywordIsNotReported()
    {
        const string testData = """
                                internal class C
                                {
                                    internal void M(bool c)
                                    {
                                        if/* c *//// x
                                        (c)
                                        {
                                        }
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    #endregion // Tests
}