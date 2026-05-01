using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0306DoStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0306DoStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer, RH0306DoStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies diagnostics are reported when a do statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForDoStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0306
                                {
                                    public void Execute()
                                    {
                                        var index = 0;
                                        {|#0:do|}
                                        {
                                            index++;
                                        }
                                        while (index < 3);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0306
                                 {
                                     public void Execute()
                                     {
                                         var index = 0;

                                         do
                                         {
                                             index++;
                                         }
                                         while (index < 3);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0306MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a do statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDoStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0306
                                {
                                    public void Execute()
                                    {
                                        var index = 0;

                                        do
                                        {
                                            index++;
                                        }
                                        while (index < 3);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a do statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDoStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH0306
                                {
                                    public void Execute()
                                    {
                                        do
                                        {
                                        }
                                        while (false);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a do statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDoStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH0306
                                {
                                    public void Execute()
                                    {
                                        var index = 0;
                                        // Comment before do
                                        do
                                        {
                                            index++;
                                        }
                                        while (index < 1);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Members
}