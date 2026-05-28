using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5004DoStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5004DoStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5004DoStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5004DoStatementsShouldBePrecededByABlankLineAnalyzer, RH5004DoStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a do statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForDoStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5004
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
                                 internal class RH5004
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

        await Verify(testCode, fixedCode, Diagnostics(RH5004DoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5004MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a do statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDoStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5004
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
                                internal class RH5004
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
                                internal class RH5004
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

    #endregion // Tests
}