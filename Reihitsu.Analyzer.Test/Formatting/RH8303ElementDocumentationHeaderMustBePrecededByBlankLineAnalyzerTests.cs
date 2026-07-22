using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer"/> and <see cref="RH8303ElementDocumentationHeaderMustBePrecededByBlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzerTests : AnalyzerTestsBase<RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer, RH8303ElementDocumentationHeaderMustBePrecededByBlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void First()
                                    {
                                    }

                                    /// <summary>
                                    /// Summary.
                                    /// </summary>
                                    void Second()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void First()
                                    {
                                    }
                                    {|#0:///|} <summary>
                                    /// Summary.
                                    /// </summary>
                                    void Second()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void First()
                                     {
                                     }
                                 
                                     /// <summary>
                                     /// Summary.
                                     /// </summary>
                                     void Second()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH8303MessageFormat));
    }

    /// <summary>
    /// Verifies that documentation at the beginning of a scope does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationAtStartOfScopeDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Summary.
                                    /// </summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an ordinary four-slash comment after code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFourSlashCommentDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void First()
                                    {
                                    }
                                    //// Ordinary comment.
                                    void Second()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the inserted blank line matches the document's detected CRLF end-of-line sequence instead of
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings (issue #257)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInsertedBlankLineUsesDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void First()
                                    {
                                    }
                                    /// <summary>
                                    /// Summary.
                                    /// </summary>
                                    void Second()
                                    {
                                    }
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.DoesNotContain("\n", fixedSource.Replace("\r\n", string.Empty));
    }

    /// <summary>
    /// Verifies that documentation-like text in multi-line comments does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineCommentsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /*
                                    comment text
                                    /// Not documentation
                                    */
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that documentation-like text in disabled regions does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDisabledTextDoesNotProduceDiagnostics()
    {
        const string testData = """
                                #if false
                                internal class FirstClass
                                {
                                }
                                /// Not documentation
                                internal class SecondClass
                                {
                                }
                                #endif
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that documentation lines are ignored when documentation mode is disabled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void First()
                                    {
                                    }
                                    /// Summary.
                                    void Second()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}