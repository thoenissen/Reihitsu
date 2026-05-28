using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5012ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer, RH5012ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a continue statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForContinueStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5012
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
                                 internal class RH5012
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

        await Verify(testCode, fixedCode, Diagnostics(RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5012MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a continue statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForContinueStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5012
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
                                internal class RH5012
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
                                internal class RH5012
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

    #endregion // Tests
}