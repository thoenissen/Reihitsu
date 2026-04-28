using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0375CodeMustNotContainMultipleStatementsOnOneLineAnalyzer"/> and <see cref="RH0375CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0375CodeMustNotContainMultipleStatementsOnOneLineAnalyzerTests : AnalyzerTestsBase<RH0375CodeMustNotContainMultipleStatementsOnOneLineAnalyzer, RH0375CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider>
{
    /// <summary>
    /// Verifies that separate-line statements do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenStatementsAreOnSeparateLines()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 1;
                                        int second = 2;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiple statements on one line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleStatementsOnOneLineAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 1; {|#0:int second = 2;|}
                                    }
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int first = 1;
                                         int second = 2;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0375CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId, AnalyzerResources.RH0375MessageFormat));
    }
}