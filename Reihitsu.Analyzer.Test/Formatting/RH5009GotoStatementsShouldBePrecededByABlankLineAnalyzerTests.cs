using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5009GotoStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer, RH5009GotoStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a goto statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForGotoStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5009
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:goto|} Done;

                                        Done:
                                        value++;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5009
                                 {
                                     public void Execute()
                                     {
                                         var value = 1;

                                         goto Done;

                                         Done:
                                         value++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5009MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a goto case statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForGotoCaseStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5009
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 0:
                                                value++;
                                                {|#0:goto|} case 1;
                                            case 1:
                                                break;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5009
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 0:
                                                 value++;

                                                 goto case 1;
                                             case 1:
                                                 break;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5009MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a goto default statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForGotoDefaultStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5009
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 0:
                                                value++;
                                                {|#0:goto|} default;
                                            default:
                                                break;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5009
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 0:
                                                 value++;

                                                 goto default;
                                             default:
                                                 break;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5009MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a goto statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForGotoStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5009
                                {
                                    public void Execute()
                                    {
                                        var value = 1;

                                        goto Done;

                                        Done:
                                        value++;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a goto statement follows a label
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForGotoStatementAfterLabel()
    {
        const string testCode = """
                                internal class RH5009
                                {
                                    public void Execute()
                                    {
                                        Start:
                                        goto Done;

                                        Done:
                                        return;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a goto statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForGotoStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5009
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        // Comment before goto
                                        goto Done;

                                        Done:
                                        value++;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}