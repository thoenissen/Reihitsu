using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer"/> and <see cref="RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzerTests : AnalyzerTestsBase<RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer, RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineCodeFixProvider>
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
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Summary.
                                    /// </summary>
                                {|#0:
                                |}    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
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

        await Verify(testData, fixedData, Diagnostics(RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH8302MessageFormat));
    }

    /// <summary>
    /// Verifies that raw-string content does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRawStringContentDoesNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    private const string Value = """
                                                                 /// <summary>
                                                                 /// Summary.
                                                                 /// </summary>
                                                                 
                                                                 body
                                                                 """;
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an ordinary four-slash comment followed by a blank line does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFourSlashCommentDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    //// Ordinary comment.

                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the blank line after a four-slash fenced-out code block is not flagged as following a
    /// documentation header, so its legitimate separator before a real documentation block survives (issue #449)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlankLineAfterFourSlashBlockDoesNotProduceDiagnostics()
    {
        const string testData = """
                                public class C
                                {
                                    ////int old;
                                    ////int older;

                                    /// <summary>
                                    /// Does X.
                                    /// </summary>
                                    public void M()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
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
                                    /// Not documentation

                                    comment text
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
                                /// Not documentation

                                internal class DisabledClass
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
                                    /// Summary.

                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}