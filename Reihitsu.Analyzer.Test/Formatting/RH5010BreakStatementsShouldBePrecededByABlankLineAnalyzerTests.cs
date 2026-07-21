using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5010BreakStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer, RH5010BreakStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a break statement directly follows another statement in a loop body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForBreakStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5010
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
                                 internal class RH5010
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

        await Verify(testCode, fixedCode, Diagnostics(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5010MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a break statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBreakStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5010
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
    /// Verifies no diagnostics are reported for break statements inside a switch section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBreakStatementInSwitchSection()
    {
        const string testCode = """
                                internal class RH5010
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
    /// Verifies a diagnostic is reported when a comment line (rather than a whitespace-only blank line) directly precedes the statement, matching the formatter's whitespace-only blank-line definition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForBreakStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5010
                                {
                                    public void StopLoop()
                                    {
                                        while (true)
                                        {
                                            var shouldStop = true;
                                            // Comment before break
                                            {|#0:break|};
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5010
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

        await Verify(testCode, fixedCode, Diagnostics(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5010MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a break statement is the unbraced embedded body of a while statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBreakStatementAsEmbeddedWhileBody()
    {
        const string testCode = """
                                internal class RH5010
                                {
                                    public void StopLoop(bool condition)
                                    {
                                        while (condition)
                                            break;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}