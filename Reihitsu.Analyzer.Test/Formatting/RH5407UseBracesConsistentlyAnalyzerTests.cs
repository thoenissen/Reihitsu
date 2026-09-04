using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5407UseBracesConsistentlyAnalyzer"/> and <see cref="RH5407UseBracesConsistentlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5407UseBracesConsistentlyAnalyzerTests : BatchCodeFixTestsBase<RH5407UseBracesConsistentlyAnalyzer, RH5407UseBracesConsistentlyCodeFixProvider>
{
    #region Tests

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

        await Verify(testData, fixedData, Diagnostics(RH5407UseBracesConsistentlyAnalyzer.DiagnosticId, AnalyzerResources.RH5407MessageFormat));
    }

    /// <summary>
    /// Verifies that else-if chains do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyElseIfChainsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool first, bool second)
                                    {
                                        if (first)
                                        {
                                            return;
                                        }
                                        else if (second)
                                            return;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is fixed without deleting the else keyword when the child shares the else line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsFixedWhenChildSharesElseLine()
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
                                        else {|#0:return;|}
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

        await Verify(testData, fixedData, Diagnostics(RH5407UseBracesConsistentlyAnalyzer.DiagnosticId, AnalyzerResources.RH5407MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void Method1()
                                    {
                                        if (true)
                                        {
                                            return;
                                        }
                                        else
                                            {|#0:return;|}
                                    }

                                    void Method2()
                                    {
                                        if (true)
                                        {
                                            return;
                                        }
                                        else
                                            {|#1:return;|}
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     void Method1()
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

                                     void Method2()
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

        // The rule reports at most one diagnostic per if-statement, so a shared owner between two occurrences is
        // unreachable; two independent if/else statements in different methods exercise the batch fixer applying
        // two genuinely separate reformats
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5407UseBracesConsistentlyAnalyzer.DiagnosticId, AnalyzerResources.RH5407MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}