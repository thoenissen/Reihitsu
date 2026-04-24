using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0374UseBracesConsistentlyAnalyzer"/> and <see cref="RH0374UseBracesConsistentlyCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0374UseBracesConsistentlyAnalyzerTests : AnalyzerTestsBase<RH0374UseBracesConsistentlyAnalyzer, RH0374UseBracesConsistentlyCodeFixProvider>
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
                                            return;
                                        }
                                        else
                                        {
                                            return;
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
                                        if (true)
                                        {
                                            return;
                                        }
                                        else
                                            {|#0:return;|}
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
                                             return;
                                         }
                                         else
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0374UseBracesConsistentlyAnalyzer.DiagnosticId, AnalyzerResources.RH0374MessageFormat));
    }
}