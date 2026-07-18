using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer, RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported and fixed when a break statement is immediately followed by another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForBreakStatementWithoutFollowingBlankLine()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            {|#0:break|};
                                            Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5011
                                 {
                                     public void Execute()
                                     {
                                         while (true)
                                         {
                                             break;

                                             Consume();
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a break statement is followed by a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBreakStatementWithFollowingBlankLine()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            break;

                                            Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a break statement is the last statement before a closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBreakStatementBeforeClosingBrace()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a diagnostic is reported when a comment line (rather than a whitespace-only blank line) directly
    /// follows the break statement, matching the formatter's whitespace-only blank-line definition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForBreakStatementWhenCommentLineDirectlyFollowsIt()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            {|#0:break|};
                                            // Comment after break
                                            Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5011
                                 {
                                     public void Execute()
                                     {
                                         while (true)
                                         {
                                             break;

                                             // Comment after break
                                             Consume();
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a break statement is immediately followed by an
    /// <c>#endregion</c> directive, mirroring the exemption the PrecededBy analyzer bases apply
    /// (issue #415)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBreakStatementFollowedByDirective()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            #region Guard
                                            break;
                                            #endregion
                                            Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}