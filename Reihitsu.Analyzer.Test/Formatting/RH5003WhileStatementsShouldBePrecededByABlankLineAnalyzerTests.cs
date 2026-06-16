using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5003WhileStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzer, RH5003WhileStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a while statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForWhileStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5003
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
                                 internal class RH5003
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

        await Verify(testCode, fixedCode, Diagnostics(RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5003MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a while statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForWhileStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5003
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
                                internal class RH5003
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
    /// Verifies a diagnostic is reported when a comment line (rather than a whitespace-only blank line) directly precedes the statement, matching the formatter's whitespace-only blank-line definition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForWhileStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5003
                                {
                                    public void Execute(bool keepRunning)
                                    {
                                        var value = 1;
                                        // Comment before while
                                        {|#0:while|} (keepRunning)
                                        {
                                            value++;
                                            break;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5003
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

        await Verify(testCode, fixedCode, Diagnostics(RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5003MessageFormat));
    }

    #endregion // Tests
}