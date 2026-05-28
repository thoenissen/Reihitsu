using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5016UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer, RH5016UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when an unchecked statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForUncheckedStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5016
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;
                                        {|#0:unchecked|}
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5016
                                 {
                                     public int Execute(int value)
                                     {
                                         var factor = 2;

                                         unchecked
                                         {
                                             return value * factor;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5016MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an unchecked statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUncheckedStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5016
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;

                                        unchecked
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an unchecked statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUncheckedStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5016
                                {
                                    public int Execute(int value)
                                    {
                                        unchecked
                                        {
                                            return value + 1;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an unchecked statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUncheckedStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5016
                                {
                                    public int Execute(int value)
                                    {
                                        var factor = 2;
                                        // Comment before unchecked
                                        unchecked
                                        {
                                            return value * factor;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}