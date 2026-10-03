using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5037CommentsMustNotBePlacedBeforeContinuationClauseAnalyzer"/>
/// </summary>
[TestClass]
public class RH5037CommentsMustNotBePlacedBeforeContinuationClauseAnalyzerTests : AnalyzerTestsBase<RH5037CommentsMustNotBePlacedBeforeContinuationClauseAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment before else is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeElseIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (a)
                                        {
                                        }
                                        {|#0:// otherwise|}
                                        else
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5037CommentsMustNotBePlacedBeforeContinuationClauseAnalyzer.DiagnosticId, AnalyzerResources.RH5037MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before catch is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeCatchIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        try
                                        {
                                        }
                                        {|#0:// on failure|}
                                        catch (Exception)
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5037CommentsMustNotBePlacedBeforeContinuationClauseAnalyzer.DiagnosticId, AnalyzerResources.RH5037MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before finally is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeFinallyIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        try
                                        {
                                        }
                                        {|#0:// cleanup|}
                                        finally
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5037CommentsMustNotBePlacedBeforeContinuationClauseAnalyzer.DiagnosticId, AnalyzerResources.RH5037MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the while of a do loop is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeDoWhileIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        do
                                        {
                                        }
                                        {|#0:// condition|}
                                        while (a);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5037CommentsMustNotBePlacedBeforeContinuationClauseAnalyzer.DiagnosticId, AnalyzerResources.RH5037MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment sharing the line with the closing brace before else is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySameLineCommentBeforeElseIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (a)
                                        {
                                        } {|#0:// otherwise|}
                                        else
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5037CommentsMustNotBePlacedBeforeContinuationClauseAnalyzer.DiagnosticId, AnalyzerResources.RH5037MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside the else block is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentInsideElseBlock()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (a)
                                        {
                                        }
                                        else
                                        {
                                            // otherwise
                                            Run();
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a statement following an if statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeFollowingStatement()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (a)
                                        {
                                        }

                                        // next
                                        Run();
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
                                        if (a)
                                        {
                                        }
                                        else
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}