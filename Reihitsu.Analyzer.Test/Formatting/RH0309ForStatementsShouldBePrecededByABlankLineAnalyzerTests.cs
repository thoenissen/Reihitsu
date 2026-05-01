using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0309ForStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0309ForStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer, RH0309ForStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies diagnostics are reported when a for statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForForStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0309
                                {
                                    public void Execute()
                                    {
                                        var count = 3;
                                        {|#0:for|} (var index = 0; index < count; index++)
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0309
                                 {
                                     public void Execute()
                                     {
                                         var count = 3;

                                         for (var index = 0; index < count; index++)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0309MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a for statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForForStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0309
                                {
                                    public void Execute()
                                    {
                                        var count = 3;

                                        for (var index = 0; index < count; index++)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a for statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForForStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH0309
                                {
                                    public void Execute()
                                    {
                                        for (var index = 0; index < 1; index++)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a for statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForForStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH0309
                                {
                                    public void Execute()
                                    {
                                        var limit = 2;
                                        // Comment before for
                                        for (var index = 0; index < limit; index++)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Members
}