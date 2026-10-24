using System;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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
    /// Verifies that relocating a trailing comment inserts no blank line above it when the preceding
    /// line is code; separating the comment from that code is the concern of RH5020.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-005")]
    public async Task VerifyRelocatedCommentGetsNoBlankLineWhenPreviousLineIsCode()
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

    /// <summary>
    /// Verifies that two block comments sharing a line without code are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-002")]
    public async Task VerifyNoDiagnosticsForTwoBlockCommentsOnALineWithoutCode()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        /* a */ /* b */
                                        N(x);
                                    }
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a block comment followed by a line comment on a line without code are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-002")]
    public async Task VerifyNoDiagnosticsForBlockCommentAndLineCommentOnALineWithoutCode()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        /* c */ // d
                                        N(x);
                                    }
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a block comment sharing the last line of a multi-line comment, with no code on either line, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-002")]
    public async Task VerifyNoDiagnosticsForBlockCommentBehindTheLastLineOfAMultiLineComment()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        /* x
                                           y */ /* b */
                                        N(x);
                                    }
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a block comment behind a delimited documentation comment on a line above a member is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-002")]
    public async Task VerifyNoDiagnosticsForBlockCommentBehindDelimitedDocumentationComment()
    {
        const string testData = """"
                                class C
                                {
                                    /** a */ /* b */
                                    void M()
                                    {
                                    }
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a line comment behind a delimited documentation comment on a line above a member is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-002")]
    public async Task VerifyNoDiagnosticsForLineCommentBehindDelimitedDocumentationComment()
    {
        const string testData = """"
                                class C
                                {
                                    /** a */ // b
                                    void M()
                                    {
                                    }
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that two comments on the last line of the file, with no line break behind them, are not reported, because the end of the file is no code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-002")]
    public async Task VerifyNoDiagnosticsForCommentsOnTheLastLineWithoutLineBreak()
    {
        const string testData = """"
                                class C
                                {
                                }
                                /* a */ // b
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment on the last line of the file, with no line break behind it, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-002")]
    public async Task VerifyNoDiagnosticsForCommentOnTheLastLineWithoutLineBreak()
    {
        const string testData = """"
                                class C
                                {
                                }
                                // end
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a block comment in front of code and a line comment behind it are both reported, although they share the line with each other
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-001")]
    public async Task VerifyCommentsAroundCodeAreReportedWhenAnotherCommentSharesTheLine()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        {|RH5104:/* a */|} N(x); {|RH5104:// b|}
                                    }
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that two block comments in front of code on their line are both reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-001")]
    public async Task VerifyTwoBlockCommentsInFrontOfCodeAreReported()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        {|RH5104:/* a */|} {|RH5104:/* b */|} N(x);
                                    }
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the spec example: the fix inserts no blank line above the moved comment, whatever the line above holds, and keeps the blank line the author wrote
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-005")]
    public async Task VerifyFixInsertsNoBlankLineAboveTheMovedComment()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        M(); {|RH5104:// first|}
                                        M(); {|RH5104:// second|}
                                        // note
                                        M(); {|RH5104:// third|}

                                        M(); {|RH5104:// fourth|}
                                        switch (x)
                                        {
                                            case 1:
                                                M(); {|RH5104:// fifth|}
                                                break;
                                        }
                                    }

                                    void M()
                                    {
                                    }
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     void N(int x)
                                     {
                                         // first
                                         M();
                                         // second
                                         M();
                                         // note
                                         // third
                                         M();

                                         // fourth
                                         M();
                                         switch (x)
                                         {
                                             case 1:
                                                 // fifth
                                                 M();
                                                 break;
                                         }
                                     }

                                     void M()
                                     {
                                     }
                                 }
                                 """";

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that no fix is offered for a comment whose line begins inside a delimited documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-006")]
    public async Task VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideADelimitedDocumentationComment()
    {
        const string testData = """"
                                class C
                                {
                                    /** <summary>
                                    M</summary> */ void M() { } {|RH5104:// c|}
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no fix is offered for a comment whose line begins inside a verbatim string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-006")]
    public async Task VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideAVerbatimString()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        var a = @"a
                                b" + {|RH5104:/* c */|} x;
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no fix is offered for a comment whose line begins inside the closing line of a raw string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-006")]
    public async Task VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideARawString()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        var b = """
                                            a
                                            """ + {|RH5104:/* c */|} x;
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no fix is offered for a comment whose line begins inside the closing line of an interpolated raw string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-006")]
    public async Task VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideAnInterpolatedRawString()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        var c = $"""
                                            {x}
                                            """ + {|RH5104:/* c */|} x;
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no fix is offered for a comment whose line begins inside the closing line of a UTF-8 raw string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-006")]
    public async Task VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideAUtf8RawString()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        var d = """
                                            a
                                            """u8.Length + {|RH5104:/* c */|} x;
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no fix is offered for a comment whose line begins at the opening brace of a hole of an interpolated verbatim string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-006")]
    public async Task VerifyNoCodeFixIsOfferedWhenTheLineStartsAtAHoleOfAnInterpolatedVerbatimString()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        var e = $@"a
                                {x}b" + {|RH5104:/* c */|} x;
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no fix is offered for a comment whose line begins at the opening brace of a hole of a string written with the verbatim marker first
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-006")]
    public async Task VerifyNoCodeFixIsOfferedWhenTheLineStartsAtAHoleOfAVerbatimInterpolatedString()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        var g = @$"a
                                {x}b" + {|RH5104:/* c */|} x;
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no fix is offered for a comment whose line begins in the text of an interpolated verbatim string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-006")]
    public async Task VerifyNoCodeFixIsOfferedWhenTheLineStartsInTheTextOfAnInterpolatedVerbatimString()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        var e = $@"a
                                b{x}" + {|RH5104:/* c */|} x;
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no fix is offered for a comment whose line begins inside a hole of an interpolated string that spans lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-006")]
    public async Task VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideAHoleThatSpansLines()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        var f = $"{
                                            x}" + {|RH5104:/* c */|} x;
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no fix is offered for a line comment whose line begins inside a block comment, while the block comment, whose own line begins outside, still gets one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-006")]
    public async Task VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideABlockComment()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        /* a
                                           b */ N(x); // c
                                    }
                                }
                                """";

        await Verify(testData.Replace("/* a", "{|RH5104:/* a").Replace("b */", "b */|}").Replace("// c", "{|RH5104:// c|}"));

        var lineCommentActions = await GetCodeFixActionsAsync(testData,
                                                              RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId,
                                                              root => Location.Create(root.SyntaxTree, new TextSpan(testData.IndexOf("// c", StringComparison.Ordinal), 4)));
        var blockCommentActions = await GetCodeFixActionsAsync(testData,
                                                               RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId,
                                                               root => Location.Create(root.SyntaxTree, TextSpan.FromBounds(testData.IndexOf("/* a", StringComparison.Ordinal), testData.IndexOf("b */", StringComparison.Ordinal) + 4)));

        Assert.IsEmpty(lineCommentActions, "No code fix may be offered for the comment whose line begins inside the block comment.");
        Assert.IsNotEmpty(blockCommentActions, "The block comment whose line begins outside a comment must keep its code fix.");
    }

    /// <summary>
    /// Verifies the spec example: a comment on the line where a multi-line verbatim string starts is moved above that line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-006")]
    public async Task VerifyCommentOnTheLineWhereAVerbatimStringStartsIsMoved()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        var a = {|RH5104:/* c */|} @"a
                                b" + x;
                                    }
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     void N(int x)
                                     {
                                         /* c */
                                         var a = @"a
                                 b" + x;
                                     }
                                 }
                                 """";

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that a comment on the line where an interpolated string with a hole that spans lines starts is moved above that line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-006")]
    public async Task VerifyCommentOnTheLineWhereAHoleThatSpansLinesStartsIsMoved()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        var f = {|RH5104:/* c */|} $"{
                                            x}" + x;
                                    }
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     void N(int x)
                                     {
                                         /* c */
                                         var f = $"{
                                             x}" + x;
                                     }
                                 }
                                 """";

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that Fix All moves a fixable comment and leaves a comment whose line begins inside a literal reported and in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5104-006")]
    public async Task VerifyFixAllMovesFixableCommentAndLeavesWithheldCommentReported()
    {
        const string testData = """"
                                class C
                                {
                                    void N(int x)
                                    {
                                        var a = @"a
                                b" + {|RH5104:/* c */|} x;
                                        N(x); {|RH5104:// d|}
                                    }
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     void N(int x)
                                     {
                                         var a = @"a
                                 b" + {|RH5104:/* c */|} x;
                                         // d
                                         N(x);
                                     }
                                 }
                                 """";

        await Verify(testData, fixedData);
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