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
    /// Verifies diagnostics are reported and fixed for a break statement inside a block that is itself the braced
    /// body of a switch section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndFixForBreakStatementInsideBracedSwitchSection()
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
                                                    {|#0:break|};
                                                }
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
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

        await Verify(testCode, fixedCode, Diagnostics(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5010MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported and fixed when a break follows a nested block inside the braced body of a
    /// switch section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndFixForBreakAfterClosingBraceInsideBracedSwitchSection()
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
                                                    if (choice > 0)
                                                    {
                                                        Consume();
                                                    }
                                                    {|#0:break|};
                                                }
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
                                     public void StopSwitch(int choice)
                                     {
                                         switch (choice)
                                         {
                                             case 1:
                                                 {
                                                     if (choice > 0)
                                                     {
                                                         Consume();
                                                     }

                                                     break;
                                                 }
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
    /// Verifies no diagnostics are reported when a break is the first statement inside a braced switch-section body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFirstBreakStatementInsideBracedSwitchSection()
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
                                                    break;
                                                }
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the existing directive exemption remains in effect inside a braced switch-section body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBreakAfterDirectiveInsideBracedSwitchSection()
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
                                #if true
                                                    break;
                                #endif
                                                }
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies Fix All inserts one blank line before every affected break in braced switch-section bodies
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixAllConvergesForBreakStatementsInsideBracedSwitchSections()
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
                                                    var first = choice;
                                                    {|#0:break|};
                                                }

                                            case 2:
                                                {
                                                    var second = choice;
                                                    {|#1:break|};
                                                }
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5010
                                 {
                                     public void StopSwitch(int choice)
                                     {
                                         switch (choice)
                                         {
                                             case 1:
                                                 {
                                                     var first = choice;

                                                     break;
                                                 }

                                             case 2:
                                                 {
                                                     var second = choice;

                                                     break;
                                                 }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5010MessageFormat, 2));
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

    /// <summary>
    /// Verifies RH5010 remains silent after RH5030 inserts the required blank line inside a braced switch-section
    /// body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticAfterRH5030FixInsideBracedSwitchSection()
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
                                                    if (choice > 0)
                                                    {
                                                        Consume();
                                                    }

                                                    break;
                                                }
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a same-line break inside a braced switch-section body is fixed to a converged blank-line gap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndFixForSameLineBreakInsideBracedSwitchSection()
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
                                                    if (choice > 0)
                                                    {
                                                        Consume();
                                                    } {|#0:break|};
                                                }
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
                                     public void StopSwitch(int choice)
                                     {
                                         switch (choice)
                                         {
                                             case 1:
                                                 {
                                                     if (choice > 0)
                                                     {
                                                         Consume();
                                                     }

                                                     break;
                                                 }
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config => config.NumberOfIncrementalIterations = 1,
                     Diagnostics(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5010MessageFormat));
    }

    /// <summary>
    /// Verifies a same-line block comment is preserved when the code fix creates the required blank-line gap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySameLineBlockCommentIsPreservedInsideBracedSwitchSection()
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
                                                    if (choice > 0)
                                                    {
                                                        Consume();
                                                    } /* Keep this comment. */ {|#0:break|};
                                                }
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
                                     public void StopSwitch(int choice)
                                     {
                                         switch (choice)
                                         {
                                             case 1:
                                                 {
                                                     if (choice > 0)
                                                     {
                                                         Consume();
                                                     } /* Keep this comment. */

                                                     break;
                                                 }
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config => config.NumberOfIncrementalIterations = 1,
                     Diagnostics(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5010MessageFormat));
    }

    /// <summary>
    /// Verifies an embedded line break in a same-line block comment does not substitute for the line terminator and
    /// blank line required after the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultilineBlockCommentConvergesInsideBracedSwitchSection()
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
                                                    if (choice > 0)
                                                    {
                                                        Consume();
                                                    } /* Keep
                                comment. */ {|#0:break|};
                                                }
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
                                     public void StopSwitch(int choice)
                                     {
                                         switch (choice)
                                         {
                                             case 1:
                                                 {
                                                     if (choice > 0)
                                                     {
                                                         Consume();
                                                     } /* Keep
                                 comment. */

                                                     break;
                                                 }
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config => config.NumberOfIncrementalIterations = 1,
                     Diagnostics(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5010MessageFormat));
    }

    /// <summary>
    /// Verifies a same-line fix uses the target statement's syntax depth when its enclosing block starts after other
    /// code on the physical line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySameLineFixUsesNestedBlockIndentationInsideSwitchSection()
    {
        const string testCode = """
                                internal class RH5010
                                {
                                    public void StopSwitch(int choice)
                                    {
                                        switch (choice)
                                        {
                                            case 1: { Consume(); {|#0:break|}; }
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
                                     public void StopSwitch(int choice)
                                     {
                                         switch (choice)
                                         {
                                             case 1: { Consume();

                                                     break; }
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config => config.NumberOfIncrementalIterations = 1,
                     Diagnostics(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5010MessageFormat));
    }

    /// <summary>
    /// Verifies a direct switch-section break is diagnosed and fixed when a following statement makes it
    /// non-terminal
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndFixForNonTerminalDirectSwitchSectionBreak()
    {
        const string testCode = """
                                #pragma warning disable CS0162

                                internal class RH5010
                                {
                                    public void StopSwitch(int choice)
                                    {
                                        switch (choice)
                                        {
                                            case 1:
                                                Consume();
                                                {|#0:break|};
                                                Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 #pragma warning disable CS0162

                                 internal class RH5010
                                 {
                                     public void StopSwitch(int choice)
                                     {
                                         switch (choice)
                                         {
                                             case 1:
                                                 Consume();

                                                 break;
                                                 Consume();
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config => config.NumberOfIncrementalIterations = 1,
                     Diagnostics(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5010MessageFormat));
    }

    /// <summary>
    /// Verifies RH5010 reports and fixes a non-terminal direct switch-section break that follows a closing brace,
    /// matching RH5030 on the shared gap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndFixForNonTerminalDirectBreakAfterClosingBrace()
    {
        const string testCode = """
                                #pragma warning disable CS0162

                                internal class RH5010
                                {
                                    public void StopSwitch(int choice)
                                    {
                                        switch (choice)
                                        {
                                            case 1:
                                                if (choice > 0)
                                                {
                                                    Consume();
                                                }
                                                {|#0:break|};
                                                Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 #pragma warning disable CS0162

                                 internal class RH5010
                                 {
                                     public void StopSwitch(int choice)
                                     {
                                         switch (choice)
                                         {
                                             case 1:
                                                 if (choice > 0)
                                                 {
                                                     Consume();
                                                 }

                                                 break;
                                                 Consume();
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config => config.NumberOfIncrementalIterations = 1,
                     Diagnostics(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5010MessageFormat));
    }

    /// <summary>
    /// Verifies a non-terminal direct break is not diagnosed when it is the switch section's first statement because
    /// no preceding statement gap exists
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFirstNonTerminalDirectSwitchSectionBreak()
    {
        const string testCode = """
                                #pragma warning disable CS0162

                                internal class RH5010
                                {
                                    public void StopSwitch(int choice)
                                    {
                                        switch (choice)
                                        {
                                            case 1:
                                                break;
                                                Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies Fix All inserts one blank line before every non-terminal direct switch-section break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixAllConvergesForNonTerminalDirectSwitchSectionBreaks()
    {
        const string testCode = """
                                #pragma warning disable CS0162

                                internal class RH5010
                                {
                                    public void StopSwitch(int choice)
                                    {
                                        switch (choice)
                                        {
                                            case 1:
                                                Consume();
                                                {|#0:break|};
                                                Consume();

                                            case 2:
                                                Consume();
                                                {|#1:break|};
                                                Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 #pragma warning disable CS0162

                                 internal class RH5010
                                 {
                                     public void StopSwitch(int choice)
                                     {
                                         switch (choice)
                                         {
                                             case 1:
                                                 Consume();

                                                 break;
                                                 Consume();

                                             case 2:
                                                 Consume();

                                                 break;
                                                 Consume();
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5010MessageFormat, 2));
    }

    #endregion // Tests
}