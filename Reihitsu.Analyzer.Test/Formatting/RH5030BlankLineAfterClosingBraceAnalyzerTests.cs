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
public class RH5030BlankLineAfterClosingBraceAnalyzerTests : AnalyzerTestsBase<RH5030BlankLineAfterClosingBraceAnalyzer, RH5030BlankLineAfterClosingBraceCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a statement directly follows an if block without a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// follows the closing brace, rather than inside the region above the directive (issue #415)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies no diagnostics are reported for a break statement that follows a closing brace when both
    /// statements are inside a single block that is itself the braced body of a switch section (issue #440)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBreakAfterClosingBraceInsideBracedSwitchSection()
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
    /// content (issue #440)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// (in either order) never inserts a second blank line (PR #546 review)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// applying both rules' code fixes (PR #546 review)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}