using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0310ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer, RH0310ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies diagnostics are reported when a return statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForReturnStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0310
                                {
                                    public int Execute()
                                    {
                                        var value = 1;
                                        {|#0:return|} value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0310
                                 {
                                     public int Execute()
                                     {
                                         var value = 1;

                                         return value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0310MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a return statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0310
                                {
                                    public int Execute()
                                    {
                                        var value = 1;

                                        return value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a return statement follows a case label
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnStatementAfterCaseLabel()
    {
        const string testCode = """
                                internal class RH0310
                                {
                                    public int Execute()
                                    {
                                        switch (1)
                                        {
                                            case 1:
                                                return 1;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a return statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH0310
                                {
                                    public int Execute()
                                    {
                                        var value = 1;
                                        // Comment before return
                                        return value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Members
}