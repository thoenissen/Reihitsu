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
public class RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer, RH5019YieldStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a yield statement directly follows a non-yield statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5019-001")]
    [Requirement("RH5019-003")]
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
    /// Verifies diagnostics are reported when a yield break statement directly follows a non-yield statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5019-001")]
    [Requirement("RH5019-003")]
    public async Task VerifyDiagnosticForYieldBreakStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH5019
                                {
                                    public IEnumerable<int> Execute()
                                    {
                                        var current = 1;
                                        {|#0:yield|} break;
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

                                         yield break;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5019MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for consecutive yield statements inside a switch section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5019-002")]
    public async Task VerifyNoDiagnosticForConsecutiveYieldStatementsInSwitchSection()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH5019
                                {
                                    public IEnumerable<int> Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 0:
                                                yield return 1;
                                                yield return 2;
                                                break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a yield statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5019-001")]
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
    [Requirement("RH5019-002")]
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
    /// Verifies a diagnostic is reported when a comment line (rather than a whitespace-only blank line) directly precedes the statement, matching the formatter's whitespace-only blank-line definition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5019-001")]
    public async Task VerifyDiagnosticForYieldStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH5019
                                {
                                    public IEnumerable<int> Execute()
                                    {
                                        var current = 1;
                                        // Comment before yield
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

                                         // Comment before yield
                                         yield return current;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5019MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a yield statement is the unbraced embedded body of a foreach statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-002")]
    public async Task VerifyNoDiagnosticForYieldStatementAsEmbeddedForeachBody()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH5019
                                {
                                    public IEnumerable<int> Execute(IEnumerable<int> values)
                                    {
                                        foreach (var value in values)
                                            yield return value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a yield statement that directly follows a yield statement is exempt while a yield statement that follows another statement is reported, even when a yield statement appears earlier in the same list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5019-001")]
    [Requirement("RH5019-002")]
    [Requirement("RH5019-003")]
    public async Task VerifyDiagnosticForYieldStatementAfterStatementEvenWhenAnEarlierYieldStatementExists()
    {
        const string testCode = """
                                class C
                                {
                                    System.Collections.Generic.IEnumerable<int> M(int a)
                                    {
                                        a++;

                                        yield return 1;
                                        yield return 2;
                                        a++;
                                        {|#0:yield|} return 3;
                                        yield break;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     System.Collections.Generic.IEnumerable<int> M(int a)
                                     {
                                         a++;

                                         yield return 1;
                                         yield return 2;
                                         a++;

                                         yield return 3;
                                         yield break;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5019MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a yield break statement that directly follows a yield return statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5019-002")]
    public async Task VerifyNoDiagnosticForYieldBreakStatementAfterYieldReturnStatement()
    {
        const string testCode = """
                                class C
                                {
                                    System.Collections.Generic.IEnumerable<int> M(int a)
                                    {
                                        a++;

                                        yield return a;
                                        yield break;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class RH5019
                                {
                                    public IEnumerable<int> Execute(bool flag)
                                    {
                                        var current = 1;
                                        {|#0:yield|} return current;
                                        if (flag)
                                        {
                                            var next = 2;
                                            {|#1:yield|} return next;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;

                                 internal class RH5019
                                 {
                                     public IEnumerable<int> Execute(bool flag)
                                     {
                                         var current = 1;

                                         yield return current;
                                         if (flag)
                                         {
                                             var next = 2;

                                             yield return next;
                                         }
                                     }
                                 }
                                 """;

        // A top-level occurrence and a nested occurrence inside an if block; consecutive yield statements are
        // exempt from this rule, so two occurrences require two distinct non-yield predecessors
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5019MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}