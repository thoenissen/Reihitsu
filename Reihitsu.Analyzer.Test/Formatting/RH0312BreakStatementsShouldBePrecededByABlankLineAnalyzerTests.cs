using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0312BreakStatementsShouldBePrecededByABlankLineCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer, RH0312BreakStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifies diagnostics are reported when a break statement directly follows another statement in a loop body.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForBreakStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0312
                                {
                                    public void StopLoop()
                                    {
                                        while (true)
                                        {
                                            var shouldStop = true;
                                            {|#0:break|};
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0312
                                 {
                                     public void StopLoop()
                                     {
                                         while (true)
                                         {
                                             var shouldStop = true;

                                             break;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0312MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a break statement already has a preceding blank line.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBreakStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0312
                                {
                                    public void StopLoop()
                                    {
                                        while (true)
                                        {
                                            var shouldStop = true;

                                            break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for break statements inside a switch section.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBreakStatementInSwitchSection()
    {
        const string testCode = """
                                internal class RH0312
                                {
                                    public void StopSwitch(int choice)
                                    {
                                        switch (choice)
                                        {
                                            case 1:
                                                var value = choice;
                                                break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a break statement directly follows a comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBreakStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH0312
                                {
                                    public void StopLoop()
                                    {
                                        while (true)
                                        {
                                            var shouldStop = true;
                                            // Comment before break
                                            break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }
}