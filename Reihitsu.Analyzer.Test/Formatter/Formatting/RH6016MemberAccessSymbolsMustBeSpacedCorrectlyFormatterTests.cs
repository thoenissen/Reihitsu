using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6016MemberAccessSymbolsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer>
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
                                        _ = string {|#0:.|}Empty;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         _ = string.Empty;
                                     }
                                 }
                                 """;

        await VerifyFormatterFixAndIdempotency(testData,
                                               fixedData,
                                               Diagnostics(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6016MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter tightens a spaced conditional access and stays at that result
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesSpacedConditionalAccess()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(string value)
                                    {
                                        _ = value {|#0:?|}.Length;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(string value)
                                     {
                                         _ = value?.Length;
                                     }
                                 }
                                 """;

        await VerifyFormatterFixAndIdempotency(testData,
                                               fixedData,
                                               Diagnostics(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6016MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter leaves code the analyzer accepts untouched, including the question
    /// marks of a conditional expression and of a nullable type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterLeavesAnalyzerCleanMemberAccessUntouched()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(string value, int[] values, bool flag)
                                    {
                                        int? nullable = null;

                                        _ = value?.Length;
                                        _ = values?[0];
                                        _ = flag ? 1 : 2;
                                        _ = nullable;
                                    }
                                }
                                """;

        await VerifyFormatterStability(testData);
    }

    #endregion // Tests
}