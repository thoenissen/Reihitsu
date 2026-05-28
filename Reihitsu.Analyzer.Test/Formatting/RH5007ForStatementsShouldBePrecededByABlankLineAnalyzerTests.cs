using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5007ForStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5007ForStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5007ForStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5007ForStatementsShouldBePrecededByABlankLineAnalyzer, RH5007ForStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a for statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForForStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5007
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
                                 internal class RH5007
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

        await Verify(testCode, fixedCode, Diagnostics(RH5007ForStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5007MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a for statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForForStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5007
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
                                internal class RH5007
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
                                internal class RH5007
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

    #endregion // Tests
}