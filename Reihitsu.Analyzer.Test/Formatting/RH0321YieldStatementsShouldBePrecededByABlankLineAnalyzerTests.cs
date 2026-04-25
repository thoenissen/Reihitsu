using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0321YieldStatementsShouldBePrecededByABlankLineCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer, RH0321YieldStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifies diagnostics are reported when a yield statement directly follows a non-yield statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForYieldStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH0321
                                {
                                    public IEnumerable<int> Execute()
                                    {
                                        var current = 1;
                                        {|#0:yield|} return current;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;

                                 internal class RH0321
                                 {
                                     public IEnumerable<int> Execute()
                                     {
                                         var current = 1;

                                         yield return current;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0321MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a yield statement already has a preceding blank line.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForYieldStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH0321
                                {
                                    public IEnumerable<int> Execute()
                                    {
                                        var current = 1;

                                        yield return current;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for consecutive yield statements.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForConsecutiveYieldStatements()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH0321
                                {
                                    public IEnumerable<int> Execute()
                                    {
                                        yield return 1;
                                        yield return 2;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a yield statement directly follows a comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForYieldStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH0321
                                {
                                    public IEnumerable<int> Execute()
                                    {
                                        var current = 1;
                                        // Comment before yield
                                        yield return current;
                                    }
                                }
                                """;

        await Verify(testCode);
    }
}