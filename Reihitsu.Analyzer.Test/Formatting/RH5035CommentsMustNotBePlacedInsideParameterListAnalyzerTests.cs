using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer"/>
/// </summary>
[TestClass]
public class RH5035CommentsMustNotBePlacedInsideParameterListAnalyzerTests : AnalyzerTestsBase<RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment before the first parameter is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeFirstParameterIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void Method(
                                        {|#0:// the value|}
                                        int value,
                                        int other)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer.DiagnosticId, AnalyzerResources.RH5035MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the closing parenthesis is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeClosingParenthesisIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void Method(int value
                                                       {|#0:// last|}
                                                      )
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer.DiagnosticId, AnalyzerResources.RH5035MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a comma between parameters is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeCommaIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void Method(int value {|#0:/* first */|}, int other)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer.DiagnosticId, AnalyzerResources.RH5035MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between the method name and its parameter list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeOpeningParenthesisIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void Method
                                        {|#0:// parameters|}
                                        (int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer.DiagnosticId, AnalyzerResources.RH5035MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a constructor parameter list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInConstructorParameterListIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public TestClass(
                                        {|#0:// the value|}
                                        int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer.DiagnosticId, AnalyzerResources.RH5035MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a delegate parameter list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInDelegateParameterListIsReported()
    {
        const string testData = """
                                public delegate void Handler(
                                    {|#0:// the value|}
                                    int value);
                                """;

        await Verify(testData, Diagnostics(RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer.DiagnosticId, AnalyzerResources.RH5035MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside an indexer parameter list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInIndexerParameterListIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int this[
                                        {|#0:// the index|}
                                        int index] => index;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer.DiagnosticId, AnalyzerResources.RH5035MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a local function parameter list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInLocalFunctionParameterListIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        int Local(
                                            {|#0:// the value|}
                                            int value)
                                        {
                                            return value;
                                        }

                                        _ = Local(1);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer.DiagnosticId, AnalyzerResources.RH5035MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a lambda parameter list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInLambdaParameterListIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Func<int, int> function = (
                                            {|#0:// the value|}
                                            int value) => value;
                                        _ = function;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer.DiagnosticId, AnalyzerResources.RH5035MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a primary constructor parameter list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInPrimaryConstructorParameterListIsReported()
    {
        const string testData = """
                                internal class TestClass(
                                    {|#0:// the value|}
                                    int value)
                                {
                                    public int Value => value;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer.DiagnosticId, AnalyzerResources.RH5035MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a function pointer parameter list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInFunctionPointerParameterListIsReported()
    {
        const string testData = """
                                internal unsafe class TestClass
                                {
                                    private delegate*<{|#0:/* value */|} int, void> _callback;
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject), Diagnostics(RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer.DiagnosticId, AnalyzerResources.RH5035MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after a binary operator in a default value is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterBinaryOperatorInDefaultValueIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void Method(int value = 1 |
                                                                   {|#0:// flag|}
                                                                   2)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer.DiagnosticId, AnalyzerResources.RH5035MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a binary operator in a default value is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeBinaryOperatorInDefaultValue()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void Method(int value = 1
                                                                   // flag
                                                                   | 2)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a lambda passed as an argument is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeLambdaArgument()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Run(
                                            // the projection
                                            (int value) => value);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment between the parameter list and the body is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBetweenParameterListAndBody()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void Method(int value)
                                        // body
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a parameter list without comments is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleLineParameterList()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    // value and other
                                    public void Method(int value, int other)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}