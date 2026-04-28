using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0308ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer, RH0308ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifies diagnostics are reported when a foreach statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForForeachStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0308
                                {
                                    public void Execute()
                                    {
                                        var items = new[] { 1, 2, 3 };
                                        {|#0:foreach|} (var item in items)
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0308
                                 {
                                     public void Execute()
                                     {
                                         var items = new[] { 1, 2, 3 };

                                         foreach (var item in items)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0308MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a foreach statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForForeachStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0308
                                {
                                    public void Execute()
                                    {
                                        var items = new[] { 1, 2, 3 };

                                        foreach (var item in items)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a foreach statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForForeachStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH0308
                                {
                                    public void Execute()
                                    {
                                        foreach (var item in new[] { 1, 2, 3 })
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a foreach statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForForeachStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH0308
                                {
                                    public void Execute()
                                    {
                                        var values = new[] { 1, 2 };
                                        // Comment before foreach
                                        foreach (var value in values)
                                        {
                                            Consume(value);
                                        }
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }
}