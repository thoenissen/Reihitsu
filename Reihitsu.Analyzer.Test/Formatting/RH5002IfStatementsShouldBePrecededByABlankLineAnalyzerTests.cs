using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5002IfStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5002IfStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer, RH5002IfStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when an if statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForIfStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:if|} (value > 0)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5002
                                 {
                                     public void Execute()
                                     {
                                         var value = 1;

                                         if (value > 0)
                                         {
                                             value++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5002MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an if statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForIfStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute()
                                    {
                                        var value = 1;

                                        if (value > 0)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an if statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForIfStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute()
                                    {
                                        if (true)
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
    public async Task VerifyDiagnosticForIfStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute(bool isReady)
                                    {
                                        var value = 1;
                                        // Comment before if
                                        {|#0:if|} (isReady)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5002
                                 {
                                     public void Execute(bool isReady)
                                     {
                                         var value = 1;

                                         // Comment before if
                                         if (isReady)
                                         {
                                             value++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5002MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an if statement is the unbraced embedded body of a while statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForIfStatementAsEmbeddedWhileBody()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public int Execute(bool outer, bool inner)
                                    {
                                        while (outer)
                                            if (inner)
                                                return 1;

                                        return 0;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for an else-if statement, since its parent is an else clause rather than a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForElseIfStatement()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute(bool first, bool second)
                                    {
                                        if (first)
                                        {
                                            DoFirst();
                                        }
                                        else if (second)
                                        {
                                            DoSecond();
                                        }
                                    }

                                    private static void DoFirst()
                                    {
                                    }

                                    private static void DoSecond()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}