using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzerTests : AnalyzerTestsBase<RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment between an if header and its block is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBetweenIfHeaderAndBlockIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (a)
                                            {|#0:// explanation|}
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzer.DiagnosticId, AnalyzerResources.RH5036MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between a foreach header and its embedded statement is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBetweenForeachHeaderAndStatementIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        foreach (var item in items)
                                            {|#0:// explanation|}
                                            Run(item);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzer.DiagnosticId, AnalyzerResources.RH5036MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment sharing the line with an if header is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySameLineCommentAfterIfHeaderIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (a) {|#0:// explanation|}
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzer.DiagnosticId, AnalyzerResources.RH5036MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between a while header and its body is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterWhileHeaderIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        while (a)
                                            {|#0:// explanation|}
                                        {
                                            break;
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzer.DiagnosticId, AnalyzerResources.RH5036MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between a for header and its body is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterForHeaderIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        for (var index = 0; index < 1; index++)
                                            {|#0:// explanation|}
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzer.DiagnosticId, AnalyzerResources.RH5036MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between a using header and its body is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterUsingHeaderIsReported()
    {
        const string testData = """
                                using System.IO;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        using (var stream = new MemoryStream())
                                            {|#0:// explanation|}
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzer.DiagnosticId, AnalyzerResources.RH5036MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between a lock header and its body is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterLockHeaderIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        lock (o)
                                            {|#0:// explanation|}
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzer.DiagnosticId, AnalyzerResources.RH5036MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between a fixed header and its body is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterFixedHeaderIsReported()
    {
        const string testData = """
                                internal unsafe class TestClass
                                {
                                    void Method(int[] items)
                                    {
                                        fixed (int* pointer = items)
                                            {|#0:// explanation|}
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject), Diagnostics(RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzer.DiagnosticId, AnalyzerResources.RH5036MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before an if statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeIfStatement()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        // explanation
                                        if (a)
                                        {
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment inside the body of an if statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentInsideBody()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        if (a)
                                        {
                                            // explanation
                                            Run();
                                        }
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment between else and its body is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBetweenElseAndBody()
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
    /// Verifies that a comment between do and its body is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBetweenDoAndBody()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        do
                                            // explanation
                                        {
                                        }
                                        while (a);
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
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}