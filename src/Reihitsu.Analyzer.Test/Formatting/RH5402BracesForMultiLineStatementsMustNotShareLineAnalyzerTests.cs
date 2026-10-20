using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer"/> and <see cref="RH5402BracesForMultiLineStatementsMustNotShareLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzerTests : BatchCodeFixTestsBase<RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer, RH5402BracesForMultiLineStatementsMustNotShareLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5402-001")]
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
    [Requirement("RH5402-001")]
    [Requirement("RH5402-002")]
    [Requirement("RH5402-003")]
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
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings.
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
    [Requirement("SHARED-BLOCK-001")]
    [Requirement("RH5402-001")]
    [Requirement("RH5402-002")]
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
    [Requirement("SHARED-BLOCK-001")]
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

    /// <summary>
    /// Verifies that a multi-line block whose opening brace is on the line of its statement is reported on the opening
    /// brace, while a brace on a line of its own and a block on one line are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5402-001")]
    [Requirement("RH5402-002")]
    public async Task VerifyOpeningBraceOnLineOfStatementIsReportedOnBrace()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a) {|RH5402:{|}
                                            M(a);
                                        } else {|RH5402:{|}
                                            M(a);
                                        }
                                        do {|RH5402:{|}
                                            M(a);
                                        } while (a);
                                        if (a)
                                        {
                                            M(a);
                                        }
                                        if (a) { }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the blocks of while, foreach with deconstruction, and lock statements are checked, while the
    /// blocks of a try statement and a lambda body are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLOCK-001")]
    [Requirement("RH5402-001")]
    public async Task VerifyOnlyBlocksOfControlFlowStatementsAreChecked()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int[] v)
                                    {
                                        while (a) {|RH5402:{|}
                                            M(a, v);
                                        }
                                        foreach (var (x, y) in new (int, int)[0]) {|RH5402:{|}
                                            M(a, v);
                                        }
                                        lock (v) {|RH5402:{|}
                                            M(a, v);
                                        }
                                        try {
                                            M(a, v);
                                        } finally {
                                            M(a, v);
                                        }
                                        System.Action f = () => {
                                            M(a, v);
                                        };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the blocks of for, foreach, using, and fixed statements are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLOCK-001")]
    [Requirement("RH5402-001")]
    public async Task VerifyRemainingControlFlowBlocksAreChecked()
    {
        const string testCode = """
                                class C
                                {
                                    unsafe void M(int[] v, System.IDisposable d)
                                    {
                                        for (var i = 0; i < 1; i++) {|RH5402:{|}
                                            M(v, d);
                                        }
                                        foreach (var x in v) {|RH5402:{|}
                                            M(v, d);
                                        }
                                        using (d) {|RH5402:{|}
                                            M(v, d);
                                        }
                                        fixed (int* p = v) {|RH5402:{|}
                                            M(v, d);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
    }

    /// <summary>
    /// Verifies that a nested block, checked, unchecked, and unsafe blocks, a block in a switch section, and the bodies
    /// of a local function and an anonymous method are not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLOCK-001")]
    public async Task VerifyBlocksOutsideControlFlowStatementsAreNotChecked()
    {
        const string testCode = """
                                class C
                                {
                                    unsafe void M(int a)
                                    {
                                        {
                                            M(a);
                                        }
                                        checked {
                                            M(a);
                                        }
                                        unchecked {
                                            M(a);
                                        }
                                        unsafe {
                                            M(a);
                                        }
                                        switch (a)
                                        {
                                            case 1: {
                                                M(a);
                                                break;
                                            }
                                        }
                                        void L() {
                                            M(a);
                                        }
                                        System.Action f = delegate {
                                            M(a);
                                        };
                                    }
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
    }

    /// <summary>
    /// Verifies that the fix moves the opening brace to its own line in the column of the line that introduces the
    /// block, and that a comment in front of the brace withholds the fix for that brace while its diagnostic stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5402-003")]
    [Requirement("SHARED-GAP-001")]
    public async Task VerifyFixMovesBraceToOwnLineUnlessCommentSitsInGap()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a) {|RH5402:{|}
                                            M(a);
                                        } else {|RH5402:{|}
                                            M(a);
                                        }
                                        if (a) /* note */ {|RH5402:{|}
                                            M(a);
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a)
                                     {
                                         if (a)
                                         {
                                             M(a);
                                         } else
                                         {
                                             M(a);
                                         }
                                         if (a) /* note */ {|RH5402:{|}
                                             M(a);
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.FixedState.MarkupHandling = MarkupMode.Allow);
    }

    /// <summary>
    /// Verifies that the fix moves the opening brace to its own line in the column of the line that introduces the
    /// block, and that a comment in front of the brace withholds the fix for that brace while its diagnostic stays,
    /// with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5402-003")]
    [Requirement("SHARED-GAP-001")]
    public async Task VerifyFixMovesBraceToOwnLineUnlessCommentSitsInGapWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a) {|RH5402:{|}
                                            M(a);
                                        } else {|RH5402:{|}
                                            M(a);
                                        }
                                        if (a) /* note */ {|RH5402:{|}
                                            M(a);
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a)
                                     {
                                         if (a)
                                         {
                                             M(a);
                                         } else
                                         {
                                             M(a);
                                         }
                                         if (a) /* note */ {|RH5402:{|}
                                             M(a);
                                         }
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode), test => test.FixedState.MarkupHandling = MarkupMode.Allow);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true) {|#0:{|} if (true) {|#1:{|}
                                        }
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         { if (true)
                                         {
                                         }
                                         }
                                     }
                                 }
                                 """;

        // The inner if statement is nested directly inside the outer if's block, and both braces originally share
        // the very same source line. Each fix only rewrites the narrow trivia gap immediately before its own
        // brace, so the two replacement spans sit directly next to each other on that shared line without
        // overlapping, exercising the batch fixer's handling of adjacent same-line replacements
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5402MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}