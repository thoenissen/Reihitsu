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
    [Requirement("RH5034-001")]
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
    [Requirement("RH5034-001")]
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
    [Requirement("RH5034-001")]
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
    [Requirement("RH5034-001")]
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
    [Requirement("RH5034-001")]
    [Requirement("SHARED-COMMENT-001")]
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
    [Requirement("RH5034-001")]
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
    [Requirement("RH5034-001")]
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
    [Requirement("RH5034-002")]
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

    /// <summary>
    /// Verifies that a comment after a documentation comment and a region directive is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5034-001")]
    public async Task VerifyCommentAfterDocumentationAndRegionDirectiveIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Does work</summary>
                                #region Members
                                    {|#0:// note|}
                                    public void DoWork()
                                    {
                                    }
                                #endregion
                                }
                                """;

        await Verify(testData, Diagnostics(RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5034MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the first token of a documented declaration is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5034-001")]
    public async Task VerifyNoDiagnosticForCommentAfterFirstTokenOfDeclaration()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Does work</summary>
                                    public
                                    // note
                                    void DoWork()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment between a documentation comment and a delegate is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5034-002")]
    public async Task VerifyCommentBetweenDocumentationAndDelegateIsReported()
    {
        const string testData = """
                                /// <summary>Handler</summary>
                                {|#0:// note|}
                                public delegate void Handler();
                                """;

        await Verify(testData, Diagnostics(RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5034MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after a documentation comment in front of a namespace is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5034-002")]
    public async Task VerifyNoDiagnosticForCommentAfterDocumentationBeforeNamespace()
    {
        const string testData = """
                                /// <summary>Namespace</summary>
                                // note
                                namespace Sample
                                {
                                    internal class TestClass
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after a documentation comment in front of an accessor is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5034-002")]
    public async Task VerifyNoDiagnosticForCommentAfterDocumentationBeforeAccessor()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _value;

                                    /// <summary>Value</summary>
                                    public int Value
                                    {
                                        /// <summary>Gets</summary>
                                        // note
                                        get { return _value; }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after a documentation comment in front of a parameter is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5034-002")]
    public async Task VerifyNoDiagnosticForCommentAfterDocumentationBeforeParameter()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Does work</summary>
                                    /// <param name="item">Item</param>
                                    public void DoWork(
                                        /// <summary>Item</summary>
                                        // note
                                        int item)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after a documentation comment in front of a local function is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5034-002")]
    public async Task VerifyNoDiagnosticForCommentAfterDocumentationBeforeLocalFunction()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        /// <summary>Local</summary>
                                        // note
                                        void Local()
                                        {
                                        }

                                        Local();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after a block documentation comment is reported when documentation comments are not parsed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5034-001")]
    [Requirement("SHARED-COMMENT-001")]
    public async Task VerifyCommentAfterBlockDocumentationWithoutDocumentationParsingIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /** <summary>Does work</summary> */
                                    {|#0:// note|}
                                    public void DoWork()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject), Diagnostics(RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5034MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between a documentation comment and a method is reported with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5034-001")]
    public async Task VerifyCommentAfterDocumentationIsReportedWithCrLfLineEndings()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Does work</summary>
                                    {|#0:// note|}
                                    public void DoWork()
                                    {
                                    }
                                }
                                """;

        await Verify(testData.ReplaceLineEndings("\r\n"), Diagnostics(RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5034MessageFormat));
    }

    /// <summary>
    /// Verifies that a block documentation comment after a single-line documentation comment is not reported, because
    /// both comments document the method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5034-001")]
    [Requirement("SHARED-COMMENT-001")]
    public async Task VerifyNoDiagnosticForBlockDocumentationCommentAfterDocumentationComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Does work</summary>
                                    /** <remarks>Does work</remarks> */
                                    public void DoWork()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a block documentation comment after a single-line documentation comment is not reported when
    /// documentation comments are not parsed, because both comments document the method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5034-001")]
    [Requirement("SHARED-COMMENT-001")]
    public async Task VerifyNoDiagnosticForBlockDocumentationCommentAfterDocumentationCommentWithoutDocumentationParsing()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Does work</summary>
                                    /** <remarks>Does work</remarks> */
                                    public void DoWork()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}