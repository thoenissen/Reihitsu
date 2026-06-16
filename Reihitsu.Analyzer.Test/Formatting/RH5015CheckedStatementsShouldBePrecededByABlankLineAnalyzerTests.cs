using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5015CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer, RH5015CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a checked statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForCheckedStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5015
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
                                 internal class RH5015
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

        await Verify(testCode, fixedCode, Diagnostics(RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5015MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a checked statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCheckedStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5015
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
                                internal class RH5015
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
    /// Verifies a diagnostic is reported when a comment line (rather than a whitespace-only blank line) directly precedes the statement, matching the formatter's whitespace-only blank-line definition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForCheckedStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5015
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;
                                        // Comment before checked
                                        {|#0:checked|}
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5015
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

        await Verify(testCode, fixedCode, Diagnostics(RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5015MessageFormat));
    }

    #endregion // Tests
}