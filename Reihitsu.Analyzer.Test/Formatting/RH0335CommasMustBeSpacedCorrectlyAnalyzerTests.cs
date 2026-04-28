using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0335CommasMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH0335CommasMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0335CommasMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0335CommasMustBeSpacedCorrectlyAnalyzer, RH0335CommasMustBeSpacedCorrectlyCodeFixProvider>
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
                                    void Method(int x, int y)
                                    {
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
                                    void Method(int x{|#0:,|}int y)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x, int y)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0335CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0335MessageFormat));
    }

    /// <summary>
    /// Verifies that array-rank commas do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyArrayRankCommasDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int[,] Method()
                                    {
                                        return new int[1, 1];
                                    }
                                }
                                """;

        await Verify(testData);
    }
}