using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3201CommentsMustContainTextAnalyzer"/> and <see cref="RH3201CommentsMustContainTextCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3201CommentsMustContainTextAnalyzerTests : BatchCodeFixTestsBase<RH3201CommentsMustContainTextAnalyzer, RH3201CommentsMustContainTextCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying empty single-line comment is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-001")]
    [Requirement("RH3201-004")]
    [Requirement("RH3201-005")]
    public async Task EmptySingleLineCommentIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0://|}
                                        var value = 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text."));
    }

    /// <summary>
    /// Verifying single-line comment with only whitespace is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-001")]
    [Requirement("RH3201-004")]
    [Requirement("RH3201-005")]
    public async Task SingleLineCommentWithWhitespaceIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0://   |}
                                        var value = 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text."));
    }

    /// <summary>
    /// Verifying empty multi-line comment is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-001")]
    [Requirement("RH3201-004")]
    [Requirement("RH3201-005")]
    public async Task EmptyMultiLineCommentIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0:/**/|}
                                        var value = 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text."));
    }

    /// <summary>
    /// Verifying multi-line comment with only whitespace is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-001")]
    [Requirement("RH3201-004")]
    [Requirement("RH3201-005")]
    public async Task MultiLineCommentWithWhitespaceIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0:/*   */|}
                                        var value = 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text."));
    }

    /// <summary>
    /// Verifying valid single-line comment is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-001")]
    public async Task ValidSingleLineCommentIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        // This is a valid comment
                                        var value = 1;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying valid multi-line comment is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-001")]
    public async Task ValidMultiLineCommentIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        /* This is a valid comment */
                                        var value = 1;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty comment after statement is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-001")]
    [Requirement("RH3201-004")]
    [Requirement("RH3201-005")]
    public async Task EmptyCommentAfterStatementIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        var value = 1; {|#0://|}
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text."));
    }

    /// <summary>
    /// Verifying empty multi-line comment with newlines is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-001")]
    [Requirement("RH3201-004")]
    public async Task EmptyMultiLineCommentWithNewlinesIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0:/*
                                        */|}
                                        var value = 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {

                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text."));
    }

    /// <summary>
    /// Verifying empty separator lines inside single-line comment blocks are allowed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-002")]
    public async Task EmptySingleLineCommentInsideCommentBlockIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        // Comment 1
                                        //
                                        // Comment 2
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying an empty separator before an ordinary four-slash comment is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-002")]
    public async Task EmptySingleLineCommentBeforeFourSlashCommentIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        // First
                                        //
                                        //// Second
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying an unterminated multi-line comment at the end of the file is not reported and does not crash the analyzer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-003")]
    public async Task UnterminatedMultiLineCommentAtEndOfFileIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                }
                                /*
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying an unterminated documentation-style multi-line comment at the end of the file is not reported and does not crash the analyzer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-003")]
    public async Task UnterminatedDocumentationStyleMultiLineCommentAtEndOfFileIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                }
                                /**
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.CompilerDiagnostics = CompilerDiagnostics.None;
                         test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject);
                     });
    }

    /// <summary>
    /// Verifying an unterminated multi-line comment whose closing slash is missing is not reported and does not crash the analyzer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-003")]
    public async Task UnterminatedSlashStarSlashCommentAtEndOfFileIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                }
                                /*/
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying a longer unterminated multi-line comment is treated as non-empty and its trailing content is not chopped
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-003")]
    public async Task LongUnterminatedMultiLineCommentWithTrailingContentIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                }
                                /*  hi
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying the rule whose subject is the text of a comment checks that text and reports a comment without text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-003")]
    [Requirement("RH3201-001")]
    [Requirement("RH3201-004")]
    public async Task EmptyCommentIsReportedBecauseCommentTextIsTheRuleSubject()
    {
        const string testCode = """
                                class C
                                {
                                    {|#0://|}
                                    int x;
                                }
                                """;

        await Verify(testCode, Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, AnalyzerResources.RH3201MessageFormat));
    }

    /// <summary>
    /// Verifies empty <c>//</c> and <c>/* */</c> comments on their own line and after code are reported over the
    /// whole comment, while a comment with text is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-001")]
    [Requirement("RH3201-004")]
    public async Task EmptyCommentsAreReportedOverTheWholeComment()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH3201://|}
                                    int _a;
                                    {|RH3201:/* */|}
                                    int _b; {|RH3201://|}
                                    int _c;
                                    // text
                                    int _d;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an empty <c>//</c> line between two <c>//</c> lines and between two <c>////</c> lines is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-002")]
    public async Task EmptyLineInsideBlocksOfSlashCommentsOnMembersIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    // First.
                                    //
                                    // Second.
                                    int _a;

                                    //// First.
                                    //
                                    //// Second.
                                    int _b;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an empty <c>//</c> line next to a documentation comment that documents a declaration is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-002")]
    public async Task EmptyLineNextToADocumentationCommentIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    // First.
                                    {|RH3201://|}
                                    /// <summary>Second.</summary>
                                    int _a;
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     // First.
                                     /// <summary>Second.</summary>
                                     int _a;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix removes a comment that is alone on its line together with the line, and a comment after code
    /// together with the whitespace in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3201-005")]
    public async Task FixRemovesTheCommentLineOrTheWhitespaceInFrontOfTheComment()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH3201://|}
                                    int _a;
                                    {|RH3201:/* */|}
                                    int _b; {|RH3201://|}
                                    int M(int a, int b) => a + {|RH3201:/* */|} b;
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int _a;
                                     int _b;
                                     int M(int a, int b) => a + b;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Characterizes that an empty comment next to a line that holds only a <c>/* */</c> comment is reported and
    /// removed, as is an empty <c>/* */</c> line between two <c>//</c> lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyCommentsNextToBlockCommentLinesAreReportedAndRemoved()
    {
        const string testCode = """
                                class C
                                {
                                    /* First. */
                                    {|RH3201://|}
                                    // Second.
                                    int _a;

                                    // First.
                                    {|RH3201:/* */|}
                                    // Second.
                                    int _b;
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     /* First. */
                                     // Second.
                                     int _a;

                                     // First.
                                     // Second.
                                     int _b;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Characterizes that the fix of an empty comment at the start of a line in front of code removes the indentation
    /// of the line with the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfCommentInFrontOfCodeRemovesTheIndentation()
    {
        const string testCode = """
                                class C
                                {
                                    /**/ int _a;
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                     int _a;
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix of a whitespace-only block comment that spans several lines leaves an empty line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfBlockCommentSpanningSeveralLinesLeavesAnEmptyLine()
    {
        const string testCode = """
                                class C
                                {
                                    /*

                                    */
                                    int _a;
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {

                                        int _a;
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix of a comment directly between a keyword and an identifier joins the two tokens
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfCommentBetweenKeywordAndIdentifierJoinsTheTokens()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return/**/a;
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        int M(int a)
                                        {
                                            returna;
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that the fix of a comment between a space and an identifier removes the space and joins the
    /// tokens
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfCommentDirectlyInFrontOfAnIdentifierJoinsTheTokens()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        return /**/a;
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        int M(int a)
                                        {
                                            returna;
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that an empty <c>//</c> line above a <c>///</c> comment inside a method body is reported and
    /// removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyLineAboveTripleSlashCommentInMethodBodyIsReportedAndRemoved()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        // First.
                                        {|RH3201://|}
                                        /// Second.
                                        M();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M()
                                     {
                                         // First.
                                         /// Second.
                                         M();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Characterizes that an empty <c>//</c> comment after code between two comment lines is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TrailingEmptyCommentBetweenCommentLinesIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    // First.
                                    int _a; //
                                    // Second.
                                    int _b;
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0://|}
                                        {|#1:/**/|}
                                        var value = 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        // The fix edits the document text rather than the syntax tree, and the two comments occupy consecutive
        // lines, so their removal spans meet
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text.", 2));
    }

    #endregion // BatchCodeFixTestsBase
}