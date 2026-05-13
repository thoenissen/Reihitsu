using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0394BlankLineAfterClosingBraceAnalyzer"/> and <see cref="RH0394BlankLineAfterClosingBraceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0394BlankLineAfterClosingBraceAnalyzerTests : AnalyzerTestsBase<RH0394BlankLineAfterClosingBraceAnalyzer, RH0394BlankLineAfterClosingBraceCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies diagnostics are reported when a statement directly follows an if block without a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenStatementFollowsIfBlockWithoutBlankLine()
    {
        const string testCode = """
                                internal class RH0394
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
                                 internal class RH0394
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

        await Verify(testCode, fixedCode, Diagnostics(RH0394BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH0394MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a statement directly follows a while block without a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenStatementFollowsWhileBlockWithoutBlankLine()
    {
        const string testCode = """
                                internal class RH0394
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
                                 internal class RH0394
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

        await Verify(testCode, fixedCode, Diagnostics(RH0394BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH0394MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a statement directly follows a try block without a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenStatementFollowsTryBlockWithoutBlankLine()
    {
        const string testCode = """
                                internal class RH0394
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
                                 internal class RH0394
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

        await Verify(testCode, fixedCode, Diagnostics(RH0394BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH0394MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a statement inside a switch section directly follows a block without a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenStatementInSwitchSectionFollowsBlockWithoutBlankLine()
    {
        const string testCode = """
                                internal class RH0394
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
                                 internal class RH0394
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

        await Verify(testCode, fixedCode, Diagnostics(RH0394BlankLineAfterClosingBraceAnalyzer.DiagnosticId, AnalyzerResources.RH0394MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a blank line is already present after a closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenBlankLineAlreadyPresent()
    {
        const string testCode = """
                                internal class RH0394
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
                                internal class RH0394
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
                                internal class RH0394
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
                                internal class RH0394
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
                                internal class RH0394
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
                                internal class RH0394
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
                                internal class RH0394
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

    #endregion // Members
}