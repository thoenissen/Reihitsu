using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5005UsingStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer, RH5005UsingStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a using statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5005-001")]
    [Requirement("RH5005-002")]
    public async Task VerifyDiagnosticForUsingStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH5005
                                {
                                    public void Execute()
                                    {
                                        var fileName = "test.txt";
                                        {|#0:using|} (var stream = File.OpenRead(fileName))
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.IO;

                                 internal class RH5005
                                 {
                                     public void Execute()
                                     {
                                         var fileName = "test.txt";

                                         using (var stream = File.OpenRead(fileName))
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5005MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a using statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5005-001")]
    public async Task VerifyNoDiagnosticForUsingStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH5005
                                {
                                    public void Execute()
                                    {
                                        var fileName = "test.txt";

                                        using (var stream = File.OpenRead(fileName))
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a using statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5005-001")]
    public async Task VerifyNoDiagnosticForUsingStatementAtStartOfBlock()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH5005
                                {
                                    public void Execute()
                                    {
                                        using (var stream = File.OpenRead("test.txt"))
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
    [Requirement("RH5005-001")]
    public async Task VerifyDiagnosticForUsingStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH5005
                                {
                                    public void Execute()
                                    {
                                        var fileName = "test.txt";
                                        // Comment before using
                                        {|#0:using|} (var stream = File.OpenRead(fileName))
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.IO;

                                 internal class RH5005
                                 {
                                     public void Execute()
                                     {
                                         var fileName = "test.txt";

                                         // Comment before using
                                         using (var stream = File.OpenRead(fileName))
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5005MessageFormat));
    }

    /// <summary>
    /// Verifies a using statement after a statement is reported on its using keyword while a using declaration after a statement is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5005-001")]
    [Requirement("RH5005-002")]
    public async Task VerifyDiagnosticForUsingStatementButNotForUsingDeclaration()
    {
        const string testCode = """
                                class C
                                {
                                    void M(System.IDisposable resource)
                                    {
                                        var a = 1;
                                        {|#0:using|} (resource)
                                        {
                                            a++;
                                        }

                                        a++;
                                        using var other = resource;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(System.IDisposable resource)
                                     {
                                         var a = 1;

                                         using (resource)
                                         {
                                             a++;
                                         }

                                         a++;
                                         using var other = resource;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5005MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a using declaration that directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5005-001")]
    public async Task VerifyNoDiagnosticForUsingDeclarationAfterStatement()
    {
        const string testCode = """
                                class C
                                {
                                    void M(System.IDisposable resource)
                                    {
                                        var a = 1;
                                        a++;
                                        using var other = resource;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the spec example: an <c>await using</c> statement is reported on its <c>using</c> keyword, and one
    /// application of the fix puts the blank line in front of <c>await</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5005-002")]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsBlankLineAboveAwaitOfAwaitUsingStatement()
    {
        const string testCode = """
                                class C
                                {
                                    async System.Threading.Tasks.Task M(System.IAsyncDisposable resource)
                                    {
                                        var a = 1;
                                        await {|RH5005:using|} (resource)
                                        {
                                            a++;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     async System.Threading.Tasks.Task M(System.IAsyncDisposable resource)
                                     {
                                         var a = 1;

                                         await using (resource)
                                         {
                                             a++;
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
    /// Verifies one application of the fix puts the blank line in front of <c>await</c> of an <c>await using</c>
    /// statement under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5005-002")]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsBlankLineAboveAwaitOfAwaitUsingStatementWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    async System.Threading.Tasks.Task M(System.IAsyncDisposable resource)
                                    {
                                        var a = 1;
                                        await {|RH5005:using|} (resource)
                                        {
                                            a++;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     async System.Threading.Tasks.Task M(System.IAsyncDisposable resource)
                                     {
                                         var a = 1;

                                         await using (resource)
                                         {
                                             a++;
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
    /// Verifies one application of the fix moves an <c>await using</c> statement that shares its line with the
    /// statement before it to a new line, with the blank line in front of <c>await</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5005-002")]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixMovesAwaitUsingStatementSharingLineWithPreviousStatement()
    {
        const string testCode = """
                                class C
                                {
                                    async System.Threading.Tasks.Task M(System.IAsyncDisposable resource)
                                    {
                                        var a = 1; await {|RH5005:using|} (resource) { a++; }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     async System.Threading.Tasks.Task M(System.IAsyncDisposable resource)
                                     {
                                         var a = 1;

                                         await using (resource) { a++; }
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
    /// <c>await using</c> statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5005-002")]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsBlankLineAboveCommentLeadingAwaitUsingStatement()
    {
        const string testCode = """
                                class C
                                {
                                    async System.Threading.Tasks.Task M(System.IAsyncDisposable resource)
                                    {
                                        var a = 1;
                                        // note
                                        await {|RH5005:using|} (resource)
                                        {
                                            a++;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     async System.Threading.Tasks.Task M(System.IAsyncDisposable resource)
                                     {
                                         var a = 1;

                                         // note
                                         await using (resource)
                                         {
                                             a++;
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
    /// Verifies one Fix All application corrects two adjacent <c>await using</c> statements, each with its blank line in
    /// front of <c>await</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5005-002")]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixAllInsertsBlankLinesAboveAwaitOfAdjacentAwaitUsingStatements()
    {
        const string testCode = """
                                class C
                                {
                                    async System.Threading.Tasks.Task M(System.IAsyncDisposable first, System.IAsyncDisposable second)
                                    {
                                        var a = 1;
                                        await {|RH5005:using|} (first)
                                        {
                                        }
                                        await {|RH5005:using|} (second)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     async System.Threading.Tasks.Task M(System.IAsyncDisposable first, System.IAsyncDisposable second)
                                     {
                                         var a = 1;

                                         await using (first)
                                         {
                                         }

                                         await using (second)
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
                                using System.IO;

                                internal class RH5005
                                {
                                    public void Execute()
                                    {
                                        var fileName = "test.txt";
                                        {|#0:using|} (var first = File.OpenRead(fileName))
                                        {
                                        }
                                        {|#1:using|} (var second = File.OpenRead(fileName))
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.IO;

                                 internal class RH5005
                                 {
                                     public void Execute()
                                     {
                                         var fileName = "test.txt";

                                         using (var first = File.OpenRead(fileName))
                                         {
                                         }

                                         using (var second = File.OpenRead(fileName))
                                         {
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent using statements, each missing its preceding blank line, so the second fix's insertion
        // point sits directly against the first fix's closing brace
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5005MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}