using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5030BlankLineAfterClosingBraceAnalyzer"/> and <see cref="RH5030BlankLineAfterClosingBraceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5030BlankLineAfterClosingBraceAnalyzerTests : BatchCodeFixTestsBase<RH5030BlankLineAfterClosingBraceAnalyzer, RH5030BlankLineAfterClosingBraceCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a statement directly follows an if block without a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("RH5030-002")]
    [Requirement("SHARED-AFTERBRACE-001")]
    [Requirement("SHARED-BLANK-003")]
    public async Task VerifyDiagnosticWhenStatementFollowsIfBlockWithoutBlankLine()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute(bool flag)
                                    {
                                        if (flag)
                                        {
                                            Consume();
                                        {|#0:}|}
                                        Consume();
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5030
                                 {
                                     public void Execute(bool flag)
                                     {
                                         if (flag)
                                         {
                                             Consume();
                                         }

                                         Consume();
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5030BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH5030MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a statement directly follows a while block without a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-AFTERBRACE-001")]
    [Requirement("SHARED-BLANK-003")]
    public async Task VerifyDiagnosticWhenStatementFollowsWhileBlockWithoutBlankLine()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute()
                                    {
                                        while (GetValue())
                                        {
                                            Consume();
                                        {|#0:}|}
                                        Consume();
                                    }

                                    private bool GetValue()
                                    {
                                        return false;
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5030
                                 {
                                     public void Execute()
                                     {
                                         while (GetValue())
                                         {
                                             Consume();
                                         }

                                         Consume();
                                     }

                                     private bool GetValue()
                                     {
                                         return false;
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5030BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH5030MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a statement directly follows a try block without a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-AFTERBRACE-001")]
    [Requirement("SHARED-BLANK-003")]
    public async Task VerifyDiagnosticWhenStatementFollowsTryBlockWithoutBlankLine()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute()
                                    {
                                        try
                                        {
                                            Consume();
                                        }
                                        catch
                                        {
                                        {|#0:}|}
                                        Consume();
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5030
                                 {
                                     public void Execute()
                                     {
                                         try
                                         {
                                             Consume();
                                         }
                                         catch
                                         {
                                         }

                                         Consume();
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5030BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH5030MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a statement inside a switch section directly follows a block without a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-AFTERBRACE-001")]
    [Requirement("SHARED-BLANK-003")]
    public async Task VerifyDiagnosticWhenStatementInSwitchSectionFollowsBlockWithoutBlankLine()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                if (GetValue())
                                                {
                                                    Consume();
                                                {|#0:}|}
                                                Consume();
                                                break;
                                        }
                                    }

                                    private bool GetValue()
                                    {
                                        return false;
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5030
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 if (GetValue())
                                                 {
                                                     Consume();
                                                 }

                                                 Consume();
                                                 break;
                                         }
                                     }

                                     private bool GetValue()
                                     {
                                         return false;
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5030BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH5030MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix inserts the blank line after an <c>#endregion</c> directive that immediately
    /// follows the closing brace, rather than inside the region above the directive.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-AFTERBRACE-001")]
    [Requirement("SHARED-BLANK-003")]
    public async Task VerifyDiagnosticWhenStatementFollowsEndRegionDirectiveWithoutBlankLine()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute(bool flag)
                                    {
                                        #region Guard
                                        if (flag)
                                        {
                                            Consume();
                                        {|#0:}|}
                                        #endregion
                                        Consume();
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5030
                                 {
                                     public void Execute(bool flag)
                                     {
                                         #region Guard
                                         if (flag)
                                         {
                                             Consume();
                                         }
                                         #endregion

                                         Consume();
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5030BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH5030MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a blank line is already present after a closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-BLANK-003")]
    public async Task VerifyNoDiagnosticWhenBlankLineAlreadyPresent()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute(bool flag)
                                    {
                                        if (flag)
                                        {
                                            Consume();
                                        }

                                        Consume();
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an else clause follows a closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    public async Task VerifyNoDiagnosticWhenElseFollowsClosingBrace()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute(bool flag)
                                    {
                                        if (flag)
                                        {
                                            Consume();
                                        }
                                        else
                                        {
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
    /// Verifies no diagnostics are reported when a catch clause follows a closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    public async Task VerifyNoDiagnosticWhenCatchFollowsClosingBrace()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute()
                                    {
                                        try
                                        {
                                            Consume();
                                        }
                                        catch
                                        {
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
    /// Verifies no diagnostics are reported when a finally clause follows a closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    public async Task VerifyNoDiagnosticWhenFinallyFollowsClosingBrace()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute()
                                    {
                                        try
                                        {
                                            Consume();
                                        }
                                        finally
                                        {
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
    /// Verifies no diagnostics are reported when a do-while's while follows a closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    public async Task VerifyNoDiagnosticWhenDoWhileWhileFollowsClosingBrace()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute()
                                    {
                                        do
                                        {
                                            Consume();
                                        }
                                        while (GetValue());
                                    }

                                    private bool GetValue()
                                    {
                                        return false;
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a closing brace is the last statement in the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-BLANK-003")]
    public async Task VerifyNoDiagnosticWhenClosingBraceIsLastStatement()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute(bool flag)
                                    {
                                        if (flag)
                                        {
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
    /// Verifies no diagnostics are reported when a break follows the main block of a switch case
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-BLANK-004")]
    public async Task VerifyNoDiagnosticWhenBreakFollowsMainBlockInSwitchCase()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
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
    /// Verifies diagnostics are reported and fixed for a break statement that follows a closing brace when both
    /// statements are inside a single block that is itself the braced body of a switch section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-AFTERBRACE-001")]
    [Requirement("SHARED-BLANK-004")]
    public async Task VerifyDiagnosticAndFixForBreakAfterClosingBraceInsideBracedSwitchSection()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    if (GetValue())
                                                    {
                                                        Consume();
                                                    {|#0:}|}
                                                    break;
                                                }
                                        }
                                    }

                                    private bool GetValue()
                                    {
                                        return false;
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5030
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 {
                                                     if (GetValue())
                                                     {
                                                         Consume();
                                                     }

                                                     break;
                                                 }
                                         }
                                     }

                                     private bool GetValue()
                                     {
                                         return false;
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5030BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH5030MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported after RH5010 inserts the required blank line inside a braced
    /// switch-section body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-BLANK-003")]
    public async Task VerifyNoDiagnosticAfterRH5010FixInsideBracedSwitchSection()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    if (GetValue())
                                                    {
                                                        Consume();
                                                    }

                                                    break;
                                                }
                                        }
                                    }

                                    private bool GetValue()
                                    {
                                        return false;
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a diagnostic is reported when a comment line separates a closing brace from the next statement,
    /// matching the formatter's blank-line definition, which only counts a line as blank when it contains no
    /// content.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-AFTERBRACE-001")]
    [Requirement("SHARED-BLANK-003")]
    public async Task VerifyDiagnosticWhenCommentLineSeparatesClosingBraceFromNextStatement()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute(bool flag)
                                    {
                                        if (flag)
                                        {
                                            Consume();
                                        {|#0:}|}
                                        // Comment after block
                                        Consume();
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5030
                                 {
                                     public void Execute(bool flag)
                                     {
                                         if (flag)
                                         {
                                             Consume();
                                         }

                                         // Comment after block
                                         Consume();
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5030BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH5030MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported and fixed when a break statement outside a switch section directly
    /// follows a closing brace. RH5010 also reports this same statement (the shared pattern already used by
    /// RH5008/RH5013, which likewise overlap RH5030 on statements that follow a closing brace); the companion
    /// tests <see cref="VerifyNoDiagnosticAfterRH5010FixInsertsBlankLineBeforeBreak"/> and
    /// <see cref="RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzerTests.VerifyNoDiagnosticAfterRH5030FixInsertsBlankLineBeforeBreak"/>
    /// verify that fixing either diagnostic first always clears the other, so applying both rules' code fixes
    /// (in either order) never inserts a second blank line.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-AFTERBRACE-001")]
    [Requirement("SHARED-BLANK-004")]
    public async Task VerifyDiagnosticAndFixForBreakStatementAfterClosingBraceOutsideSwitchSection()
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
                                            {|#0:}|}
                                            break;
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
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

        await Verify(testCode, fixedCode, Diagnostics(RH5030BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH5030MessageFormat));
    }

    /// <summary>
    /// Verifies that once RH5010's code fix inserts the blank line before a break statement that follows a
    /// closing brace outside a switch section, RH5030 no longer reports a diagnostic on the same code. Combined
    /// with <see cref="VerifyDiagnosticAndFixForBreakStatementAfterClosingBraceOutsideSwitchSection"/>, this shows
    /// fixing either rule's diagnostic first always satisfies the other, so no double blank line can result from
    /// applying both rules' code fixes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-BLANK-003")]
    public async Task VerifyNoDiagnosticAfterRH5010FixInsertsBlankLineBeforeBreak()
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
    /// Verifies RH5030 retains its same-line exclusion inside a braced switch-section body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-BLANK-003")]
    public async Task VerifyNoDiagnosticForSameLineBreakInsideBracedSwitchSection()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    if (value > 0)
                                                    {
                                                        Consume();
                                                    } break;
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
    /// Verifies a closing brace followed by a non-terminal direct switch-section break is diagnosed and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("SHARED-AFTERBRACE-001")]
    [Requirement("SHARED-BLANK-004")]
    public async Task VerifyDiagnosticAndFixForNonTerminalDirectBreakAfterClosingBrace()
    {
        const string testCode = """
                                #pragma warning disable CS0162

                                internal class RH5030
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                if (value > 0)
                                                {
                                                    Consume();
                                                {|#0:}|}
                                                break;
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

                                 internal class RH5030
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 if (value > 0)
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
                     Diagnostics(RH5030BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH5030MessageFormat));
    }

    /// <summary>
    /// Verifies a statement that ends with a closing brace is reported on that brace when another statement follows it
    /// without a blank line, also across a comment line or a directive, but not on one line or before a terminal break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5030-001")]
    [Requirement("RH5030-002")]
    [Requirement("SHARED-BLANK-003")]
    [Requirement("SHARED-BLANK-004")]
    public async Task VerifyStatementAfterClosingBraceWithoutBlankLineIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        if (a > 0)
                                        {
                                            a++;
                                        {|RH5030:}|}
                                        a++;
                                        if (a > 0)
                                        {
                                            a++;
                                        {|RH5030:}|} // note
                                        // next
                                        a++;
                                        if (a > 0)
                                        {
                                        {|RH5030:}|}
                                #pragma warning disable CS0168
                                        a++;
                                        if (a > 0) { } a++;
                                        switch (a)
                                        {
                                            case 1:
                                                {
                                                    a++;
                                                }
                                                break;
                                            case 2:
                                                {
                                                    a++;
                                                {|RH5030:}|}
                                                a++;
                                                break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix inserts the blank line directly above the next statement, below a comment that precedes it, and
    /// behind a directive that follows the brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-AFTERBRACE-001")]
    [Requirement("SHARED-BLANK-003")]
    public async Task VerifyFixInsertsBlankLineDirectlyBeforeNextStatement()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        if (a > 0)
                                        {
                                            a++;
                                        {|RH5030:}|}
                                        a++;
                                        if (a > 0)
                                        {
                                            a++;
                                        {|RH5030:}|} // note
                                        // next
                                        a++;
                                        if (a > 0)
                                        {
                                        {|RH5030:}|}
                                #pragma warning disable CS0168
                                        a++;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         if (a > 0)
                                         {
                                             a++;
                                         }

                                         a++;
                                         if (a > 0)
                                         {
                                             a++;
                                         } // note

                                         // next
                                         a++;
                                         if (a > 0)
                                         {
                                         }
                                 #pragma warning disable CS0168

                                         a++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the inserted line break uses the document's CRLF line ending
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-AFTERBRACE-001")]
    public async Task VerifyFixInsertsCarriageReturnLineFeedBlankLineBeforeNextStatement()
    {
        const string testCode = "class C\n{\n    void M(int a)\n    {\n        if (a > 0)\n        {\n        }\n        a++;\n    }\n}\n";
        const string fixedCode = "class C\n{\n    void M(int a)\n    {\n        if (a > 0)\n        {\n        }\n\n        a++;\n    }\n}\n";

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedCode), actual);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5030
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    if (value > 0)
                                                    {
                                                        Consume();
                                                    {|#0:}|}
                                                    break;
                                                }

                                            case 2:
                                                {
                                                    if (value > 1)
                                                    {
                                                        Consume();
                                                    {|#1:}|}
                                                    break;
                                                }
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5030
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 {
                                                     if (value > 0)
                                                     {
                                                         Consume();
                                                     }

                                                     break;
                                                 }

                                             case 2:
                                                 {
                                                     if (value > 1)
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

        // Verifies Fix All inserts one blank line after every affected closing brace in braced switch-section bodies
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5030BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH5030MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}