using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer"/> and <see cref="RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests : AnalyzerTestsBase<RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer, RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsCodeFixProvider>
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
                                        if (true)
                                            {|#0:Other(1,
                                                  2);|}
                                    }
                                
                                    void Other(int value1, int value2)
                                    {
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
                                             Other(1,
                                                   2);
                                         }
                                     }
                                 
                                     void Other(int value1, int value2)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer.DiagnosticId, AnalyzerResources.RH0373MessageFormat));
    }
}