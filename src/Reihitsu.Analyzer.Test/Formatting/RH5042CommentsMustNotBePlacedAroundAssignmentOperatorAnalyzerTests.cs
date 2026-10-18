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
    [Requirement("RH5042-001")]
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
    [Requirement("RH5042-001")]
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
    [Requirement("RH5042-001")]
    [Requirement("SHARED-COMMENT-003")]
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
    [Requirement("RH5042-001")]
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
    [Requirement("RH5042-001")]
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
    [Requirement("RH5042-001")]
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
    [Requirement("RH5042-002")]
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
    [Requirement("RH5042-002")]
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
    [Requirement("RH5042-001")]
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

    /// <summary>
    /// Verifies that a comment after the equals sign of an event field initializer is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5042-001")]
    public async Task VerifyCommentAfterEventFieldInitializerEqualsIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    public event Action Changed =
                                        {|#0:// none|}
                                        null;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5042MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the equals sign of an enum member value is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5042-001")]
    public async Task VerifyCommentAfterEnumMemberEqualsIsReported()
    {
        const string testData = """
                                public enum Kind
                                {
                                    First = {|#0:/* start */|} 1,
                                }
                                """;

        await Verify(testData, Diagnostics(RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5042MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the equals sign of an object initializer member is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5042-001")]
    public async Task VerifyCommentAfterObjectInitializerMemberEqualsIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int A;

                                    void Method()
                                    {
                                        _ = new TestClass { A = {|#0:/* first */|} 1 };
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5042MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the equals sign of a with expression member is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5042-001")]
    public async Task VerifyCommentAfterWithExpressionMemberEqualsIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = (1, 2) with { Item1 = {|#0:/* first */|} 3 };
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5042MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the null-coalescing assignment operator is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5042-001")]
    public async Task VerifyCommentAfterCoalesceAssignmentOperatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(string text)
                                    {
                                        text ??=
                                            {|#0:// fallback|}
                                            "";
                                        _ = text;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5042MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the operator of a deconstructing assignment is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5042-001")]
    public async Task VerifyCommentBeforeDeconstructingAssignmentOperatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first, int second)
                                    {
                                        (first, second)
                                            {|#0:// swap|}
                                            = (second, first);
                                        _ = first + second;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5042MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the equals sign of a using alias is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5042-002")]
    public async Task VerifyNoDiagnosticForUsingAliasEquals()
    {
        const string testData = """
                                using Text =
                                    // alias
                                    System.String;

                                internal class TestClass
                                {
                                    private Text _value = "";
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after the equals sign of a local initializer is reported with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5042-001")]
    public async Task VerifyCommentAfterInitializingEqualsIsReportedWithCrLfLineEndings()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var timeout =
                                            {|#0:// default|}
                                            30;
                                        _ = timeout;
                                    }
                                }
                                """;

        await Verify(testData.ReplaceLineEndings("\r\n"), Diagnostics(RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer.DiagnosticId, AnalyzerResources.RH5042MessageFormat));
    }

    #endregion // Tests
}