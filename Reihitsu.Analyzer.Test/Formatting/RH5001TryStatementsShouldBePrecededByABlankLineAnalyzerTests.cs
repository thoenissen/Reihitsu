using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5001TryStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5001TryStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5001TryStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5001TryStatementsShouldBePrecededByABlankLineAnalyzer, RH5001TryStatementsShouldBePrecededByABlankLineCodeFixProvider>
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
    /// Verifies no diagnostics are reported for top-level try statements because the formatter does not apply
    /// statement-spacing rules to compilation-unit statement lists
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForTopLevelTryStatement()
    {
        const string testCode = """
                                System.Console.WriteLine("Before");
                                try
                                {
                                }
                                catch
                                {
                                }
                                """;

        await Verify(testCode, test => test.TestState.OutputKind = OutputKind.ConsoleApplication);
    }

    /// <summary>
    /// Verifies a diagnostic is reported when a comment line (rather than a whitespace-only blank line) directly precedes the statement, matching the formatter's whitespace-only blank-line definition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForTryStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5001
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        // Comment before try
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

        await Verify(testCode, fixedCode, Diagnostics(RH5001TryStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5001MessageFormat));
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

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5001
                                {
                                    public void Execute()
                                    {
                                        var value = 1; {|#0:try|}
                                        {
                                            value++;
                                        }
                                        catch
                                        {
                                        }
                                        {|#1:try|}
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

        // The first occurrence shares its line with the previous statement, exercising the wide gap-replacement
        // branch, while the second sits on its own line directly after the first catch block, exercising the
        // narrow leading-trivia branch
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5001TryStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5001MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}