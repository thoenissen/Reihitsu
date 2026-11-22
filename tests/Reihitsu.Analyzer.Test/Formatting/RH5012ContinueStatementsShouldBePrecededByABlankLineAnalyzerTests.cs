using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5012ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer, RH5012ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a continue statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5012-001")]
    [Requirement("RH5012-002")]
    public async Task VerifyDiagnosticForContinueStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5012
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            var shouldContinue = true;
                                            {|#0:continue|};
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5012
                                 {
                                     public void Iterate()
                                     {
                                         while (true)
                                         {
                                             var shouldContinue = true;

                                             continue;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a continue statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5012-001")]
    public async Task VerifyNoDiagnosticForContinueStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5012
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            var shouldContinue = true;

                                            continue;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a continue statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5012-001")]
    public async Task VerifyNoDiagnosticForContinueStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5012
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            continue;
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
    [Requirement("RH5012-001")]
    public async Task VerifyDiagnosticForContinueStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5012
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            var shouldContinue = true;
                                            // Comment before continue
                                            {|#0:continue|};
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5012
                                 {
                                     public void Iterate()
                                     {
                                         while (true)
                                         {
                                             var shouldContinue = true;

                                             // Comment before continue
                                             continue;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a labeled <c>continue</c> that follows a statement is reported on the <c>continue</c> keyword and
    /// fixed with a blank line in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5012-001")]
    [Requirement("RH5012-002")]
    public async Task VerifyLabeledContinueFollowingAStatementIsReportedAndFixed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[] values, int a)
                                    {
                                        outer: foreach (var v in values)
                                        {
                                            foreach (var w in values)
                                            {
                                                a++;
                                                {|RH5012:continue|} outer;
                                            }
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[] values, int a)
                                     {
                                         outer: foreach (var v in values)
                                         {
                                             foreach (var w in values)
                                             {
                                                 a++;

                                                 continue outer;
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a labeled <c>continue</c> that is the first statement of its block, and one with a blank line in
    /// front of it, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5012-001")]
    public async Task VerifyLabeledContinueFirstInBlockOrAfterBlankLineIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[] values, int a)
                                    {
                                        outer: foreach (var v in values)
                                        {
                                            foreach (var w in values)
                                            {
                                                if (v == w)
                                                {
                                                    continue outer;
                                                }

                                                a++;

                                                continue outer;
                                            }
                                        }
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
                                #pragma warning disable CS0162

                                internal class RH5012
                                {
                                    public void Iterate()
                                    {
                                        while (true)
                                        {
                                            var shouldContinue = true;
                                            {|#0:continue|};
                                            {|#1:continue|};
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 #pragma warning disable CS0162

                                 internal class RH5012
                                 {
                                     public void Iterate()
                                     {
                                         while (true)
                                         {
                                             var shouldContinue = true;

                                             continue;

                                             continue;
                                         }
                                     }
                                 }
                                 """;

        // The second continue is unreachable code, kept alive only to exercise Fix All on two adjacent
        // occurrences in the same block; the pragma at file top suppresses CS0162 without touching either
        // occurrence
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}