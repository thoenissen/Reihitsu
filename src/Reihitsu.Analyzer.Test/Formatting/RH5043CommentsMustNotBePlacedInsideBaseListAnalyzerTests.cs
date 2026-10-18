using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5043CommentsMustNotBePlacedInsideBaseListAnalyzer"/>
/// </summary>
[TestClass]
public class RH5043CommentsMustNotBePlacedInsideBaseListAnalyzerTests : AnalyzerTestsBase<RH5043CommentsMustNotBePlacedInsideBaseListAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment after the colon of a base list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5043-001")]
    [Requirement("SHARED-COMMENT-005")]
    public async Task VerifyCommentAfterBaseListColonIsReported()
    {
        const string testData = """
                                public class Repository :
                                    {|#0:// storage|}
                                    Base,
                                    IRepository
                                {
                                }

                                public class Base
                                {
                                }

                                public interface IRepository
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH5043CommentsMustNotBePlacedInsideBaseListAnalyzer.DiagnosticId, AnalyzerResources.RH5043MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between two base types is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5043-001")]
    [Requirement("SHARED-COMMENT-005")]
    public async Task VerifyCommentBetweenBaseTypesIsReported()
    {
        const string testData = """
                                public class Repository : Base,
                                    {|#0:// contract|}
                                    IRepository
                                {
                                }

                                public class Base
                                {
                                }

                                public interface IRepository
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH5043CommentsMustNotBePlacedInsideBaseListAnalyzer.DiagnosticId, AnalyzerResources.RH5043MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside the base list of an enum is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5043-001")]
    [Requirement("SHARED-COMMENT-003")]
    public async Task VerifyCommentInEnumBaseIsReported()
    {
        const string testData = """
                                public enum Kind : {|#0:/* storage */|} byte
                                {
                                    First,
                                }
                                """;

        await Verify(testData, Diagnostics(RH5043CommentsMustNotBePlacedInsideBaseListAnalyzer.DiagnosticId, AnalyzerResources.RH5043MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the colon of a base list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5043-001")]
    [Requirement("SHARED-COMMENT-004")]
    public async Task VerifyNoDiagnosticForCommentBeforeBaseListColon()
    {
        const string testData = """
                                public class Repository
                                    // storage
                                    : Base
                                {
                                }

                                public class Base
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment between the base list and the body is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5043-001")]
    [Requirement("SHARED-COMMENT-004")]
    public async Task VerifyNoDiagnosticForCommentAfterBaseList()
    {
        const string testData = """
                                public class Repository : Base
                                    // body
                                {
                                }

                                public class Base
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before a constraint clause is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5043-001")]
    [Requirement("SHARED-COMMENT-004")]
    public async Task VerifyNoDiagnosticForCommentBeforeConstraintClause()
    {
        const string testData = """
                                public class Repository<T> : Base
                                    // reference types
                                    where T : class
                                {
                                }

                                public class Base
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment in the base list of a struct is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5043-001")]
    public async Task VerifyCommentInStructBaseListIsReported()
    {
        const string testData = """
                                public struct Data :
                                    {|#0:// contract|}
                                    IContract
                                {
                                }

                                public interface IContract
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH5043CommentsMustNotBePlacedInsideBaseListAnalyzer.DiagnosticId, AnalyzerResources.RH5043MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment in the base list of an interface is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5043-001")]
    public async Task VerifyCommentInInterfaceBaseListIsReported()
    {
        const string testData = """
                                public interface IDerived : {|#0:/* contract */|} IContract
                                {
                                }

                                public interface IContract
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH5043CommentsMustNotBePlacedInsideBaseListAnalyzer.DiagnosticId, AnalyzerResources.RH5043MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment in the base list of a record is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5043-001")]
    public async Task VerifyCommentInRecordBaseListIsReported()
    {
        const string testData = """
                                public record Derived :
                                    {|#0:// base|}
                                    Base;

                                public record Base;
                                """;

        await Verify(testData, Diagnostics(RH5043CommentsMustNotBePlacedInsideBaseListAnalyzer.DiagnosticId, AnalyzerResources.RH5043MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the colon of a base list is reported with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5043-001")]
    public async Task VerifyCommentAfterBaseListColonIsReportedWithCrLfLineEndings()
    {
        const string testData = """
                                public class Repository :
                                    {|#0:// storage|}
                                    Base
                                {
                                }

                                public class Base
                                {
                                }
                                """;

        await Verify(testData.ReplaceLineEndings("\r\n"), Diagnostics(RH5043CommentsMustNotBePlacedInsideBaseListAnalyzer.DiagnosticId, AnalyzerResources.RH5043MessageFormat));
    }

    #endregion // Tests
}