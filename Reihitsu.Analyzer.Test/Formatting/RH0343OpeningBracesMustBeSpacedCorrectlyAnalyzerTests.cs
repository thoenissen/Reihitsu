using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0343OpeningBracesMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH0343OpeningBracesMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0343OpeningBracesMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0343OpeningBracesMustBeSpacedCorrectlyAnalyzer, RH0343OpeningBracesMustBeSpacedCorrectlyCodeFixProvider>
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
                                    int Property{|#0:{|} get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Property { get; set; }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0343OpeningBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0343MessageFormat));
    }

    /// <summary>
    /// Verifies that interpolated strings do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInterpolatedStringsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    string Method(int value)
                                    {
                                        return $"Value: {value}";
                                    }
                                }
                                """;

        await Verify(testData);
    }
}