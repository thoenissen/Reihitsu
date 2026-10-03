using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5041CommentsMustNotBePlacedInsideDeclaratorListAnalyzer"/>
/// </summary>
[TestClass]
public class RH5041CommentsMustNotBePlacedInsideDeclaratorListAnalyzerTests : AnalyzerTestsBase<RH5041CommentsMustNotBePlacedInsideDeclaratorListAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment after the comma between declarators is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterDeclaratorSeparatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _first,
                                        {|#0:// second|}
                                        _second;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5041CommentsMustNotBePlacedInsideDeclaratorListAnalyzer.DiagnosticId, AnalyzerResources.RH5041MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the comma between declarators is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeDeclaratorSeparatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _first {|#0:/* first */|}, _second;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5041CommentsMustNotBePlacedInsideDeclaratorListAnalyzer.DiagnosticId, AnalyzerResources.RH5041MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the semicolon of a field is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeFieldTerminatorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _value
                                        {|#0:// terminator|}
                                        ;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5041CommentsMustNotBePlacedInsideDeclaratorListAnalyzer.DiagnosticId, AnalyzerResources.RH5041MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between local declarators is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInLocalDeclaratorListIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        int first = 1,
                                            {|#0:// second|}
                                            second = 2;
                                        Run(first, second);
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5041CommentsMustNotBePlacedInsideDeclaratorListAnalyzer.DiagnosticId, AnalyzerResources.RH5041MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the semicolon of an event field is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeEventFieldTerminatorIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    public event Action Changed
                                        {|#0:// terminator|}
                                        ;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5041CommentsMustNotBePlacedInsideDeclaratorListAnalyzer.DiagnosticId, AnalyzerResources.RH5041MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between the type and the first declarator is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBetweenTypeAndFirstDeclarator()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int
                                        // value
                                        _value;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a field is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeField()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    // first and second
                                    private int _first, _second;
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
                                    private int _first, _second;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after the comma between enum members is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentAfterEnumMemberComma()
    {
        const string testData = """
                                public enum Kind
                                {
                                    First,

                                    // second
                                    Second
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before the semicolon of a return statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeReturnTerminator()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private static int Method()
                                    {
                                        return 1
                                            // terminator
                                            ;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}