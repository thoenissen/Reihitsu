using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0304IfStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0304IfStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0304IfStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0304IfStatementsShouldBePrecededByABlankLineAnalyzer, RH0304IfStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies diagnostics are reported when an if statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForIfStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0304
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:if|} (value > 0)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0304
                                 {
                                     public void Execute()
                                     {
                                         var value = 1;

                                         if (value > 0)
                                         {
                                             value++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0304IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0304MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an if statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForIfStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0304
                                {
                                    public void Execute()
                                    {
                                        var value = 1;

                                        if (value > 0)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an if statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForIfStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH0304
                                {
                                    public void Execute()
                                    {
                                        if (true)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an if statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForIfStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH0304
                                {
                                    public void Execute(bool isReady)
                                    {
                                        var value = 1;
                                        // Comment before if
                                        if (isReady)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Members
}