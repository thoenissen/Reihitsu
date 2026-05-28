using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5019YieldStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer, RH5019YieldStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a yield statement directly follows a non-yield statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForYieldStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH5019
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

                                 internal class RH5019
                                 {
                                     public IEnumerable<int> Execute()
                                     {
                                         var current = 1;

                                         yield return current;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5019MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a yield statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForYieldStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH5019
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
    /// Verifies no diagnostics are reported for consecutive yield statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForConsecutiveYieldStatements()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH5019
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
    /// Verifies no diagnostics are reported when a yield statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForYieldStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH5019
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

    #endregion // Tests
}