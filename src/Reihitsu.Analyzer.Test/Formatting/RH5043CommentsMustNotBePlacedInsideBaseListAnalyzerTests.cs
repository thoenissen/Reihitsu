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

    #endregion // Tests
}