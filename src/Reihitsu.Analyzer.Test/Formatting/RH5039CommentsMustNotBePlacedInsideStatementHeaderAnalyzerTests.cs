using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer"/>
/// </summary>
[TestClass]
public class RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzerTests : AnalyzerTestsBase<RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment inside a for header is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyCommentInForHeaderIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        for (
                                            {|#0:// start|}
                                            var index = 0; index < 1; index++)
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between the if keyword and its condition is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-003")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyCommentBetweenIfKeywordAndConditionIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if {|#0:/* condition */|} (a)
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a while header is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-004")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyCommentInWhileHeaderIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        while (a
                                               {|#0:// condition|}
                                              )
                                        {
                                            break;
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a foreach header is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyCommentInForeachHeaderIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        foreach (var item {|#0:/* each */|} in items)
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a using header is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyCommentInUsingHeaderIsReported()
    {
        const string testData = """
                                using System.IO;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        using (
                                            {|#0:// resource|}
                                            var stream = new MemoryStream())
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a lock header is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyCommentInLockHeaderIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        lock ({|#0:/* gate */|} o)
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after a binary operator inside an if header is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-008")]
    public async Task VerifyCommentAfterBinaryOperatorInHeaderIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (a &&
                                            {|#0:// why b|}
                                            b)
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the is keyword of a pattern inside an if header is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-007")]
    public async Task VerifyCommentBeforePatternIsKeywordIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (o
                                            {|#0:// null check|}
                                            is null)
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a pattern combinator inside an if header is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-007")]
    public async Task VerifyCommentBeforePatternCombinatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (o is int
                                            {|#0:// or long|}
                                            or long)
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a binary operator inside an if header is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-007")]
    [Requirement("SHARED-COMMENT-008")]
    public async Task VerifyNoDiagnosticForCommentBeforeBinaryOperatorInHeader()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (a
                                            // why b
                                            && b)
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a type test inside an if header is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-007")]
    [Requirement("SHARED-COMMENT-008")]
    public async Task VerifyNoDiagnosticForCommentBeforeTypeTestInHeader()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (o
                                            // text check
                                            is string)
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment inside a switch header is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyNoDiagnosticForCommentInSwitchHeader()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        switch (o
                                                // value
                                               )
                                        {
                                            default:
                                                break;
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before an if statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-004")]
    public async Task VerifyNoDiagnosticForCommentBeforeIfStatement()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        // why b
                                        if (a && b)
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a statement inside a lambda block in an if header is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-006")]
    public async Task VerifyNoDiagnosticForStatementInLambdaBlockInHeader()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (((Func<bool>)(() =>
                                            {
                                                // explanation
                                                return a;
                                            }))())
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment inside the body of a for statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyNoDiagnosticForCommentInBody()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        for (var index = 0; index < 1; index++)
                                        {
                                            // explanation
                                            Run(index);
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment between await and foreach is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyCommentBetweenAwaitAndForeachIsReported()
    {
        const string testData = """
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                internal class TestClass
                                {
                                    async Task Method(IAsyncEnumerable<int> values)
                                    {
                                        await {|#0:/* each */|} foreach (var value in values)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between await and using is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyCommentBetweenAwaitAndUsingIsReported()
    {
        const string testData = """
                                using System.IO;
                                using System.Threading.Tasks;

                                internal class TestClass
                                {
                                    async Task Method()
                                    {
                                        await {|#0:/* dispose */|} using (var stream = new MemoryStream())
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a fixed header is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyCommentInFixedHeaderIsReported()
    {
        const string testData = """
                                internal unsafe class TestClass
                                {
                                    void Method(int[] items)
                                    {
                                        fixed ({|#0:/* pinned */|} int* pointer = items)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject), Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between the header and the body of an if statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-004")]
    public async Task VerifyNoDiagnosticForCommentBetweenHeaderAndBody()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (a)
                                            // explanation
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before the block of a lambda inside an if header is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-006")]
    public async Task VerifyNoDiagnosticForCommentBeforeLambdaBlockInHeader()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (((Func<bool>)(() =>
                                            // body
                                            {
                                                return a;
                                            }))())
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a condition that starts a switch expression is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-005")]
    public async Task VerifyCommentBeforeConditionStartingWithSwitchExpressionIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (
                                            {|#0:// classify|}
                                            o switch
                                            {
                                                _ => true
                                            })
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside the parentheses of a catch clause is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyNoDiagnosticForCommentInCatchDeclaration()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        try
                                        {
                                        }
                                        catch (/* any */ Exception)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment inside the condition of a do statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyNoDiagnosticForCommentInDoWhileCondition()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a)
                                    {
                                        do
                                        {
                                        }
                                        while (/* again */ a);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment in the header of a deconstructing foreach statement is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    [Requirement("SHARED-COMMENT-010")]
    public async Task VerifyCommentInDeconstructingForeachHeaderIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method((int, int)[] pairs)
                                    {
                                        foreach (var (first, {|#0:/* second */|} second) in pairs)
                                        {
                                            _ = first + second;
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment in an argument list inside a statement header is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-003")]
    public async Task VerifyCommentInArgumentListInsideHeaderIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (Run(
                                            {|#0:// value|}
                                            1) > 0)
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment in a for header is reported with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5039-001")]
    public async Task VerifyCommentInHeaderIsReportedWithCrLfLineEndings()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        for (
                                            {|#0:// start|}
                                            var index = 0; index < 1; index++)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData.ReplaceLineEndings("\r\n"), Diagnostics(RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer.DiagnosticId, AnalyzerResources.RH5039MessageFormat));
    }

    #endregion // Tests
}