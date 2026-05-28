using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5018LockStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5018LockStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5018LockStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5018LockStatementsShouldBePrecededByABlankLineAnalyzer, RH5018LockStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a lock statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForLockStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5018
                                {
                                    private readonly object gate = new();

                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:lock|} (gate)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5018
                                 {
                                     private readonly object gate = new();

                                     public void Execute()
                                     {
                                         var value = 1;

                                         lock (gate)
                                         {
                                             value++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5018LockStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5018MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a lock statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForLockStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5018
                                {
                                    private readonly object gate = new();

                                    public void Execute()
                                    {
                                        var value = 1;

                                        lock (gate)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a lock statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForLockStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5018
                                {
                                    private readonly object gate = new();

                                    public void Execute()
                                    {
                                        lock (gate)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a lock statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForLockStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5018
                                {
                                    private readonly object gate = new();

                                    public void Execute()
                                    {
                                        var value = 1;
                                        // Comment before lock
                                        lock (gate)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}