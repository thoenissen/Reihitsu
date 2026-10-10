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

    #endregion // Tests
}