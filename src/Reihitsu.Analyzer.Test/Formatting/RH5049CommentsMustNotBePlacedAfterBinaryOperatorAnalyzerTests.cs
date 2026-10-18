using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer"/>
/// </summary>
[TestClass]
public class RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzerTests : AnalyzerTestsBase<RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment after a logical operator is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5049-001")]
    [Requirement("SHARED-COMMENT-007")]
    public async Task VerifyCommentAfterLogicalOperatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private static bool Method(bool a, bool b)
                                    {
                                        return a &&
                                            {|#0:// why b|}
                                            b;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5049MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment sharing the line with a binary operator is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5049-001")]
    [Requirement("SHARED-COMMENT-003")]
    public async Task VerifySameLineCommentAfterOperatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var sum = 1 + {|#0:// second|}
                                                  2;
                                        _ = sum;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5049MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after a null-coalescing operator is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5049-001")]
    [Requirement("SHARED-COMMENT-007")]
    public async Task VerifyCommentAfterCoalesceOperatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var value = o ??
                                            {|#0:// fallback|}
                                            items;
                                        _ = value;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5049MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a binary operator is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5049-001")]
    public async Task VerifyNoDiagnosticForCommentBeforeOperator()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private static bool Method(bool a, bool b)
                                    {
                                        return a

                                               // why b
                                               && b;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after a pattern combinator is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5049-001")]
    [Requirement("SHARED-COMMENT-007")]
    public async Task VerifyNoDiagnosticForCommentAfterPatternCombinator()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var isNumber = o is int or
                                            // wide
                                            long;
                                        _ = isNumber;
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
                                        var value = a && b;
                                        _ = value;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after the as of a conversion is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5049-001")]
    [Requirement("SHARED-COMMENT-007")]
    public async Task VerifyCommentAfterAsOperatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(object o)
                                    {
                                        var text = o as {|#0:/* text */|} string;
                                        _ = text;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5049MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the is of a type test is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5049-001")]
    [Requirement("SHARED-COMMENT-007")]
    public async Task VerifyCommentAfterTypeTestIsKeywordIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(object o)
                                    {
                                        var isText = o is
                                            {|#0:// text|}
                                            string;
                                        _ = isText;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5049MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the is of a pattern match is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5049-001")]
    [Requirement("SHARED-COMMENT-007")]
    public async Task VerifyNoDiagnosticForCommentAfterPatternIsKeyword()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(object o)
                                    {
                                        var isNull = o is
                                            // null check
                                            null;
                                        _ = isNull;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after the range operator is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-007")]
    public async Task VerifyNoDiagnosticForCommentAfterRangeOperator()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x)
                                    {
                                        var range = x..
                                            // end
                                            2;
                                        _ = range;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after a shift operator is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5049-001")]
    [Requirement("SHARED-COMMENT-007")]
    public async Task VerifyCommentAfterShiftOperatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x)
                                    {
                                        var shifted = x << {|#0:/* bits */|} 2;
                                        _ = shifted;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5049MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after a binary operator in a field initializer is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5049-001")]
    public async Task VerifyCommentAfterOperatorInFieldInitializerIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _value = 1 *
                                        {|#0:// factor|}
                                        2;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5049MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after a binary operator in an argument is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5049-001")]
    public async Task VerifyCommentAfterOperatorInArgumentIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x)
                                    {
                                        Run(x >
                                            {|#0:// limit|}
                                            1);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5049MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after a logical operator is reported with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5049-001")]
    public async Task VerifyCommentAfterLogicalOperatorIsReportedWithCrLfLineEndings()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private static bool Method(bool a, bool b)
                                    {
                                        return a &&
                                            {|#0:// why b|}
                                            b;
                                    }
                                }
                                """;

        await Verify(testData.ReplaceLineEndings("\r\n"), Diagnostics(RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5049MessageFormat));
    }

    #endregion // Tests
}