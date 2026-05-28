using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5017FixedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer, RH5017FixedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a fixed statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForFixedStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5017
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
                                 internal class RH5017
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

        await Verify(testCode, fixedCode, Diagnostics(RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5017MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a fixed statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFixedStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5017
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
    /// Verifies no diagnostics are reported when a fixed statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFixedStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5017
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
    /// Verifies no diagnostics are reported when a fixed statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFixedStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5017
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

    #endregion // Tests
}