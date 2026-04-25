using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0319FixedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer, RH0319FixedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifies diagnostics are reported when a fixed statement directly follows another statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForFixedStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0319
                                {
                                    private readonly byte[] data = new byte[1];

                                    public unsafe void Pin()
                                    {
                                        var buffer = new byte[4];
                                        {|#0:fixed|} (byte* pointer = buffer)
                                        {
                                            *pointer = 1;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0319
                                 {
                                     private readonly byte[] data = new byte[1];

                                     public unsafe void Pin()
                                     {
                                         var buffer = new byte[4];

                                         fixed (byte* pointer = buffer)
                                         {
                                             *pointer = 1;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0319MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a fixed statement already has a preceding blank line.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFixedStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0319
                                {
                                    private readonly byte[] data = new byte[1];

                                    public unsafe void Pin()
                                    {
                                        var buffer = new byte[4];

                                        fixed (byte* pointer = buffer)
                                        {
                                            *pointer = 1;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a fixed statement is the first statement in a block.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFixedStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH0319
                                {
                                    private readonly byte[] data = new byte[1];

                                    public unsafe void Pin()
                                    {
                                        fixed (byte* value = data)
                                        {
                                            *value = 42;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a fixed statement directly follows a comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFixedStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH0319
                                {
                                    public unsafe void Pin()
                                    {
                                        var buffer = new byte[4];
                                        // Comment before fixed
                                        fixed (byte* pointer = buffer)
                                        {
                                            *pointer = 1;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }
}