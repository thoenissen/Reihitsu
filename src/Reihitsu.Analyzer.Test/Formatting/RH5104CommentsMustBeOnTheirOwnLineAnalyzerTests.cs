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
public class RH5104CommentsMustBeOnTheirOwnLineAnalyzerTests : BatchCodeFixTestsBase<RH5104CommentsMustBeOnTheirOwnLineAnalyzer, RH5104CommentsMustBeOnTheirOwnLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that comments on separate lines do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-002")]
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
    [Requirement("RH5104-001")]
    [Requirement("RH5104-003")]
    [Requirement("RH5104-004")]
    [Requirement("RH5104-005")]
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
    [Requirement("RH5104-001")]
    [Requirement("RH5104-004")]
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
    [Requirement("RH5104-001")]
    [Requirement("RH5104-003")]
    [Requirement("RH5104-004")]
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
    [Requirement("RH5104-002")]
    [Requirement("SHARED-COMMENT-002")]
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
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-004")]
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
    /// text into the string's literal content.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-002")]
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
    /// text into the string's literal content.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-002")]
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
    /// would still insert text into the outer string's literal content.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-002")]
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
    /// still detected and fixed, confirming the multi-line exemption is scoped narrowly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-001")]
    [Requirement("RH5104-004")]
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
    /// line is code, matching the RH5020 policy so the fix does not immediately raise a new diagnostic.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-005")]
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
    /// above the target line.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-005")]
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
    /// is not immediately raised.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-001")]
    [Requirement("RH5104-004")]
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

    /// <summary>
    /// Verifies that a comment behind a field declaration is reported on the comment and moved above the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-001")]
    [Requirement("RH5104-003")]
    [Requirement("RH5104-004")]
    public async Task VerifyTrailingCommentBehindFieldIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _a; {|#0:// first|}
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     // first
                                     private int _a;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat));
    }

    /// <summary>
    /// Verifies that a block comment in front of the code on its line is reported and moved above that line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-001")]
    [Requirement("RH5104-004")]
    public async Task VerifyLeadingBlockCommentInFrontOfCodeIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        {|#0:/* lead */|} Method();
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         /* lead */
                                         Method();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment behind an active preprocessor directive is not reported, because it belongs to the
    /// directive line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-002")]
    [Requirement("SHARED-COMMENT-002")]
    public async Task VerifyCommentBehindConditionalDirectiveDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                #if DEBUG // debug
                                        Method();
                                #endif
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment moved below a line that starts with a <c>//</c> comment gets no blank line in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-005")]
    public async Task VerifyRelocatedCommentBelowLineCommentGetsNoBlankLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Method();
                                        // note
                                        Method(); {|#0:// third|}
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         Method();
                                         // note
                                         // third
                                         Method();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment moved onto the first line after a switch label gets no blank line in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-005")]
    public async Task VerifyRelocatedCommentBelowSwitchLabelGetsNoBlankLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                Method(value); {|#0:// fourth|}
                                                break;
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 // fourth
                                                 Method(value);
                                                 break;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment moved onto the line after a preprocessor directive gets no blank line in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-005")]
    public async Task VerifyRelocatedCommentBelowDirectiveGetsNoBlankLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Method();
                                #if !UNDEFINED_SYMBOL
                                        Method(); {|#0:// debug only|}
                                #endif
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         Method();
                                 #if !UNDEFINED_SYMBOL
                                         // debug only
                                         Method();
                                 #endif
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat));
    }

    /// <summary>
    /// Verifies that a block comment followed by code on its line is moved together with the whitespace behind it, so
    /// the whitespace in front of it stays on the line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-001")]
    [Requirement("RH5104-004")]
    public async Task VerifyBlockCommentInFrontOfClosingParenthesisKeepsTheWhitespaceInFrontOfIt()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int value)
                                    {
                                        Method(value {|#0:/* value */|});
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int value)
                                     {
                                         /* value */
                                         Method(value );
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void Method(bool left, bool right)
                                    {
                                        if (left {|#0:/* c1 */|} == right {|#1:/* c2 */|})
                                        {
                                            return;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     void Method(bool left, bool right)
                                     {
                                         /* c1 */
                                         /* c2 */
                                         if (left == right )
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        // Both comments relocate to the same insertion point (the start of the "if" line), so their zero-length
        // insertion edits collide at the same offset in the original document; the batch fixer applies only one
        // insertion per pass and needs a second pass to relocate the comment it deferred
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat, 2),
                                  test => test.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}