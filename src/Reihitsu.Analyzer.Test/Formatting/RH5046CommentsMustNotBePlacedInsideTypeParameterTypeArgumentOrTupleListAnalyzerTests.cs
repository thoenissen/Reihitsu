using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5046CommentsMustNotBePlacedInsideTypeParameterTypeArgumentOrTupleListAnalyzer"/>
/// </summary>
[TestClass]
public class RH5046CommentsMustNotBePlacedInsideTypeParameterTypeArgumentOrTupleListAnalyzerTests : AnalyzerTestsBase<RH5046CommentsMustNotBePlacedInsideTypeParameterTypeArgumentOrTupleListAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment inside a type argument list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInTypeArgumentListIsReported()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var map = new Dictionary<
                                            {|#0:// key|}
                                            string,
                                            int>();
                                        _ = map;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5046CommentsMustNotBePlacedInsideTypeParameterTypeArgumentOrTupleListAnalyzer.DiagnosticId, AnalyzerResources.RH5046MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a type parameter list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInTypeParameterListIsReported()
    {
        const string testData = """
                                public class Cache<
                                    {|#0:// key type|}
                                    TKey,
                                    TValue>
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH5046CommentsMustNotBePlacedInsideTypeParameterTypeArgumentOrTupleListAnalyzer.DiagnosticId, AnalyzerResources.RH5046MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a tuple type is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInTupleTypeIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public (int First, {|#0:/* second */|} int Second) Method() => (1, 2);
                                }
                                """;

        await Verify(testData, Diagnostics(RH5046CommentsMustNotBePlacedInsideTypeParameterTypeArgumentOrTupleListAnalyzer.DiagnosticId, AnalyzerResources.RH5046MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside the type argument list of a method call is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInMethodTypeArgumentListIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        _ = Array.Empty<{|#0:/* element */|} int>();
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5046CommentsMustNotBePlacedInsideTypeParameterTypeArgumentOrTupleListAnalyzer.DiagnosticId, AnalyzerResources.RH5046MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a tuple expression is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForTupleExpression()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var tuple = (
                                            // first
                                            1, 2);
                                        _ = tuple;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment between a type name and its type argument list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeTypeArgumentList()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        _ = Array.Empty
                                            // element
                                            <int>();
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a constraint clause is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeConstraintClause()
    {
        const string testData = """
                                public class Cache<TKey>
                                    // reference keys
                                    where TKey : class
                                {
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}