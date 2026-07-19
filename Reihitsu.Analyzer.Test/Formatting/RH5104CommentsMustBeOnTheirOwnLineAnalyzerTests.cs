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

    /// <summary>
    /// Verifies that the inserted line break matches the document's detected CRLF end-of-line sequence instead of
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings (issue #257)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInsertedLineBreakUsesDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool left, bool right)
                                    {
                                        if (left == right) // Compare the values.
                                        {
                                            return;
                                        }
                                    }
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.DoesNotContain("\n", fixedSource.Replace("\r\n", string.Empty));
    }

    /// <summary>
    /// Verifies that a comment inside an interpolation hole on a continuation line of a multi-line
    /// verbatim interpolated string does not produce a diagnostic, since relocating it would insert
    /// text into the string's literal content (issue #412)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyHoleCommentInMultiLineVerbatimInterpolatedStringDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    string Method(int x)
                                    {
                                        return $@"line1
                                line2 {x /* c */} end";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment inside an interpolation hole on a continuation line of a multi-line
    /// raw interpolated string literal does not produce a diagnostic, since relocating it would insert
    /// text into the string's literal content (issue #412)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyHoleCommentInMultiLineRawInterpolatedStringDoesNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    string Method(int x)
                                    {
                                        return $"""
                                            line1
                                            line2 {x /* c */} end
                                            """;
                                    }
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment inside a single-line interpolated string nested within a hole of an
    /// outer multi-line verbatim interpolated string does not produce a diagnostic. The exemption must
    /// walk every enclosing interpolated string, not just the innermost one, since relocating the comment
    /// would still insert text into the outer string's literal content (issue #412 review follow-up)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyHoleCommentInSingleLineInterpolatedStringNestedInMultiLineOuterStringDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    string Method(int x)
                                    {
                                        return $@"line1
                                line2 {$"nested {x /* c */}"} end";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment inside an interpolation hole of a single-line interpolated string is
    /// still detected and fixed, confirming the multi-line exemption is scoped narrowly (issue #412)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleLineInterpolatedStringHoleCommentIsStillDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    string Method(int x)
                                    {
                                        return $"abc {x{|#0:/* c */|}} end";
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     string Method(int x)
                                     {
                                         /* c */
                                         return $"abc {x} end";
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat));
    }

    /// <summary>
    /// Verifies that relocating a trailing comment inserts a blank line above it when the preceding
    /// line is code, matching the RH5020 policy so the fix does not immediately raise a new diagnostic
    /// (issue #412)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRelocatedCommentGetsPrecedingBlankLineWhenPreviousLineIsCode()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool left, bool right)
                                    {
                                        var first = 1;
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
                                         var first = 1;

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
    /// Verifies that relocating a trailing comment does not duplicate an already-existing blank line
    /// above the target line (issue #412)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRelocatedCommentDoesNotDuplicateExistingBlankLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool left, bool right)
                                    {
                                        var first = 1;

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
                                         var first = 1;

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
    /// Verifies that removing a multi-line comment which carries the only line break between two
    /// statements re-inserts the break, so the two statements are not joined onto one line and RH5103
    /// is not immediately raised (issue #412)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineBreakCarryingCommentRemovalDoesNotJoinStatements()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int x = 1; {|#0:/* a
                                b */|} int y = 2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         /* a
                                 b */
                                         int x = 1;
                                         int y = 2;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat));
    }

    #endregion // Tests
}