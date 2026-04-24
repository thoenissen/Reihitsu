using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0360StatementMustNotBeOnSingleLineAnalyzer"/> and <see cref="RH0360StatementMustNotBeOnSingleLineCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0360StatementMustNotBeOnSingleLineAnalyzerTests : AnalyzerTestsBase<RH0360StatementMustNotBeOnSingleLineAnalyzer, RH0360StatementMustNotBeOnSingleLineCodeFixProvider>
{
    /// <summary>
    /// Verifies that clean code does not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
    /// Verifies that the issue is detected and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true) {|#0:{|} return; }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         if (true) {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0360StatementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH0360MessageFormat));
    }
}