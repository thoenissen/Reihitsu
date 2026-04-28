using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0305WhileStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer, RH0305WhileStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifies diagnostics are reported when a while statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForWhileStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0305
                                {
                                    public void Execute()
                                    {
                                        var index = 0;
                                        {|#0:while|} (index < 3)
                                        {
                                            index++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0305
                                 {
                                     public void Execute()
                                     {
                                         var index = 0;

                                         while (index < 3)
                                         {
                                             index++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0305MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a while statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForWhileStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0305
                                {
                                    public void Execute()
                                    {
                                        var index = 0;

                                        while (index < 3)
                                        {
                                            index++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a while statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForWhileStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH0305
                                {
                                    public void Execute()
                                    {
                                        while (false)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a while statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForWhileStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH0305
                                {
                                    public void Execute(bool keepRunning)
                                    {
                                        var value = 1;
                                        // Comment before while
                                        while (keepRunning)
                                        {
                                            value++;
                                            break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }
}