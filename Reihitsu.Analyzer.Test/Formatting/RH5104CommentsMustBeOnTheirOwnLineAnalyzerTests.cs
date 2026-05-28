using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5104CommentsMustBeOnTheirOwnLineAnalyzer"/> and <see cref="RH5104CommentsMustBeOnTheirOwnLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5104CommentsMustBeOnTheirOwnLineAnalyzerTests : AnalyzerTestsBase<RH5104CommentsMustBeOnTheirOwnLineAnalyzer, RH5104CommentsMustBeOnTheirOwnLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that comments on separate lines do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCommentsAreOnOwnLines()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool left, bool right)
                                    {
                                        // Compare the values.
                                        if (left == right)
                                        {
                                            /*
                                             * Leave early when both values match.
                                             */
                                            return;
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a trailing single-line comment is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingSingleLineCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool left, bool right)
                                    {
                                        if (left == right) {|#0:// Compare the values.|}
                                        {
                                            return;
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(bool left, bool right)
                                     {
                                         // Compare the values.
                                         if (left == right)
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat));
    }

    /// <summary>
    /// Verifies that a trailing multi-line comment is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingMultiLineCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool left, bool right)
                                    {
                                        if (left == right) {|#0:/* Compare the values. */|}
                                        {
                                            return;
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(bool left, bool right)
                                     {
                                         /* Compare the values. */
                                         if (left == right)
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat));
    }

    /// <summary>
    /// Verifies that an inline block comment inside an expression is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInlineBlockCommentInExpressionIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool left, bool right)
                                    {
                                        if (left {|#0:/* Compare the values. */|} == right)
                                        {
                                            return;
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(bool left, bool right)
                                     {
                                         /* Compare the values. */
                                         if (left == right)
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat));
    }

    /// <summary>
    /// Verifies that comments attached to region directives do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEndRegionCommentsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods

                                    void Method()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}