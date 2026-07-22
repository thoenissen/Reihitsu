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

    /// <summary>
    /// Verifies no diagnostics are reported for a break statement inside a block that is itself the braced body
    /// of a switch section, even when the statement immediately preceding it is not a blank line (issue #440)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBreakStatementInsideBracedSwitchSection()
    {
        const string testCode = """
                                internal class RH5010
                                {
                                    public void StopSwitch(int choice)
                                    {
                                        switch (choice)
                                        {
                                            case 1:
                                                {
                                                    var value = choice;
                                                    break;
                                                }
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a break statement that directly follows a closing brace outside a
    /// switch section, matching the formatter, which does not exempt break statements based on what precedes
    /// them outside switch sections (issue #440)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForBreakStatementAfterClosingBraceOutsideSwitchSection()
    {
        const string testCode = """
                                internal class RH5010
                                {
                                    public void StopLoop(bool flag)
                                    {
                                        while (true)
                                        {
                                            if (flag)
                                            {
                                                Consume();
                                            }
                                            {|#0:break|};
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5010
                                 {
                                     public void StopLoop(bool flag)
                                     {
                                         while (true)
                                         {
                                             if (flag)
                                             {
                                                 Consume();
                                             }

                                             break;
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5010MessageFormat));
    }

    /// <summary>
    /// Verifies that once RH5030's code fix inserts the blank line after the closing brace that precedes a break
    /// statement outside a switch section, RH5010 no longer reports a diagnostic on the same code. Combined with
    /// <see cref="VerifyDiagnosticForBreakStatementAfterClosingBraceOutsideSwitchSection"/> and
    /// <see cref="RH5030BlankLineAfterClosingBraceAnalyzerTests.VerifyDiagnosticAndFixForBreakStatementAfterClosingBraceOutsideSwitchSection"/>,
    /// this shows fixing either rule's diagnostic first always satisfies the other, so no double blank line can
    /// result from applying both rules' code fixes (PR #546 review)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticAfterRH5030FixInsertsBlankLineBeforeBreak()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute(bool flag)
                                    {
                                        while (true)
                                        {
                                            if (flag)
                                            {
                                                Consume();
                                            }

                                            break;
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}