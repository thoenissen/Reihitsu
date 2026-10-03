using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5045CommentsMustNotBePlacedInsideConditionalExpressionAnalyzer"/>
/// </summary>
[TestClass]
public class RH5045CommentsMustNotBePlacedInsideConditionalExpressionAnalyzerTests : AnalyzerTestsBase<RH5045CommentsMustNotBePlacedInsideConditionalExpressionAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that comments before the question mark and the colon of a conditional expression are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentsBeforeConditionalOperatorsAreReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var text = a
                                            {|#0:// valid case|}
                                            ? "ok"
                                            {|#1:// fallback|}
                                            : "error";
                                        _ = text;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5045CommentsMustNotBePlacedInsideConditionalExpressionAnalyzer.DiagnosticId, AnalyzerResources.RH5045MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a comment after the question mark of a conditional expression is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterQuestionMarkIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var text = a ?
                                            {|#0:// valid case|}
                                            "ok" : "error";
                                        _ = text;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5045CommentsMustNotBePlacedInsideConditionalExpressionAnalyzer.DiagnosticId, AnalyzerResources.RH5045MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the colon of a conditional expression is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterColonIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var text = a ? "ok" : {|#0:/* fallback */|} "error";
                                        _ = text;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5045CommentsMustNotBePlacedInsideConditionalExpressionAnalyzer.DiagnosticId, AnalyzerResources.RH5045MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside an operand that is not next to an operator of the conditional expression is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentInsideOperand()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var value = a ? (1
                                            // offset
                                            + 2) : 3;
                                        _ = value;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a conditional access is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeConditionalAccess()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var text = o
                                            // text
                                            ?.ToString();
                                        _ = text;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after the colon of a named argument is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeNamedArgumentColon()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private static int Compute(int value) => value;

                                    private int Method() => Compute(value:
                                        // the value
                                        1);
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a conditional expression is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeConditionalExpression()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        // valid gives ok, otherwise error
                                        var text = a ? "ok" : "error";
                                        _ = text;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment at the start of the file is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentAtStartOfFile()
    {
        const string testData = """
                                // header
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var value = a ? 1 : 2;
                                        _ = value;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}