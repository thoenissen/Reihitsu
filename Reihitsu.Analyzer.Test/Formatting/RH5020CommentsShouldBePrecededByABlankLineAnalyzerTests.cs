using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5020CommentsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5020CommentsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5020CommentsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5020CommentsShouldBePrecededByABlankLineAnalyzer, RH5020CommentsShouldBePrecededByABlankLineCodeFixProvider>
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

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
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

    /// <summary>
    /// Verifies a diagnostic is still reported when the preceding source line only looks like a directive because
    /// it is the content of a verbatim string starting with <c>#</c>, confirming the directive detection is
    /// syntax-based rather than a plain text prefix check (issue #350)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenPrecedingLineIsStringContentStartingWithHash()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    public void Execute()
                                    {
                                        var text = @"
                                #value";
                                        {|#0:// Explain the text|}
                                        Consume(text);
                                    }

                                    private void Consume(string value)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5020
                                 {
                                     public void Execute()
                                     {
                                         var text = @"
                                 #value";

                                         // Explain the text
                                         Consume(text);
                                     }

                                     private void Consume(string value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies that four-slash ordinary comments are covered by RH5020
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFourSlashCommentIsDetectedAndFixed()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    void Execute()
                                    {
                                        var value = 0;
                                        {|#0://// Ordinary comment|}
                                        value++;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5020
                                 {
                                     void Execute()
                                     {
                                         var value = 0;

                                         //// Ordinary comment
                                         value++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies that own-line block comments are covered while inline block comments remain outside the rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOwnLineBlockCommentIsDetectedButInlineCommentIsExcluded()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    void Execute()
                                    {
                                        var value = 0; /* inline */
                                        {|#0:/* own line */|}
                                        value++;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5020
                                 {
                                     void Execute()
                                     {
                                         var value = 0; /* inline */

                                         /* own line */
                                         value++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies that a mixed consecutive ordinary-comment block is owned by its first comment only
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMixedConsecutiveCommentsProduceOneDiagnostic()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    void Execute()
                                    {
                                        var value = 0;
                                        {|#0://// first|}
                                        /* continuation */
                                        value++;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5020
                                 {
                                     void Execute()
                                     {
                                         var value = 0;

                                         //// first
                                         /* continuation */
                                         value++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    void Execute()
                                    {
                                        var first = 1;
                                        {|#0://// first|}
                                        Consume(first);
                                        var second = 2;
                                        {|#1:/* second */|}
                                        Consume(second);
                                    }

                                    void Consume(int value)
                                    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5020
                                 {
                                     void Execute()
                                     {
                                         var first = 1;

                                         //// first
                                         Consume(first);
                                         var second = 2;

                                         /* second */
                                         Consume(second);
                                     }

                                     void Consume(int value)
                                     {
                                     }
                                 }
                                 """;

        // Verifies that Fix All converges for multiple newly covered ordinary-comment kinds
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}