using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0307UsingStatementsShouldBePrecededByABlankLineCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer, RH0307UsingStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifies diagnostics are reported when a using statement directly follows another statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForUsingStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH0307
                                {
                                    public void Execute()
                                    {
                                        var fileName = "test.txt";
                                        {|#0:using|} (var stream = File.OpenRead(fileName))
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.IO;

                                 internal class RH0307
                                 {
                                     public void Execute()
                                     {
                                         var fileName = "test.txt";

                                         using (var stream = File.OpenRead(fileName))
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0307MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a using statement already has a preceding blank line.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUsingStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH0307
                                {
                                    public void Execute()
                                    {
                                        var fileName = "test.txt";

                                        using (var stream = File.OpenRead(fileName))
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a using statement is the first statement in a block.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUsingStatementAtStartOfBlock()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH0307
                                {
                                    public void Execute()
                                    {
                                        using (var stream = File.OpenRead("test.txt"))
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a using statement directly follows a comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUsingStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH0307
                                {
                                    public void Execute()
                                    {
                                        var fileName = "test.txt";
                                        // Comment before using
                                        using (var stream = File.OpenRead(fileName))
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }
}