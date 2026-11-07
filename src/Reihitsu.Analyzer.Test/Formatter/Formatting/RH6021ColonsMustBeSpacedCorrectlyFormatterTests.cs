using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6021ColonsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6021ColonsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6021ColonsMustBeSpacedCorrectlyAnalyzer>
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
                                internal class TestClass{|#0::|}System.IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass : System.IDisposable
                                 {
                                     public void Dispose()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH6021ColonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6021MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter's output for a constructor-initializer colon directly followed by a documentation comment
    /// stays unchanged and raises no diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6021-002")]
    public async Task VerifyFormatterOutputWithStrayDocumentationCommentBehindColonIsNotReported()
    {
        const string testData = """
                                internal class B
                                {
                                    public B(int a)
                                    {
                                    }
                                }

                                internal class C : B
                                {
                                    public C()
                                        :/// x
                                    base(1)
                                    {
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    #endregion // Tests
}