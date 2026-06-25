using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5020SingleLineCommentsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5020SingleLineCommentsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5020SingleLineCommentsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5020SingleLineCommentsShouldBePrecededByABlankLineAnalyzer, RH5020SingleLineCommentsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a single-line comment directly follows a statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForCommentWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5020
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
                                 internal class RH5020
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

        await Verify(testCode, fixedCode, Diagnostics(RH5020SingleLineCommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported only for the first comment in a comment block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForCommentBlockWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    public void Execute()
                                    {
                                        var value = 0;
                                        {|#0:// Explain the value|}
                                        // Continue the explanation
                                        Consume(value);
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5020
                                 {
                                     public void Execute()
                                     {
                                         var value = 0;

                                         // Explain the value
                                         // Continue the explanation
                                         Consume(value);
                                     }

                                     private void Consume(int value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5020SingleLineCommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a single-line comment already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5020
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
    /// Verifies no diagnostics are reported for the first comment in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFirstCommentInBlock()
    {
        const string testCode = """
                                internal class RH5020
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
    /// Verifies no diagnostics are reported for the first comment in a file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFirstCommentInFile()
    {
        const string testCode = """
                                // File header comment
                                internal class RH5020
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a comment starts a switch section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFirstCommentInSwitchSection()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 0:
                                                // First comment in switch section
                                                Consume(value);
                                                break;
                                        }
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a comment directly follows another comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForAdjacentCommentLines()
    {
        const string testCode = """
                                internal class RH5020
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

    /// <summary>
    /// Verifies that the inserted blank line matches the document's detected CRLF end-of-line sequence instead of
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings (issue #257)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInsertedBlankLineUsesDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                internal class RH5020
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

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.DoesNotContain("\n", fixedSource.Replace("\r\n", string.Empty));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when the line immediately preceding the comment is a preprocessor
    /// directive, which acts as a transparent boundary rather than ordinary preceding content (issue #350)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentDirectlyAfterPreprocessorDirective()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    public void Execute()
                                    {
                                        var value = 0;
                                #if true
                                        value++;
                                #endif
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

    #endregion // Tests
}