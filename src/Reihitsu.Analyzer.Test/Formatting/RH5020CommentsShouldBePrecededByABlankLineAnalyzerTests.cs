using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5020CommentsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5020CommentsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5020CommentsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5020CommentsShouldBePrecededByABlankLineAnalyzer, RH5020CommentsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a single-line comment directly follows a statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-004")]
    [Requirement("RH5020-005")]
    public async Task VerifyDiagnosticForCommentWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    public void Execute()
                                    {
                                        var value = 0;
                                        {|#0:// Explain the value|}
                                        Consume(value);
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5020
                                 {
                                     public void Execute()
                                     {
                                         var value = 0;

                                         // Explain the value
                                         Consume(value);
                                     }

                                     private void Consume(int value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported only for the first comment in a comment block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-003")]
    [Requirement("RH5020-005")]
    public async Task VerifyDiagnosticForCommentBlockWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    public void Execute()
                                    {
                                        var value = 0;
                                        {|#0:// Explain the value|}
                                        // Continue the explanation
                                        Consume(value);
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5020
                                 {
                                     public void Execute()
                                     {
                                         var value = 0;

                                         // Explain the value
                                         // Continue the explanation
                                         Consume(value);
                                     }

                                     private void Consume(int value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a single-line comment already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("SHARED-BLANK-001")]
    public async Task VerifyNoDiagnosticForCommentWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    public void Execute()
                                    {
                                        var value = 0;

                                        // Explain the value
                                        Consume(value);
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for the first comment in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-002")]
    public async Task VerifyNoDiagnosticForFirstCommentInBlock()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    public void Execute()
                                    {
                                        // First comment in block
                                        Consume(0);
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for the first comment in a file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-002")]
    public async Task VerifyNoDiagnosticForFirstCommentInFile()
    {
        const string testCode = """
                                // File header comment
                                internal class RH5020
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a comment starts a switch section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-002")]
    public async Task VerifyNoDiagnosticForFirstCommentInSwitchSection()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 0:
                                                // First comment in switch section
                                                Consume(value);
                                                break;
                                        }
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a comment directly follows another comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-002")]
    [Requirement("RH5020-003")]
    public async Task VerifyNoDiagnosticForAdjacentCommentLines()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    public void Execute()
                                    {
                                        // First comment
                                        // Follow-up comment
                                        Consume(0);
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the inserted blank line matches the document's detected CRLF end-of-line sequence instead of
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-005")]
    public async Task VerifyInsertedBlankLineUsesDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                internal class RH5020
                                {
                                    public void Execute()
                                    {
                                        var value = 0;
                                        // Explain the value
                                        Consume(value);
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.DoesNotContain("\n", fixedSource.Replace("\r\n", string.Empty));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when the line immediately preceding the comment is a preprocessor
    /// directive, which acts as a transparent boundary rather than ordinary preceding content
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-003")]
    public async Task VerifyNoDiagnosticForCommentDirectlyAfterPreprocessorDirective()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    public void Execute()
                                    {
                                        var value = 0;
                                #if true
                                        value++;
                                #endif
                                        // Explain the value
                                        Consume(value);
                                    }

                                    private void Consume(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a diagnostic is still reported when the preceding source line only looks like a directive because
    /// it is the content of a verbatim string starting with <c>#</c>, confirming the directive detection is
    /// syntax-based rather than a plain text prefix check
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-005")]
    public async Task VerifyDiagnosticWhenPrecedingLineIsStringContentStartingWithHash()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    public void Execute()
                                    {
                                        var text = @"
                                #value";
                                        {|#0:// Explain the text|}
                                        Consume(text);
                                    }

                                    private void Consume(string value)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5020
                                 {
                                     public void Execute()
                                     {
                                         var text = @"
                                 #value";

                                         // Explain the text
                                         Consume(text);
                                     }

                                     private void Consume(string value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies that four-slash ordinary comments are covered by RH5020
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-005")]
    public async Task VerifyFourSlashCommentIsDetectedAndFixed()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    void Execute()
                                    {
                                        var value = 0;
                                        {|#0://// Ordinary comment|}
                                        value++;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5020
                                 {
                                     void Execute()
                                     {
                                         var value = 0;

                                         //// Ordinary comment
                                         value++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies that own-line block comments are covered while inline block comments remain outside the rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-005")]
    [Requirement("SHARED-BLANK-002")]
    public async Task VerifyOwnLineBlockCommentIsDetectedButInlineCommentIsExcluded()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    void Execute()
                                    {
                                        var value = 0; /* inline */
                                        {|#0:/* own line */|}
                                        value++;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5020
                                 {
                                     void Execute()
                                     {
                                         var value = 0; /* inline */

                                         /* own line */
                                         value++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies that a mixed consecutive ordinary-comment block is owned by its first comment only
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-003")]
    [Requirement("RH5020-005")]
    public async Task VerifyMixedConsecutiveCommentsProduceOneDiagnostic()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    void Execute()
                                    {
                                        var value = 0;
                                        {|#0://// first|}
                                        /* continuation */
                                        value++;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5020
                                 {
                                     void Execute()
                                     {
                                         var value = 0;

                                         //// first
                                         /* continuation */
                                         value++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies every kind of ordinary comment that starts its line is reported on its text when the line above is not
    /// blank, while a comment behind code, also behind the end of a multi-line comment, is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-004")]
    [Requirement("SHARED-BLANK-002")]
    public async Task VerifyOwnLineCommentsWithoutPrecedingBlankLineAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    int _a;
                                    {|RH5020:// line comment|}
                                    int _b;
                                    {|RH5020:/* block comment */|}
                                    int _c;
                                    {|RH5020://// four slashes|}
                                    int _d; // trailing
                                    {|RH5020:// after a trailing comment|}
                                    int _e;

                                    void M(int a)
                                    {
                                        var x = 1; /* a
                                        b */ // behind code
                                        a++;
                                        {|RH5020:// before the closing brace|}
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the first comment of a file, a type, a block, a case section, and a default section is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-002")]
    public async Task VerifyFirstCommentOfFileTypeBlockOrSwitchSectionIsNotReported()
    {
        const string testCode = """
                                // first in file
                                class C
                                {
                                    // first in type
                                    int _a;

                                    void M(int a)
                                    {
                                        // first in block
                                        a++;
                                        switch (a)
                                        {
                                            case 1:
                                                // first in section
                                                break;
                                            default:
                                                // first in default
                                                break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a comment is not reported when a comment or a directive that comes after the line break behind the
    /// preceding token stands directly in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-003")]
    [Requirement("SHARED-BLANK-002")]
    public async Task VerifyCommentFollowingCommentOrDirectiveAfterLineBreakIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    int _a;

                                    // first line
                                    // second line
                                    /* third line */
                                    int _b;
                                #pragma warning disable CS0169
                                    // after directive
                                    int _c;

                                    void M(int a)
                                    {
                                        a++;

                                        /* a
                                        b */ // c
                                        // d
                                        a++;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a comment is reported when the comments in front of it start on the line of the preceding token and
    /// end before the first line break outside a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-003")]
    [Requirement("SHARED-BLANK-002")]
    public async Task VerifyCommentFollowingCommentsOnPrecedingTokenLineIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++; /* a
                                        b */ // c
                                        {|RH5020:// d|}
                                        a++;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix inserts one empty line directly above each reported comment and changes nothing else
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-005")]
    public async Task VerifyFixInsertsEmptyLineAboveEachComment()
    {
        const string testCode = """
                                class C
                                {
                                    int _a;
                                    {|RH5020:// line comment|}
                                    int _b;
                                    {|RH5020:/* block comment */|}
                                    int _c; // trailing
                                    {|RH5020:// after a trailing comment|}
                                    int _d;
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int _a;

                                     // line comment
                                     int _b;

                                     /* block comment */
                                     int _c; // trailing

                                     // after a trailing comment
                                     int _d;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the inserted empty line uses the document's CRLF line ending and carries no indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-005")]
    public async Task VerifyFixInsertsCarriageReturnLineFeedEmptyLineAboveComment()
    {
        const string testCode = "class C\n{\n    int _a;\n    // note\n    int _b;\n}\n";
        const string fixedCode = "class C\n{\n    int _a;\n\n    // note\n    int _b;\n}\n";

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedCode), actual);
    }

    /// <summary>
    /// Verifies that a comment on its own line that leads a using directive of the same group is reported without a blank
    /// line above it, and that the fix inserts one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-005")]
    public async Task VerifyDiagnosticForCommentLeadingUsingDirectiveOfTheSameGroup()
    {
        const string testCode = """
                                using System;
                                {|#0:// About Linq|}
                                using System.Linq;

                                internal class RH5020
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 // About Linq
                                 using System.Linq;

                                 internal class RH5020
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies that an ordinary comment on the line of a using directive of the same group is reported without a blank
    /// line above it, and that the fix inserts one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-005")]
    public async Task VerifyDiagnosticForCommentOnTheLineOfUsingDirectiveOfTheSameGroup()
    {
        const string testCode = """
                                using System;
                                {|#0:/* c */|} using System.Linq;

                                internal class RH5020
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 /* c */ using System.Linq;

                                 internal class RH5020
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: the first comment behind the opening parenthesis of a record's parameter list,
    /// a method's parameter list, and an argument list, behind the opening bracket of a collection expression, and behind
    /// the <c>&lt;</c> of a type parameter list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-002")]
    public async Task SpecExampleFirstCommentBehindOpeningParenthesisBracketOrLessThanIsNotReported()
    {
        const string testCode = """
                                public record R(
                                    // first
                                    int A,
                                    int B);

                                class C
                                {
                                    int M(
                                        // first
                                        int a,
                                        int b)
                                    {
                                        return M(
                                            // first
                                            a,
                                            b);
                                    }

                                    int[] F =
                                    [
                                        // first
                                        1,
                                    ];

                                    void O<
                                        // first
                                        T>()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the specification example under CRLF line endings: the first comment behind an opening parenthesis,
    /// bracket, or <c>&lt;</c> is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-002")]
    public async Task SpecExampleFirstCommentBehindOpeningParenthesisBracketOrLessThanIsNotReportedWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                public record R(
                                    // first
                                    int A,
                                    int B);

                                class C
                                {
                                    int[] F =
                                    [
                                        // first
                                        1,
                                    ];

                                    void O<
                                        // first
                                        T>()
                                    {
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifies the specification example: a comment behind a comma of a parameter list is reported, and the fix inserts
    /// the blank line above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-002")]
    public async Task SpecExampleCommentBehindCommaOfParameterListIsReportedAndFixed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a,
                                           {|RH5020:// second|}
                                           int b)
                                    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a,

                                            // second
                                            int b)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies both sides of the boundary in one list: the first comment behind the opening parenthesis or bracket is not
    /// reported, a comment behind a comma of the same list is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-002")]
    public async Task FirstCommentBehindOpeningTokenIsNotReportedWhileCommentBehindCommaIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(
                                        // first
                                        int a,
                                        {|RH5020:// second|}
                                        int b)
                                    {
                                    }

                                    int[] F =
                                    [
                                        /* first */
                                        1,
                                        {|RH5020:/* second */|}
                                        2,
                                    ];
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(
                                         // first
                                         int a,

                                         // second
                                         int b)
                                     {
                                     }

                                     int[] F =
                                     [
                                         /* first */
                                         1,

                                         /* second */
                                         2,
                                     ];
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the first comment behind the opening token of an attribute list, an attribute argument list, an
    /// indexer's parameter list, a statement header, a cast, a parenthesized expression, a tuple, an array size, an element
    /// access, a positional, parenthesized, and list pattern, a lambda's parameter list, and a type argument list is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-002")]
    public async Task FirstCommentBehindEveryOtherOpeningTokenIsNotReported()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    [
                                        // attribute
                                        Obsolete]
                                    int this[
                                        // indexer
                                        int i] => i;

                                    [Obsolete(
                                        // attribute argument
                                        "x")]
                                    List<
                                        // return type
                                        int> M(object o, int a, (int, int) p, int[] l)
                                    {
                                        if (
                                            // header
                                            a > 0)
                                        {
                                            a = (
                                                // cast
                                                int)o;
                                        }

                                        a = (
                                            // parenthesized
                                            a + 1) * 2;
                                        var t = (
                                            /* tuple */
                                            1,
                                            2);
                                        var s = new int[
                                            // array size
                                            3];
                                        _ = s[
                                            // element access
                                            0];
                                        _ = p is (
                                            // positional
                                            1, 2);
                                        _ = a is (
                                            // parenthesized pattern
                                            > 0);
                                        _ = l is [
                                            // list pattern
                                            1,
                                            ..
                                        ];
                                        Func<int, int> f = (
                                            // lambda
                                            int x) => x;
                                        _ = N<
                                            // type argument
                                            int>();
                                        _ = t;
                                        _ = f;

                                        return null;
                                    }

                                    T N<T>() => default;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the first comment behind the opening parenthesis of a union's case-type list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-002")]
    public async Task FirstCommentInUnionCaseTypeListIsNotReported()
    {
        const string testCode = """
                                public union U(
                                    // first
                                    int,
                                    string);
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that the first comment behind the <c>&lt;</c> of a function pointer parameter list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-002")]
    public async Task FirstCommentInFunctionPointerParameterListIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    unsafe void P(delegate*<
                                        // function pointer
                                        int, void> f)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
    }

    /// <summary>
    /// Verifies that a comment behind a <c>&lt;</c> that is a binary operator or a relational pattern is reported and
    /// fixed, since such a <c>&lt;</c> opens nothing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-002")]
    public async Task CommentBehindLessThanOperatorIsReportedAndFixed()
    {
        const string testCode = """
                                class C
                                {
                                    bool N(int a, int b)
                                    {
                                        return a <
                                            {|RH5020:// compared|}
                                            b;
                                    }

                                    bool P(int a)
                                    {
                                        return a is <
                                            {|RH5020:// bound|}
                                            5;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     bool N(int a, int b)
                                     {
                                         return a <

                                             // compared
                                             b;
                                     }

                                     bool P(int a)
                                     {
                                         return a is <

                                             // bound
                                             5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a comment in an empty parameter list or collection expression, which the closing token owns, is not
    /// reported behind the opening token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-002")]
    public async Task CommentInEmptyListBehindOpeningTokenIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(
                                        // no parameters yet
                                        )
                                    {
                                        int[] a = [
                                            // no elements yet
                                        ];
                                        _ = a;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the first comment on its own line below an opening parenthesis whose line a trailing comment ends is
    /// not reported, since the token in front of it is the opening parenthesis
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-002")]
    [Requirement("RH5020-003")]
    public async Task FirstCommentBelowTrailingCommentOfOpeningParenthesisIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        M( // note
                                            // first
                                            a);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that under documentation mode <c>None</c>, where <c>///</c> is an ordinary comment, the first one behind an
    /// opening parenthesis is not reported and one behind a comma is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5020-001")]
    [Requirement("RH5020-002")]
    public async Task TripleSlashCommentWithoutDocumentationParsingFollowsTheOpeningTokenRule()
    {
        const string testCode = """
                                public record R(
                                    /// first
                                    int A,
                                    {|RH5020:/// second|}
                                    int B);
                                """;
        const string fixedCode = """
                                 public record R(
                                     /// first
                                     int A,

                                     /// second
                                     int B);
                                 """;

        await Verify(testCode, fixedCode, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5020
                                {
                                    void Execute()
                                    {
                                        var first = 1;
                                        {|#0://// first|}
                                        Consume(first);
                                        var second = 2;
                                        {|#1:/* second */|}
                                        Consume(second);
                                    }

                                    void Consume(int value)
                                    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5020
                                 {
                                     void Execute()
                                     {
                                         var first = 1;

                                         //// first
                                         Consume(first);
                                         var second = 2;

                                         /* second */
                                         Consume(second);
                                     }

                                     void Consume(int value)
                                     {
                                     }
                                 }
                                 """;

        // Verifies that Fix All converges for multiple newly covered ordinary-comment kinds
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}