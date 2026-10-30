using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer"/>
/// </summary>
[TestClass]
public class RH6005OperatorKeywordMustBeFollowedBySpaceFormatterTests : FormatterTestsBase<RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer>
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
                                    public static TestClass {|#0:operator|}+(TestClass left, TestClass right) => left;
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public static TestClass operator +(TestClass left, TestClass right)
                                     {
                                         return left;
                                     }
                                 }
                                 """;

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6005MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter's output for an <c>operator</c> keyword directly followed by a documentation comment stays
    /// unchanged and raises no diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6005-001")]
    public async Task VerifyFormatterOutputWithStrayDocumentationCommentBehindOperatorKeywordIsNotReported()
    {
        const string testData = """
                                internal class C
                                {
                                    public static C operator/// x
                                    +(C a, C b)
                                    {
                                        return a;
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    #endregion // Tests
}