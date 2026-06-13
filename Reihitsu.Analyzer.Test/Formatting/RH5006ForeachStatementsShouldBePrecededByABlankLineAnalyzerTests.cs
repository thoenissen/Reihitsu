using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5006ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer, RH5006ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a foreach statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForForeachStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5006
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
                                 internal class RH5006
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

        await Verify(testCode, fixedCode, Diagnostics(RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5006MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported when a deconstruction foreach statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForDeconstructionForeachStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH5006
                                {
                                    public void Execute(List<(int A, int B)> items)
                                    {
                                        var count = 0;
                                        {|#0:foreach|} (var (a, b) in items)
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;

                                 internal class RH5006
                                 {
                                     public void Execute(List<(int A, int B)> items)
                                     {
                                         var count = 0;

                                         foreach (var (a, b) in items)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5006MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a foreach statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForForeachStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5006
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
                                internal class RH5006
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
                                internal class RH5006
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

    #endregion // Tests
}