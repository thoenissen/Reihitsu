using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0369WhileDoFooterMustNotBePrecededByBlankLineAnalyzer"/> and <see cref="RH0369WhileDoFooterMustNotBePrecededByBlankLineCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0369WhileDoFooterMustNotBePrecededByBlankLineAnalyzerTests : AnalyzerTestsBase<RH0369WhileDoFooterMustNotBePrecededByBlankLineAnalyzer, RH0369WhileDoFooterMustNotBePrecededByBlankLineCodeFixProvider>
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
                                        do
                                        {
                                        }
                                        while (true);
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
                                        do
                                        {
                                        }
                                {|#0:
                                |}        while (true);
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         do
                                         {
                                         }
                                         while (true);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0369WhileDoFooterMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0369MessageFormat));
    }

    /// <summary>
    /// Verifies that regular while statements do not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyWhileStatementsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        while (true)
                                        {

                                            break;
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }
}