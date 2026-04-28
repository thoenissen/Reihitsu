using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0317CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer, RH0317CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifies diagnostics are reported when a checked statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForCheckedStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0317
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;
                                        {|#0:checked|}
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0317
                                 {
                                     public int Execute(int value)
                                     {
                                         var factor = 2;

                                         checked
                                         {
                                             return value * factor;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0317MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a checked statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCheckedStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0317
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;

                                        checked
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a checked statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCheckedStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH0317
                                {
                                    public int Execute(int value)
                                    {
                                        checked
                                        {
                                            return value + 1;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a checked statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCheckedStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH0317
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;
                                        // Comment before checked
                                        checked
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }
}