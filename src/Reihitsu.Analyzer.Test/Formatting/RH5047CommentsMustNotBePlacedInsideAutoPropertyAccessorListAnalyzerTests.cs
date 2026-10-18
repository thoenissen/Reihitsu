using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5047CommentsMustNotBePlacedInsideAutoPropertyAccessorListAnalyzer"/>
/// </summary>
[TestClass]
public class RH5047CommentsMustNotBePlacedInsideAutoPropertyAccessorListAnalyzerTests : AnalyzerTestsBase<RH5047CommentsMustNotBePlacedInsideAutoPropertyAccessorListAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment between the property signature and its accessor list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5047-002")]
    public async Task VerifyCommentBeforeAccessorListIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int Value
                                        {|#0:// storage|}
                                        { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5047CommentsMustNotBePlacedInsideAutoPropertyAccessorListAnalyzer.DiagnosticId, AnalyzerResources.RH5047MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before an accessor without a body is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5047-001")]
    [Requirement("SHARED-COMMENT-005")]
    public async Task VerifyCommentBeforeAutoAccessorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int Other
                                    {
                                        {|#0:// read|}
                                        get;
                                        set;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5047CommentsMustNotBePlacedInsideAutoPropertyAccessorListAnalyzer.DiagnosticId, AnalyzerResources.RH5047MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the closing brace of an auto-property accessor list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5047-001")]
    [Requirement("SHARED-COMMENT-003")]
    [Requirement("SHARED-COMMENT-005")]
    public async Task VerifyCommentBeforeClosingBraceIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int Value { get; {|#0:/* write */|} }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5047CommentsMustNotBePlacedInsideAutoPropertyAccessorListAnalyzer.DiagnosticId, AnalyzerResources.RH5047MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside an abstract property accessor list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5047-001")]
    public async Task VerifyCommentInAbstractPropertyAccessorListIsReported()
    {
        const string testData = """
                                public abstract class Base
                                {
                                    public abstract int Value { {|#0:/* read */|} get; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5047CommentsMustNotBePlacedInsideAutoPropertyAccessorListAnalyzer.DiagnosticId, AnalyzerResources.RH5047MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside an interface indexer accessor list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5047-001")]
    public async Task VerifyCommentInInterfaceIndexerAccessorListIsReported()
    {
        const string testData = """
                                public interface IStore
                                {
                                    int this[int index]
                                    {
                                        {|#0:// read|}
                                        get;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5047CommentsMustNotBePlacedInsideAutoPropertyAccessorListAnalyzer.DiagnosticId, AnalyzerResources.RH5047MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before an accessor with a body is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5047-001")]
    public async Task VerifyNoDiagnosticForAccessorWithBody()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _value;

                                    public int Value
                                    {
                                        // read
                                        get { return _value; }
                                        set { _value = value; }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before an accessor list containing an expression-bodied accessor is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5047-001")]
    [Requirement("RH5047-002")]
    public async Task VerifyNoDiagnosticForAccessorWithExpressionBody()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int Value
                                        // computed
                                        { get => 1; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after an auto-property accessor list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5047-002")]
    public async Task VerifyNoDiagnosticForCommentAfterAccessorList()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int Value { get; }

                                    // next
                                    public int Other { get; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before an auto-property is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5047-002")]
    public async Task VerifyNoDiagnosticForCommentBeforeProperty()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    // storage, read and write
                                    public int Value { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment in the accessor list of an event is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5047-001")]
    public async Task VerifyNoDiagnosticForCommentInEventAccessorList()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    public event Action Changed
                                    {
                                        // subscribe
                                        add { }
                                        remove { }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment inside an accessor list whose accessor has an expression body is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5047-001")]
    public async Task VerifyNoDiagnosticForCommentInsideAccessorListWithExpressionBody()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int Computed
                                    {
                                        // computed
                                        get => 1;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before the accessor list of an interface property is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5047-002")]
    public async Task VerifyCommentBeforeAccessorListOfInterfacePropertyIsReported()
    {
        const string testData = """
                                public interface IStore
                                {
                                    int Value
                                        {|#0:// storage|}
                                        { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5047CommentsMustNotBePlacedInsideAutoPropertyAccessorListAnalyzer.DiagnosticId, AnalyzerResources.RH5047MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before an auto accessor is reported with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5047-001")]
    public async Task VerifyCommentBeforeAutoAccessorIsReportedWithCrLfLineEndings()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int Other
                                    {
                                        {|#0:// read|}
                                        get;
                                        set;
                                    }
                                }
                                """;

        await Verify(testData.ReplaceLineEndings("\r\n"), Diagnostics(RH5047CommentsMustNotBePlacedInsideAutoPropertyAccessorListAnalyzer.DiagnosticId, AnalyzerResources.RH5047MessageFormat));
    }

    #endregion // Tests
}