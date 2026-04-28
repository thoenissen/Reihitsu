using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0363OpeningBraceMustNotBeFollowedByBlankLineAnalyzer"/> and <see cref="RH0363OpeningBraceMustNotBeFollowedByBlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0363OpeningBraceMustNotBeFollowedByBlankLineAnalyzerTests : AnalyzerTestsBase<RH0363OpeningBraceMustNotBeFollowedByBlankLineAnalyzer, RH0363OpeningBraceMustNotBeFollowedByBlankLineCodeFixProvider>
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
                                {|#0:
                                |}        int value = 0;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0363OpeningBraceMustNotBeFollowedByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0363MessageFormat));
    }

    /// <summary>
    /// Verifies that blank lines inside raw strings do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRawStringsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    string Property => """
                                                       {

                                                       }
                                                       """;
                                }
                                """";

        await Verify(testData);
    }
}