using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0337DocumentationLinesMustBeginWithSingleSpaceAnalyzer"/> and <see cref="RH0337DocumentationLinesMustBeginWithSingleSpaceCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0337DocumentationLinesMustBeginWithSingleSpaceAnalyzerTests : AnalyzerTestsBase<RH0337DocumentationLinesMustBeginWithSingleSpaceAnalyzer, RH0337DocumentationLinesMustBeginWithSingleSpaceCodeFixProvider>
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
                                    /// Summary.
                                    void Method()
                                    {
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
                                    {|#0:///|}Summary.
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// Summary.
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0337DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId, AnalyzerResources.RH0337MessageFormat));
    }
}