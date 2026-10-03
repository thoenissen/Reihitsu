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

    #endregion // Tests
}