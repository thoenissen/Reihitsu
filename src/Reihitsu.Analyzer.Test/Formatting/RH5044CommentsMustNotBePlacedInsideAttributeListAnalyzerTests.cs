using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5044CommentsMustNotBePlacedInsideAttributeListAnalyzer"/>
/// </summary>
[TestClass]
public class RH5044CommentsMustNotBePlacedInsideAttributeListAnalyzerTests : AnalyzerTestsBase<RH5044CommentsMustNotBePlacedInsideAttributeListAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment before the first attribute of an attribute list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5044-001")]
    [Requirement("SHARED-COMMENT-004")]
    [Requirement("SHARED-COMMENT-005")]
    public async Task VerifyCommentBeforeFirstAttributeIsReported()
    {
        const string testData = """
                                using System;

                                [
                                    {|#0:// serialization|}
                                    Serializable,
                                    Obsolete]
                                public class Data
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH5044CommentsMustNotBePlacedInsideAttributeListAnalyzer.DiagnosticId, AnalyzerResources.RH5044MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the closing bracket of an attribute list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5044-001")]
    [Requirement("SHARED-COMMENT-003")]
    [Requirement("SHARED-COMMENT-005")]
    public async Task VerifyCommentBeforeClosingBracketIsReported()
    {
        const string testData = """
                                using System;

                                [Serializable {|#0:/* serialization */|}]
                                public class Data
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH5044CommentsMustNotBePlacedInsideAttributeListAnalyzer.DiagnosticId, AnalyzerResources.RH5044MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside the arguments of an attribute is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5044-001")]
    public async Task VerifyCommentInAttributeArgumentsIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete(
                                        {|#0:// reason|}
                                        "Use V2")]
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5044CommentsMustNotBePlacedInsideAttributeListAnalyzer.DiagnosticId, AnalyzerResources.RH5044MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside an assembly attribute list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5044-001")]
    public async Task VerifyCommentInAssemblyAttributeListIsReported()
    {
        const string testData = """
                                using System;

                                [assembly: {|#0:/* compliance */|} CLSCompliant(false)]

                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH5044CommentsMustNotBePlacedInsideAttributeListAnalyzer.DiagnosticId, AnalyzerResources.RH5044MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before an attribute list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5044-001")]
    [Requirement("SHARED-COMMENT-004")]
    public async Task VerifyNoDiagnosticForCommentBeforeAttributeList()
    {
        const string testData = """
                                using System;

                                // serialization
                                [Serializable, Obsolete]
                                public class Data
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment at the start of the file before an attribute list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentAtStartOfFile()
    {
        const string testData = """
                                // header
                                [assembly: System.CLSCompliant(false)]

                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after an attribute list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5044-001")]
    [Requirement("SHARED-COMMENT-004")]
    public async Task VerifyNoDiagnosticForCommentAfterAttributeList()
    {
        const string testData = """
                                using System;

                                [Serializable]
                                // serialization
                                public class Data
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment in a module attribute list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5044-001")]
    public async Task VerifyCommentInModuleAttributeListIsReported()
    {
        const string testData = """
                                using System;

                                [module: {|#0:/* compliance */|} CLSCompliant(false)]

                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH5044CommentsMustNotBePlacedInsideAttributeListAnalyzer.DiagnosticId, AnalyzerResources.RH5044MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a binary operator in an attribute argument is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5044-001")]
    [Requirement("SHARED-COMMENT-008")]
    public async Task VerifyNoDiagnosticForCommentBeforeBinaryOperatorInAttributeArgument()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete("Use "
                                        // version
                                        + "V2")]
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before the first attribute of a list is reported with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5044-001")]
    public async Task VerifyCommentBeforeFirstAttributeIsReportedWithCrLfLineEndings()
    {
        const string testData = """
                                using System;

                                [
                                    {|#0:// serialization|}
                                    Serializable]
                                public class Data
                                {
                                }
                                """;

        await Verify(testData.ReplaceLineEndings("\r\n"), Diagnostics(RH5044CommentsMustNotBePlacedInsideAttributeListAnalyzer.DiagnosticId, AnalyzerResources.RH5044MessageFormat));
    }

    #endregion // Tests
}