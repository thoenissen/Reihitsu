using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0316SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer, RH0316SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifies diagnostics are reported when a switch statement directly follows another statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForSwitchStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0316
                                {
                                    public int Execute(int number)
                                    {
                                        var offset = 1;
                                        {|#0:switch|} (number)
                                        {
                                            case 0:
                                                return offset;
                                            default:
                                                return number;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0316
                                 {
                                     public int Execute(int number)
                                     {
                                         var offset = 1;

                                         switch (number)
                                         {
                                             case 0:
                                                 return offset;
                                             default:
                                                 return number;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0316MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a switch statement already has a preceding blank line.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSwitchStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0316
                                {
                                    public int Execute(int number)
                                    {
                                        var offset = 1;

                                        switch (number)
                                        {
                                            case 0:
                                                return offset;
                                            default:
                                                return number;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a switch statement is the first statement in a block.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSwitchStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH0316
                                {
                                    public int Execute(int number)
                                    {
                                        switch (number)
                                        {
                                            case 0:
                                                return 0;
                                            default:
                                                return number;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a switch statement directly follows a comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSwitchStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH0316
                                {
                                    public int Execute(int number)
                                    {
                                        var offset = 1;
                                        // Comment before switch
                                        switch (number)
                                        {
                                            case 0:
                                                return offset;
                                            default:
                                                return number;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }
}