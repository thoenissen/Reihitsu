using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer"/>
/// </summary>
[TestClass]
public class RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzerTests : AnalyzerTestsBase<RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment after the equals sign of a local initializer is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterInitializingEqualsIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var timeout =
                                            {|#0:// default|}
                                            30;
                                        _ = timeout;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5042MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before an assignment operator is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeAssignmentOperatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _value;

                                    private void Reset()
                                    {
                                        _value
                                            {|#0:// reset|}
                                            = 0;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5042MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after a compound assignment operator is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterCompoundAssignmentOperatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var total = 0;
                                        total += {|#0:/* increment */|} 1;
                                        _ = total;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5042MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between the equals sign and an array initializer is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeArrayInitializerIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int[] _values =
                                        {|#0:// defaults|}
                                        { 1, 2 };
                                }
                                """;

        await Verify(testData, Diagnostics(RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5042MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the equals sign of a parameter default is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterParameterDefaultEqualsIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public void Method(int value = {|#0:/* default */|} 1)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5042MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the equals sign of a property initializer is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterPropertyInitializerEqualsIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int Value { get; } =
                                        {|#0:// default|}
                                        1;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5042MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the equals sign of an anonymous object member is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForAnonymousObjectMemberEquals()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        _ = new { First =
                                            // value
                                            1 };
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after the equals sign of an attribute named argument is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForAttributeNamedArgumentEquals()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete("Use V2", DiagnosticId =
                                        // identifier
                                        "ID1")]
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before an assignment statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeAssignment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        // default
                                        var timeout = 30;
                                        _ = timeout;
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
                                        var timeout = 30;
                                        _ = timeout;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}