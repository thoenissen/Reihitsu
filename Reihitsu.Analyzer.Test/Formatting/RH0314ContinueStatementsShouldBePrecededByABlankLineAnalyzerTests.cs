using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0314ContinueStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0314ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0314ContinueStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0314ContinueStatementsShouldBePrecededByABlankLineAnalyzer, RH0314ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies diagnostics are reported when a continue statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForContinueStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0314
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            var shouldContinue = true;
                                            {|#0:continue|};
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0314
                                 {
                                     public void Iterate()
                                     {
                                         while (true)
                                         {
                                             var shouldContinue = true;

                                             continue;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0314ContinueStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0314MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a continue statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForContinueStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0314
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            var shouldContinue = true;

                                            continue;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a continue statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForContinueStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH0314
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a continue statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForContinueStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH0314
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            var shouldContinue = true;
                                            // Comment before continue
                                            continue;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Members
}