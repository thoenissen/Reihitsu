using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzer, RH0313BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies diagnostics are reported and fixed when a break statement is immediately followed by another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForBreakStatementWithoutFollowingBlankLine()
    {
        const string testCode = """
                                internal class RH0313
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
        var fixedCode = "internal class RH0313" + Environment.NewLine
                        + "{" + Environment.NewLine
                        + "    public void Execute()" + Environment.NewLine
                        + "    {" + Environment.NewLine
                        + "        while (true)" + Environment.NewLine
                        + "        {" + Environment.NewLine
                        + "            break;" + Environment.NewLine
                        + Environment.NewLine
                        + "            Consume();" + Environment.NewLine
                        + "        }" + Environment.NewLine
                        + "    }" + Environment.NewLine
                        + Environment.NewLine
                        + "    private void Consume()" + Environment.NewLine
                        + "    {" + Environment.NewLine
                        + "    }" + Environment.NewLine
                        + "}";

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0313MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a break statement is followed by a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBreakStatementWithFollowingBlankLine()
    {
        const string testCode = """
                                internal class RH0313
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
                                internal class RH0313
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

    #endregion // Members
}