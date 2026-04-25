using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0365CodeMustNotContainMultipleBlankLinesInARowAnalyzer"/> and <see cref="RH0365CodeMustNotContainMultipleBlankLinesInARowCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0365CodeMustNotContainMultipleBlankLinesInARowAnalyzerTests : AnalyzerTestsBase<RH0365CodeMustNotContainMultipleBlankLinesInARowAnalyzer, RH0365CodeMustNotContainMultipleBlankLinesInARowCodeFixProvider>
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
                                        int first = 0;
                                
                                {|#0:
                                |}        int second = 1;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int first = 0;
                                 
                                         int second = 1;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0365CodeMustNotContainMultipleBlankLinesInARowAnalyzer.DiagnosticId, AnalyzerResources.RH0365MessageFormat));
    }

    /// <summary>
    /// Verifies that multiple blank lines inside raw strings do not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyRawStringsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    string Property => """
                                                       First


                                                       Second
                                                       """;
                                }
                                """";

        await Verify(testData);
    }
}