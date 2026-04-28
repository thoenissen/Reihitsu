using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0340OpeningParenthesisMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH0340OpeningParenthesisMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0340OpeningParenthesisMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0340OpeningParenthesisMustBeSpacedCorrectlyAnalyzer, RH0340OpeningParenthesisMustBeSpacedCorrectlyCodeFixProvider>
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
                                    int Method()
                                    {
                                        return ({|#0: |}0);
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method()
                                     {
                                         return (0);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0340OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0340MessageFormat));
    }
}