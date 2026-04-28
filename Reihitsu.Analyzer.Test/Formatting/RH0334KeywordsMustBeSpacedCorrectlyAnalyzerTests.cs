using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0334KeywordsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH0334KeywordsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0334KeywordsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0334KeywordsMustBeSpacedCorrectlyAnalyzer, RH0334KeywordsMustBeSpacedCorrectlyCodeFixProvider>
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
                                        {|#0:if|}(true)
                                        {
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
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0334KeywordsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0334MessageFormat));
    }

    /// <summary>
    /// Verifies that operator-like keywords do not require a separating space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTypeofDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    System.Type Method()
                                    {
                                        return typeof(object);
                                    }
                                }
                                """;

        await Verify(testData);
    }
}