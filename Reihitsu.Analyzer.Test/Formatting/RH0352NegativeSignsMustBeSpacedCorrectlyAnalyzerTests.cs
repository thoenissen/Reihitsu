using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0352NegativeSignsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH0352NegativeSignsMustBeSpacedCorrectlyCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0352NegativeSignsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0352NegativeSignsMustBeSpacedCorrectlyAnalyzer, RH0352NegativeSignsMustBeSpacedCorrectlyCodeFixProvider>
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
                                        int value = -{|#0: |}1;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = -1;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0352NegativeSignsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0352MessageFormat));
    }
}