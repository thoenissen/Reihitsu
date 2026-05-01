using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0303TryStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0303TryStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0303TryStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0303TryStatementsShouldBePrecededByABlankLineAnalyzer, RH0303TryStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies diagnostics are reported when a try statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForTryStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0303
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:try|}
                                        {
                                            value++;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0303
                                 {
                                     public void Execute()
                                     {
                                         var value = 1;

                                         try
                                         {
                                             value++;
                                         }
                                         catch
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0303TryStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0303MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a try statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForTryStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0303
                                {
                                    public void Execute()
                                    {
                                        var value = 1;

                                        try
                                        {
                                            value++;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a try statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForTryStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH0303
                                {
                                    public void Execute()
                                    {
                                        try
                                        {
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a try statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForTryStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH0303
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        // Comment before try
                                        try
                                        {
                                            value++;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Members
}