using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer"/>
/// </summary>
[TestClass]
public class RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzerTests : AnalyzerTestsBase<RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment between a documentation comment and a method is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBetweenDocumentationAndMethodIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Does work</summary>
                                    {|#0:// TODO: rename|}
                                    public void DoWork()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5034MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between a documentation comment and the attributes of the declaration is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBetweenDocumentationAndAttributesIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    /// <summary>Does work</summary>
                                    {|#0:// TODO: rename|}
                                    [Obsolete]
                                    public void DoWork()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5034MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between a documentation comment and a class is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBetweenDocumentationAndClassIsReported()
    {
        const string testData = """
                                /// <summary>Data</summary>
                                {|#0:/* note */|}
                                public class Data
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5034MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between a documentation comment and an enum member is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBetweenDocumentationAndEnumMemberIsReported()
    {
        const string testData = """
                                public enum Kind
                                {
                                    /// <summary>First</summary>
                                    {|#0:// note|}
                                    First,
                                }
                                """;

        await Verify(testData, Diagnostics(RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5034MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after a documentation comment is reported when documentation comments are not parsed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentAfterDocumentationWithoutDocumentationParsingIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Does work</summary>
                                    {|#0:// TODO: rename|}
                                    public void DoWork()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject), Diagnostics(RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5034MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the documentation comment is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeDocumentation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    // TODO: rename
                                    /// <summary>Does work</summary>
                                    public void DoWork()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before an undocumented method is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentWithoutDocumentation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    // TODO: rename
                                    public void DoWork()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after a misplaced documentation comment before a statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentAfterDocumentationBeforeStatement()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        /// <summary>Value</summary>
                                        // note
                                        var value = 1;
                                        _ = value;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}