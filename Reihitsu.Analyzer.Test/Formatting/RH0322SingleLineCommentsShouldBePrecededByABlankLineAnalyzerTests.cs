using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0322SingleLineCommentsShouldBePrecededByABlankLineCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzer, RH0322SingleLineCommentsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifies diagnostics are reported when a single-line comment directly follows a statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForCommentWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0322
                                {
                                    public void Execute()
                                    {
                                        var value = 0;
                                        {|#0:// Explain the value|}
                                        Consume(value);
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH0322
                                 {
                                     public void Execute()
                                     {
                                         var value = 0;

                                         // Explain the value
                                         Consume(value);
                                     }

                                     private void Consume(int value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0322MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a single-line comment already has a preceding blank line.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH0322
                                {
                                    public void Execute()
                                    {
                                        var value = 0;

                                        // Explain the value
                                        Consume(value);
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for the first comment in a block.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFirstCommentInBlock()
    {
        const string testCode = """
                                internal class RH0322
                                {
                                    public void Execute()
                                    {
                                        // First comment in block
                                        Consume(0);
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a comment directly follows another comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForAdjacentCommentLines()
    {
        const string testCode = """
                                internal class RH0322
                                {
                                    public void Execute()
                                    {
                                        // First comment
                                        // Follow-up comment
                                        Consume(0);
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }
}