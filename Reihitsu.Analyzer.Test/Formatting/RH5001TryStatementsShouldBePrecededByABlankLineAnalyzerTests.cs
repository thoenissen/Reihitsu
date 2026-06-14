using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5001TryStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5001TryStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5001TryStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5001TryStatementsShouldBePrecededByABlankLineAnalyzer, RH5001TryStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a try statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForTryStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5001
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:try|}
                                        {
                                            value++;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5001
                                 {
                                     public void Execute()
                                     {
                                         var value = 1;

                                         try
                                         {
                                             value++;
                                         }
                                         catch
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5001TryStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5001MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a try statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForTryStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5001
                                {
                                    public void Execute()
                                    {
                                        var value = 1;

                                        try
                                        {
                                            value++;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a try statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForTryStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5001
                                {
                                    public void Execute()
                                    {
                                        try
                                        {
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a try statement directly follows a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForTryStatementWhenCommentDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5001
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        // Comment before try
                                        try
                                        {
                                            value++;
                                        }
                                        catch
                                        {
                                        }
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
        const string testCode = """
                                internal class RH5001
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        try
                                        {
                                            value++;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode));

        Assert.DoesNotContain("\n", fixedSource.Replace("\r\n", string.Empty));
    }

    #endregion // Tests
}