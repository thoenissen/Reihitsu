using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0367OpeningBraceMustNotBePrecededByBlankLineAnalyzer"/> and <see cref="RH0367OpeningBraceMustNotBePrecededByBlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0367OpeningBraceMustNotBePrecededByBlankLineAnalyzerTests : AnalyzerTestsBase<RH0367OpeningBraceMustNotBePrecededByBlankLineAnalyzer, RH0367OpeningBraceMustNotBePrecededByBlankLineCodeFixProvider>
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
                                {|#0:
                                |}    {
                                        int value = 0;
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

        await Verify(testData, fixedData, Diagnostics(RH0367OpeningBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0367MessageFormat));
    }

    /// <summary>
    /// Verifies that raw-string content does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRawStringOpeningBraceDoesNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    private const string Value = """
                                                                 
                                                                 {
                                                                 """;
                                }
                                """";

        await Verify(testData);
    }
}