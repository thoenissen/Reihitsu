using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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
    [Requirement("RH5048-001")]
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
    [Requirement("RH5048-001")]
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
    [Requirement("RH5048-001")]
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
    [Requirement("RH5048-001")]
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
    [Requirement("RH5048-001")]
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
    [Requirement("RH5048-001")]
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

    /// <summary>
    /// Verifies that a comment inside a preprocessor directive before the first call of a wrapped chain is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5048-001")]
    [Requirement("SHARED-COMMENT-002")]
    public async Task VerifyNoDiagnosticForCommentInsideDirectiveBeforeFirstCall()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var query = items
                                        #if !UNDEFINED_SYMBOL // positive only
                                        #endif
                                            .Where(value => value > 0)
                                            .ToList();
                                        _ = query;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment in disabled code before the first call of a wrapped chain is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5048-001")]
    public async Task VerifyNoDiagnosticForCommentInDisabledCodeBeforeFirstCall()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var query = items
                                        #if UNDEFINED_SYMBOL
                                            // only positive
                                        #endif
                                            .Where(value => value > 0)
                                            .ToList();
                                        _ = query;
                                    }

                                    static int Run(params object[] values) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment in active conditional code before the first call of a wrapped chain is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5048-001")]
    public async Task VerifyCommentInActiveConditionalCodeBeforeFirstCallIsReported()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(bool a, bool b, int[] items, object o)
                                    {
                                        var query = items
                                        #if !UNDEFINED_SYMBOL
                                            {|#0:// only positive|}
                                        #endif
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
    /// Verifies that a comment in front of a wrapped conditional access that is the chain's first link is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5048-001")]
    [Requirement("SHARED-CHAIN-001")]
    public async Task VerifyCommentBeforeWrappedConditionalFirstLinkIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private TestClass Self() => this;

                                    void Method(TestClass c)
                                    {
                                        var a = c {|#0:// self|}
                                            ?.Self();
                                        _ = a;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer.DiagnosticId, AnalyzerResources.RH5048MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment in front of a wrapped null-forgiving access that is the chain's first link is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5048-001")]
    [Requirement("SHARED-CHAIN-001")]
    public async Task VerifyCommentBeforeWrappedNullForgivingFirstLinkIsReported()
    {
        const string testData = """
                                #nullable enable

                                internal class TestClass
                                {
                                    private TestClass Self() => this;

                                    void Method(TestClass? c)
                                    {
                                        var a = c
                                            {|#0:// self|}
                                            !.Self();
                                        _ = a;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer.DiagnosticId, AnalyzerResources.RH5048MessageFormat));
    }

    /// <summary>
    /// Verifies that every comment in front of a wrapped first link is reported on its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5048-001")]
    [Requirement("SHARED-COMMENT-003")]
    public async Task VerifyEveryCommentBeforeWrappedFirstLinkIsReportedSeparately()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(int[] items)
                                    {
                                        var query = items {|#0:/* source */|}
                                            {|#1:// only positive|}
                                            .Where(value => value > 0);
                                        _ = query;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer.DiagnosticId, AnalyzerResources.RH5048MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a comment inside the argument list of the root is not reported, because it is not in the gap in
    /// front of the first link's operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5048-001")]
    [Requirement("SHARED-CHAIN-001")]
    public async Task VerifyCommentInsideAttachedArgumentListOfRootIsNotReported()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var query = Get(/* value */ 1)
                                            .Where(value => value > 0);
                                        _ = query;
                                    }

                                    int[] Get(int x) => [x];
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment in front of an element access attached to the root is not reported, because the gap in
    /// front of the first link's operator holds no comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5048-001")]
    [Requirement("SHARED-CHAIN-001")]
    public async Task VerifyCommentBeforeAttachedElementAccessOfRootIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int[] items)
                                    {
                                        var text = items // element
                                            ?[0]
                                            .ToString();
                                        _ = text;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a block comment in front of a first link that stays on the root's line is not reported, and
    /// neither is a comment in front of a later link of the same chain
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5048-001")]
    public async Task VerifyInlineCommentBeforeFirstLinkOnRootLineIsNotReported()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    void Method(int[] items)
                                    {
                                        var query = items /* inline */ .Where(value => value > 0)
                                                                       // materialize
                                                                       .ToList();
                                        _ = query;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the change-doc example: an ordinary comment in front of a first link that starts its line behind a
    /// documentation comment written behind the root is reported, as behind a single-line comment. Only the report of
    /// the ordinary comment is asserted; whether the documentation comment itself is reported is not decided here
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5048-001")]
    [Requirement("SHARED-BLANK-008")]
    public async Task VerifyCommentBeforeFirstLinkBehindStrayDocumentationCommentIsReported()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    internal object Method(int[] items)
                                    {
                                        return items /* c */ /// x
                                            .Where(value => value > 0)
                                            .ToList();
                                    }
                                }
                                """;
        const string comment = "/* c */";

        foreach (var endOfLine in new[] { "\n", "\r\n" })
        {
            var source = testData.Replace("\r\n", "\n").Replace("\n", endOfLine);
            var commentStart = source.IndexOf(comment, StringComparison.Ordinal);
            var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

            Assert.IsTrue(diagnostics.Any(diagnostic => diagnostic.Id == RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer.DiagnosticId
                                                        && diagnostic.Location.SourceSpan.Start == commentStart
                                                        && diagnostic.Location.SourceSpan.Length == comment.Length),
                          $"The comment in front of the first link should be reported under {(endOfLine == "\n" ? "LF" : "CRLF")} line endings.");
        }
    }

    /// <summary>
    /// Verifies the control of the documentation-comment case: an ordinary comment in front of a first link that starts
    /// its line behind a single-line comment is reported, and so is the single-line comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5048-001")]
    public async Task VerifyCommentBeforeFirstLinkBehindSingleLineCommentIsReported()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    internal object Method(int[] items)
                                    {
                                        return items {|#0:/* c */|} {|#1:// x|}
                                            .Where(value => value > 0)
                                            .ToList();
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer.DiagnosticId, AnalyzerResources.RH5048MessageFormat, 2));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Runs the analyzer over the source and returns every diagnostic it reports, so a test can assert one diagnostic
    /// without deciding the others
    /// </summary>
    /// <param name="source">The source text</param>
    /// <returns>The analyzer diagnostics</returns>
    private static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: CancellationToken.None);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator)
                                                                                    .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("Test", [tree], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return await compilation.WithAnalyzers([new RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer()])
                                .GetAnalyzerDiagnosticsAsync(CancellationToken.None)
                                .ConfigureAwait(false);
    }

    #endregion // Methods
}