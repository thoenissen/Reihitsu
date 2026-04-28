using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0359BracesForMultiLineStatementsMustNotShareLineAnalyzer"/> and <see cref="RH0359BracesForMultiLineStatementsMustNotShareLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0359BracesForMultiLineStatementsMustNotShareLineAnalyzerTests : AnalyzerTestsBase<RH0359BracesForMultiLineStatementsMustNotShareLineAnalyzer, RH0359BracesForMultiLineStatementsMustNotShareLineCodeFixProvider>
{
    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
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

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true) {|#0:{|}
                                            int value = 0;
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
                                             int value = 0;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0359BracesForMultiLineStatementsMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH0359MessageFormat));
    }
}