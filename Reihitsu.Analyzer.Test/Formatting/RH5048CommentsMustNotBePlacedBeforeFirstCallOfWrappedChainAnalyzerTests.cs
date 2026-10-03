using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer"/>
/// </summary>
[TestClass]
public class RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzerTests : AnalyzerTestsBase<RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment before the first call of a wrapped chain is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeFirstCallOfWrappedChainIsReported()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var query = items
                                            {|#0:// only positive|}
                                            .Where(value => value > 0)
                                            .Select(value => value)
                                            .ToList();
                                        _ = query;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer.DiagnosticId, AnalyzerResources.RH5048MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the first call of a wrapped chain is reported with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeFirstCallOfWrappedChainWithCrLfIsReported()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var query = items
                                            {|#0:// only positive|}
                                            .Where(value => value > 0)
                                            .ToList();
                                        _ = query;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData.Replace("\n", "\r\n"), Diagnostics(RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer.DiagnosticId, AnalyzerResources.RH5048MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment sharing the line with the receiver of a wrapped chain is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySameLineCommentBeforeWrappedFirstCallIsReported()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var query = items {|#0:// only positive|}
                                            .Where(value => value > 0)
                                            .ToList();
                                        _ = query;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer.DiagnosticId, AnalyzerResources.RH5048MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a later call of a chain is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentBeforeLaterCall()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var query = items.Where(value => value > 0)

                                                         // project
                                                         .Select(value => value)
                                                         .ToList();
                                        _ = query;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before the first call of a chain that is not wrapped is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUnwrappedChain()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var query = items /* only positive */.Where(value => value > 0).ToList();
                                        _ = query;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment before the first call after an intermediate member access is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticAfterIntermediateMemberAccess()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var text = items.Length
                                            // text
                                            .ToString();
                                        _ = text;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}