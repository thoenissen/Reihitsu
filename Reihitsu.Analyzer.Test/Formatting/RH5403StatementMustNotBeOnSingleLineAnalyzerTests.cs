using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5403StatementMustNotBeOnSingleLineAnalyzer"/> and <see cref="RH5403StatementMustNotBeOnSingleLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5403StatementMustNotBeOnSingleLineAnalyzerTests : AnalyzerTestsBase<RH5403StatementMustNotBeOnSingleLineAnalyzer, RH5403StatementMustNotBeOnSingleLineCodeFixProvider>
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
                                        if (true) {|#0:{|} return; }
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
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5403StatementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5403MessageFormat));
    }

    /// <summary>
    /// Verifies that the inserted line breaks match the document's detected CRLF end-of-line sequence instead of
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings (issue #257)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInsertedLineBreaksUseDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true) { return; }
                                    }
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.DoesNotContain("\n", fixedSource.Replace("\r\n", string.Empty));
    }

    /// <summary>
    /// Verifies that the complete shared statement-parent family reports populated single-line bodies
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAdditionalStatementParentsAreDetected()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    unsafe void Method((int, int)[] values, IDisposable disposable, int[] buffer)
                                    {
                                        foreach (var (first, second) in values) {|#0:{|} return; }
                                        using (disposable) {|#1:{|} return; }
                                        lock (this) {|#2:{|} return; }
                                        fixed (int* item = buffer) {|#3:{|} return; }
                                        do {|#4:{|} return; } while (false);
                                    }
                                }
                                """;

        await Verify(testData,
                     test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject),
                     Diagnostics(RH5403StatementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5403MessageFormat, 5));
    }

    /// <summary>
    /// Verifies that empty covered bodies and populated catch bodies remain outside RH5403
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyCoveredBodyAndCatchAreExcluded()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        do { } while (false);

                                        try
                                        {
                                        }
                                        catch { return; }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a single-line block nested in a lambda body is detected, so the rule reaches blocks that are
    /// not direct descendants of a member declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleLineBlockInsideLambdaBodyIsDetected()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Action action = () =>
                                                        {
                                                            if (true) {|#0:{|} return; }
                                                        };
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5403StatementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5403MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line block nested in a local function is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleLineBlockInsideLocalFunctionIsDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        void Local()
                                        {
                                            if (true) {|#0:{|} return; }
                                        }

                                        Local();
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5403StatementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5403MessageFormat));
    }

    #endregion // Tests
}