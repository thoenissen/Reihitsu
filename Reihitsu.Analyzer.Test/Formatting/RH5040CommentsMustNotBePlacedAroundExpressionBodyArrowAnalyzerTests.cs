using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5040CommentsMustNotBePlacedAroundExpressionBodyArrowAnalyzer"/>
/// </summary>
[TestClass]
public class RH5040CommentsMustNotBePlacedAroundExpressionBodyArrowAnalyzerTests : AnalyzerTestsBase<RH5040CommentsMustNotBePlacedAroundExpressionBodyArrowAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment before the arrow of an expression-bodied property is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforePropertyArrowIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int Value
                                        {|#0:// computed|}
                                        => 42;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5040CommentsMustNotBePlacedAroundExpressionBodyArrowAnalyzer.DiagnosticId, AnalyzerResources.RH5040MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the arrow of an expression-bodied property is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterPropertyArrowIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int Other =>
                                        {|#0:// computed|}
                                        42;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5040CommentsMustNotBePlacedAroundExpressionBodyArrowAnalyzer.DiagnosticId, AnalyzerResources.RH5040MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the arrow of an expression-bodied method is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAroundMethodArrowIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int Method() => {|#0:/* computed */|} 42;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5040CommentsMustNotBePlacedAroundExpressionBodyArrowAnalyzer.DiagnosticId, AnalyzerResources.RH5040MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the arrow of an expression-bodied accessor is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAroundAccessorArrowIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _value;

                                    public int Value
                                    {
                                        get
                                            {|#0:// stored|}
                                            => _value;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5040CommentsMustNotBePlacedAroundExpressionBodyArrowAnalyzer.DiagnosticId, AnalyzerResources.RH5040MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before an expression-bodied member is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeExpressionBodiedMember()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    // computed
                                    public int Value => 42;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after the arrow of an expression-bodied local function is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForLocalFunctionArrow()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        int Local() =>
                                            // computed
                                            42;

                                        _ = Local();
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after the arrow of a lambda is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForLambdaArrow()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        Func<int> function = () =>
                                            // computed
                                            42;
                                        _ = function;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after the arrow of a switch expression arm is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSwitchArmArrow()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        _ = o switch
                                        {
                                            string =>
                                                // text
                                                1,
                                            _ => 2
                                        };
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
                                    public int Value => 42;
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}