using System;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.UsingDirectives;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.UsingDirectives;

/// <summary>
/// Tests that <see cref="UsingDirectiveOrderingPhase"/>, run on its own, keeps one blank line above the first comment that
/// leads a directive of the same group where a blank line stood above it in the source, writes none where none stood, and
/// leaves its own output unchanged, under LF and CRLF line endings
/// </summary>
[TestClass]
public class UsingDirectivePhaseCommentBlankLineTests
{
    #region Fields

    /// <summary>
    /// The line endings every fixture is exercised against
    /// </summary>
    private static readonly string[] _lineEndings = ["\n", "\r\n"];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies that the blank line above a single-line comment that leads a directive of the same group stays while the
    /// block is reordered around it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveCommentStaysWhenTheBlockIsReorderedAroundIt()
    {
        const string input = """
                             using Microsoft.CodeAnalysis;
                             using System;

                             // About Linq
                             using System.Linq;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;

                                // About Linq
                                using System.Linq;

                                using Microsoft.CodeAnalysis;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line above a single-line comment stays when its directive moves
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveCommentStaysWhenItsDirectiveMoves()
    {
        const string input = """
                             using Microsoft.CodeAnalysis;

                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;

                                // About Linq
                                using System.Linq;

                                using Microsoft.CodeAnalysis;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line above an ordinary comment on the directive's line stays through a reorder
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveCommentOnTheDirectivesLineStaysThroughAReorder()
    {
        const string input = """
                             using Microsoft.CodeAnalysis;
                             using System;

                             /* c */ using System.Linq;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;

                                /* c */ using System.Linq;

                                using Microsoft.CodeAnalysis;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertResult(input, expected);
    }

    /// <summary>
    /// Verifies that several blank lines above a comment that leads a directive of the same group become one
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void SeveralBlankLinesAboveCommentBecomeOne()
    {
        const string input = """
                             using System.Linq;
                             using System;



                             // About Text
                             using System.Text;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Linq;

                                // About Text
                                using System.Text;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertResult(input, expected);
    }

    /// <summary>
    /// Verifies that several blank lines above a comment become one also when the order does not change
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void SeveralBlankLinesAboveCommentBecomeOneWithoutReorder()
    {
        const string input = """
                             using System;


                             // About Linq
                             using System.Linq;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line that ends a file header counts for the comment that moves away from the top with its
    /// directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void HeaderBlankLineCountsForTheCommentThatMovesAwayFromTheTop()
    {
        const string input = """
                             // Header

                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // Header

                                using System;

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment without a blank line above it in the source gets none, also when its directive moves into
    /// the middle of its group
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void CommentWithoutBlankLineAboveGetsNoneWhenItsDirectiveMovesIntoTheGroup()
    {
        const string input = """
                             using System;
                             using Microsoft.CodeAnalysis;
                             // About Linq
                             using System.Linq;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;
                                // About Linq
                                using System.Linq;

                                using Microsoft.CodeAnalysis;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment without a blank line above it in an ordered block gets none
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void CommentWithoutBlankLineAboveInAnOrderedBlockGetsNone()
    {
        AssertUnchanged("""
                        using System;
                        // About Linq
                        using System.Linq;

                        class C
                        {
                            int x;
                        }
                        """);
    }

    /// <summary>
    /// Verifies that the blank line above a single-line comment in an ordered block is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveSingleLineCommentInAnOrderedBlockIsKept()
    {
        AssertUnchanged("""
                        using System;

                        // About Linq
                        using System.Linq;

                        class C
                        {
                            int x;
                        }
                        """);
    }

    /// <summary>
    /// Verifies that the blank line above a delimited comment on its own line in an ordered block is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveDelimitedCommentInAnOrderedBlockIsKept()
    {
        AssertUnchanged("""
                        using System;

                        /* About Linq */
                        using System.Linq;

                        class C
                        {
                            int x;
                        }
                        """);
    }

    /// <summary>
    /// Verifies that the blank line above a single-line documentation comment in an ordered block is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveSingleLineDocumentationCommentInAnOrderedBlockIsKept()
    {
        AssertUnchanged("""
                        using System;

                        /// About Linq
                        using System.Linq;

                        class C
                        {
                            int x;
                        }
                        """);
    }

    /// <summary>
    /// Verifies that the blank line above a delimited documentation comment in an ordered block is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveDelimitedDocumentationCommentInAnOrderedBlockIsKept()
    {
        AssertUnchanged("""
                        using System;

                        /** About Text */
                        using System.Text;

                        class C
                        {
                            int x;
                        }
                        """);
    }

    /// <summary>
    /// Verifies that the blank line above an ordinary comment on the directive's line in an ordered block is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveCommentOnTheDirectivesLineInAnOrderedBlockIsKept()
    {
        AssertUnchanged("""
                        using System;

                        /* c */ using System.Linq;

                        class C
                        {
                            int x;
                        }
                        """);
    }

    /// <summary>
    /// Verifies that the blank line above a delimited documentation comment that spans lines and ends on the directive's
    /// line is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveMultiLineDocumentationCommentOnTheDirectivesLineIsKept()
    {
        AssertUnchanged("""
                        using System;

                        /** first
                           second */ using System.Linq;

                        class C
                        {
                            int x;
                        }
                        """);
    }

    /// <summary>
    /// Verifies that the blank lines above and below a comment that leads a directive of the same group are kept, since
    /// the trivia from the comment to the directive is kept as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLinesAboveAndBelowTheCommentAreKept()
    {
        AssertUnchanged("""
                        using System;

                        // About Linq

                        using System.Linq;

                        class C
                        {
                            int x;
                        }
                        """);
    }

    /// <summary>
    /// Verifies that a blank line between two comments that lead a directive is kept, with and without a blank line above
    /// the first comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineBetweenTwoCommentsIsKept()
    {
        AssertUnchanged("""
                        using System;
                        // A

                        // B
                        using System.Linq;

                        class C
                        {
                            int x;
                        }
                        """);
        AssertUnchanged("""
                        using System;

                        // A

                        // B
                        using System.Linq;

                        class C
                        {
                            int x;
                        }
                        """);
    }

    /// <summary>
    /// Verifies that the blank line above a comment is kept behind a predecessor that ends with a trailing comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveCommentBehindATrailingCommentIsKept()
    {
        AssertUnchanged("""
                        using System; // trailing

                        // About Linq
                        using System.Linq;

                        class C
                        {
                            int x;
                        }
                        """);
    }

    /// <summary>
    /// Verifies that the blank line above a comment is kept, and kept through a reorder, inside a block-scoped namespace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveCommentIsKeptInsideABlockScopedNamespace()
    {
        const string input = """
                             namespace N
                             {
                                 using Microsoft.CodeAnalysis;
                                 using System;

                                 // About Linq
                                 using System.Linq;

                                 class C
                                 {
                                     int x;
                                 }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    using System;

                                    // About Linq
                                    using System.Linq;

                                    using Microsoft.CodeAnalysis;

                                    class C
                                    {
                                        int x;
                                    }
                                }
                                """;

        AssertResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line above a comment is kept, and kept through a reorder, inside a file-scoped namespace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveCommentIsKeptInsideAFileScopedNamespace()
    {
        const string input = """
                             namespace N;

                             using Microsoft.CodeAnalysis;
                             using System;

                             // About Linq
                             using System.Linq;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                namespace N;

                                using System;

                                // About Linq
                                using System.Linq;

                                using Microsoft.CodeAnalysis;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line above a directive of the same group without a leading comment is still removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-002")]
    public void BlankLineAboveDirectiveWithoutCommentIsRemoved()
    {
        const string input = """
                             using System;

                             using System.Linq;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line ending the header counts for the comment that moves away from the top behind a trailing
    /// comment that spans lines, also when a whitespace-only line stands between the header's blank line and the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void HeaderBlankLineAndWhitespaceOnlyLineBecomeOneBlankLineBehindATrailingCommentSpanningLines()
    {
        const string input = """
                             // Header

                             <ws>
                             // About Linq
                             using System.Linq;
                             using System.Collections; /* a
                                b */using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // Header

                                using System;
                                using System.Collections; /* a
                                   b */

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertResult(input.Replace("<ws>", "    "), expected);
    }

    /// <summary>
    /// Verifies that the blank line above a comment that leads a directive of the same group is kept when the directive in
    /// front of it after the reorder ends with a trailing comment that spans lines and is followed by no line break
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveCommentIsKeptBehindATrailingCommentSpanningLines()
    {
        const string input = """
                             using System.Collections; /* a
                                b */using System;

                             // About Text
                             using System.Text;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections; /* a
                                   b */

                                // About Text
                                using System.Text;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertResult(input, expected);
    }

    /// <summary>
    /// Verifies that a directive of the same group without a leading comment gets no line break behind a trailing comment
    /// that spans lines, because the line break inside that comment already separates the two
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-005")]
    public void DirectiveWithoutCommentGetsNoLineBreakBehindATrailingCommentSpanningLines()
    {
        const string input = """
                             using System.Collections; /* a
                                b */using System;
                             using System.Linq;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections; /* a
                                   b */using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertResult(input, expected);
    }

    /// <summary>
    /// Runs the phase under every line ending, asserts its output, and asserts that the phase run again on its own output
    /// changes nothing
    /// </summary>
    /// <param name="inputWithLf">The input with LF line endings</param>
    /// <param name="expectedWithLf">The expected output with LF line endings</param>
    private static void AssertResult(string inputWithLf, string expectedWithLf)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var expected = Normalize(expectedWithLf, endOfLine);
            var actual = Apply(Normalize(inputWithLf, endOfLine), endOfLine);

            Assert.AreEqual(expected, actual, $"Unexpected phase output under {Describe(endOfLine)} line endings.");
            Assert.AreEqual(expected, Apply(actual, endOfLine), $"The phase changed its own output under {Describe(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Runs the phase under every line ending and asserts that it changes nothing
    /// </summary>
    /// <param name="inputWithLf">The input with LF line endings</param>
    private static void AssertUnchanged(string inputWithLf)
    {
        AssertResult(inputWithLf, inputWithLf);
    }

    /// <summary>
    /// Runs the using-directive phase over the given source
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="endOfLine">The document's line ending</param>
    /// <returns>The phase output</returns>
    private static string Apply(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(input);
        var context = new FormattingContext(endOfLine);

        return new UsingDirectiveOrderingPhase().Execute(tree.GetRoot(), context, CancellationToken.None).ToFullString();
    }

    /// <summary>
    /// Rewrites every line break of the text to the given line ending
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="endOfLine">The line ending</param>
    /// <returns>The normalized text</returns>
    private static string Normalize(string text, string endOfLine)
    {
        var lineFeedOnly = text.Replace("\r\n", "\n");

        return endOfLine == "\n" ? lineFeedOnly : lineFeedOnly.Replace("\n", endOfLine);
    }

    /// <summary>
    /// Names the given line ending
    /// </summary>
    /// <param name="endOfLine">The line ending</param>
    /// <returns><c>LF</c> or <c>CRLF</c></returns>
    private static string Describe(string endOfLine)
    {
        return endOfLine == "\n" ? "LF" : "CRLF";
    }

    #endregion // Methods
}