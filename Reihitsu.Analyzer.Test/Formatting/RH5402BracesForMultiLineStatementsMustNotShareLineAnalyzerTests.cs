using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer"/> and <see cref="RH5402BracesForMultiLineStatementsMustNotShareLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzerTests : AnalyzerTestsBase<RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer, RH5402BracesForMultiLineStatementsMustNotShareLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true) {|#0:{|}
                                            int value = 0;
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         {
                                             int value = 0;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5402MessageFormat));
    }

    /// <summary>
    /// Verifies that the inserted line break matches the document's detected CRLF end-of-line sequence instead of
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings (issue #257)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInsertedLineBreakUsesDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true) {
                                            int value = 0;
                                        }
                                    }
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.DoesNotContain("\n", fixedSource.Replace("\r\n", string.Empty));
    }

    /// <summary>
    /// Verifies that both foreach syntax shapes use the same multi-line brace policy
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOrdinaryAndVariableForeachBodiesAreDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method((int, int)[] values)
                                    {
                                        foreach (var value in values) {|#0:{|}
                                        }

                                        foreach (var (first, second) in values) {|#1:{|}
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5402MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that catch blocks remain outside the statement-parent policy
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCatchBlockIsExcluded()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        try
                                        {
                                        }
                                        catch {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}