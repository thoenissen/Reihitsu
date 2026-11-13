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
public class RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer, RH5006ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a foreach statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5006-001")]
    [Requirement("RH5006-002")]
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
    [Requirement("RH5006-001")]
    [Requirement("RH5006-002")]
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
    [Requirement("RH5006-001")]
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
    [Requirement("RH5006-001")]
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
    /// Verifies a diagnostic is reported when a comment line (rather than a whitespace-only blank line) directly precedes the statement, matching the formatter's whitespace-only blank-line definition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5006-001")]
    public async Task VerifyDiagnosticForForeachStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5006
                                {
                                    public void Execute()
                                    {
                                        var values = new[] { 1, 2 };
                                        // Comment before foreach
                                        {|#0:foreach|} (var value in values)
                                        {
                                            Consume(value);
                                        }
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
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

        await Verify(testCode, fixedCode, Diagnostics(RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5006MessageFormat));
    }

    /// <summary>
    /// Verifies the spec example: an <c>await foreach</c> statement is reported on its <c>foreach</c> keyword, and one
    /// application of the fix puts the blank line in front of <c>await</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5006-002")]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsBlankLineAboveAwaitOfAwaitForeachStatement()
    {
        const string testCode = """
                                class C
                                {
                                    async System.Threading.Tasks.Task M(System.Collections.Generic.IAsyncEnumerable<int> items)
                                    {
                                        var a = 1;
                                        await {|RH5006:foreach|} (var item in items)
                                        {
                                            a += item;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     async System.Threading.Tasks.Task M(System.Collections.Generic.IAsyncEnumerable<int> items)
                                     {
                                         var a = 1;

                                         await foreach (var item in items)
                                         {
                                             a += item;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix puts the blank line in front of <c>await</c> of an <c>await foreach</c>
    /// statement under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5006-002")]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsBlankLineAboveAwaitOfAwaitForeachStatementWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    async System.Threading.Tasks.Task M(System.Collections.Generic.IAsyncEnumerable<int> items)
                                    {
                                        var a = 1;
                                        await {|RH5006:foreach|} (var item in items)
                                        {
                                            a += item;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     async System.Threading.Tasks.Task M(System.Collections.Generic.IAsyncEnumerable<int> items)
                                     {
                                         var a = 1;

                                         await foreach (var item in items)
                                         {
                                             a += item;
                                         }
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode),
                     NormalizeToCarriageReturnLineFeed(fixedCode),
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix moves an <c>await foreach</c> statement that shares its line with the
    /// statement before it to a new line, with the blank line in front of <c>await</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5006-002")]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixMovesAwaitForeachStatementSharingLineWithPreviousStatement()
    {
        const string testCode = """
                                class C
                                {
                                    async System.Threading.Tasks.Task M(System.Collections.Generic.IAsyncEnumerable<int> items)
                                    {
                                        var a = 1; await {|RH5006:foreach|} (var item in items) { a += item; }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     async System.Threading.Tasks.Task M(System.Collections.Generic.IAsyncEnumerable<int> items)
                                     {
                                         var a = 1;

                                         await foreach (var item in items) { a += item; }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix puts the blank line above the comment line that stands directly above an
    /// <c>await foreach</c> statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5006-002")]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsBlankLineAboveCommentLeadingAwaitForeachStatement()
    {
        const string testCode = """
                                class C
                                {
                                    async System.Threading.Tasks.Task M(System.Collections.Generic.IAsyncEnumerable<int> items)
                                    {
                                        var a = 1;
                                        // note
                                        await {|RH5006:foreach|} (var item in items)
                                        {
                                            a += item;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     async System.Threading.Tasks.Task M(System.Collections.Generic.IAsyncEnumerable<int> items)
                                     {
                                         var a = 1;

                                         // note
                                         await foreach (var item in items)
                                         {
                                             a += item;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one Fix All application corrects two adjacent <c>await foreach</c> statements, each with its blank line in
    /// front of <c>await</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5006-002")]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixAllInsertsBlankLinesAboveAwaitOfAdjacentAwaitForeachStatements()
    {
        const string testCode = """
                                class C
                                {
                                    async System.Threading.Tasks.Task M(System.Collections.Generic.IAsyncEnumerable<int> first, System.Collections.Generic.IAsyncEnumerable<int> second)
                                    {
                                        var a = 1;
                                        await {|RH5006:foreach|} (var item in first)
                                        {
                                        }
                                        await {|RH5006:foreach|} (var item in second)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     async System.Threading.Tasks.Task M(System.Collections.Generic.IAsyncEnumerable<int> first, System.Collections.Generic.IAsyncEnumerable<int> second)
                                     {
                                         var a = 1;

                                         await foreach (var item in first)
                                         {
                                         }

                                         await foreach (var item in second)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 2;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
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
                                        {|#1:foreach|} (var item in items)
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

                                         foreach (var item in items)
                                         {
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent foreach statements, each missing its preceding blank line, so the second fix's insertion
        // point sits directly against the first fix's closing brace
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5006MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}