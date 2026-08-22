using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline regression tests for documentation comments that begin after code on the same line
/// </summary>
[TestClass]
public class TrailingDocumentationCommentPreservationTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a trailing summary is moved intact above the following member
    /// </summary>
    [TestMethod]
    public void MovesTrailingSummaryAboveFollowingMember()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } /// <summary>Trailing summary.</summary>

                                 public int Other { get; set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; }

                                    /// <summary>
                                    /// Trailing summary.
                                    /// </summary>
                                    public int Other { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that documentation on the first token is separated safely from an ordinary banner comment
    /// </summary>
    [TestMethod]
    public void MovesFirstTokenDocumentationBelowPrecedingBannerComment()
    {
        const string input = """
                             /* banner */ /// <summary>Trailing summary.</summary>
                             internal class TestClass
                             {
                                 public int Value { get; set; }
                             }
                             """;
        const string expected = """
                                /* banner */
                                /// <summary>
                                /// Trailing summary.
                                /// </summary>
                                internal class TestClass
                                {
                                    public int Value { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading block-comment sibling is separated from following documentation inside a containing root
    /// </summary>
    [TestMethod]
    public void MovesDocumentationBelowLeadingBlockCommentSibling()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; }
                                 /* banner */ /// <summary>Other value.</summary>
                                 public int Other { get; set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; }

                                    /* banner */
                                    /// <summary>
                                    /// Other value.
                                    /// </summary>
                                    public int Other { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line block-comment sibling is separated from following documentation inside a containing root
    /// </summary>
    [TestMethod]
    public void MovesDocumentationBelowLeadingMultilineBlockCommentSibling()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; }
                                 /* first banner line
                                  * second banner line */ /// <summary>Other value.</summary>
                                 public int Other { get; set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; }

                                    /* first banner line
                                     * second banner line */
                                    /// <summary>
                                    /// Other value.
                                    /// </summary>
                                    public int Other { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that ordinary full-file cleanup still removes a leading line before existing documentation
    /// </summary>
    [TestMethod]
    public void RemovesLeadingLineBeforeExistingDocumentationInFullTree()
    {
        const string input = """

                             /// <summary>Documented type.</summary>
                             internal class TestClass
                             {
                                 public int Value { get; set; }
                             }
                             """;
        const string expected = """
                                /// <summary>
                                /// Documented type.
                                /// </summary>
                                internal class TestClass
                                {
                                    public int Value { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every line of a trailing multi-line element is moved above the following member
    /// </summary>
    [TestMethod]
    public void MovesTrailingMultilineElementAboveFollowingMember()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } /// <remarks>First line.
                                 /// Second line.</remarks>

                                 public int Other { get; set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; }

                                    /// <remarks>
                                    /// First line.
                                    /// Second line.
                                    /// </remarks>
                                    public int Other { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that trailing plain documentation text is moved above the following member
    /// </summary>
    [TestMethod]
    public void MovesTrailingPlainDocumentationTextAboveFollowingMember()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } /// trailing note

                                 public int Other { get; set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; }

                                    /// trailing note
                                    public int Other { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that relocation preserves nested XML content
    /// </summary>
    [TestMethod]
    public void MovesTrailingSummaryWithNestedXmlAboveFollowingMember()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } /// <summary>Uses <see cref="Other"/>.</summary>

                                 public int Other { get; set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; }

                                    /// <summary>
                                    /// Uses <see cref="Other"/>.
                                    /// </summary>
                                    public int Other { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that relocation preserves documentation already written above the following member
    /// </summary>
    [TestMethod]
    public void PreservesExistingDocumentationAboveFollowingMember()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } /// trailing note
                                 /// <summary>Other value.</summary>
                                 public int Other { get; set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; }

                                    /// trailing note
                                    /// <summary>
                                    /// Other value.
                                    /// </summary>
                                    public int Other { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that relocation remains inside the owning conditional-compilation branch
    /// </summary>
    [TestMethod]
    public void MovesTrailingDocumentationWithinDirectiveBranch()
    {
        const string input = """
                             internal class TestClass
                             {
                             #if DEBUG
                                 public int Value { get; set; } /// trailing note

                                 public int Other { get; set; }
                             #endif
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                #if DEBUG
                                    public int Value { get; set; }

                                    /// trailing note
                                    public int Other { get; set; }
                                #endif
                                }
                                """;
        var parseOptions = new CSharpParseOptions(preprocessorSymbols: ["DEBUG"]);

        AssertRuleResult(input, expected, parseOptions);
    }

    /// <summary>
    /// Verifies that documentation with no following declaration stays on the line the author wrote it on. It
    /// documents nothing - the closing brace does not open the type declaration - so relocating it would be a
    /// placement decision the formatter cannot make (issues #591, #625)
    /// </summary>
    [TestMethod]
    public void PreservesTrailingDocumentationWithoutFollowingMember()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } /// trailing note
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a line comment written after a field's semicolon is untouched. It is the counterpart of
    /// <see cref="PreservesDelimitedDocumentationAfterFieldSemicolon"/>: Roslyn files it as the semicolon's
    /// trailing trivia rather than the closing brace's leading trivia, so no phase ever considered relocating it
    /// </summary>
    [TestMethod]
    public void PreservesLineCommentAfterFieldSemicolon()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _first; // Comment
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block comment written after a field's semicolon is untouched
    /// </summary>
    [TestMethod]
    public void PreservesBlockCommentAfterFieldSemicolon()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _second; /* Comment */
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block comment written before a field's semicolon keeps the space in front of it. The
    /// spacing rules decide from the token pair alone, so without the comment-gap exemption they would collapse
    /// the gap between the declarator and the semicolon and delete that space
    /// </summary>
    [TestMethod]
    public void PreservesBlockCommentBeforeFieldSemicolon()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _fourth /* Comment */;
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment written after a field's semicolon stays on the field's line
    /// </summary>
    [TestMethod]
    public void PreservesDelimitedDocumentationAfterFieldSemicolon()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _third; /** Comment */
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment written after a field's semicolon, with another field
    /// following it, reaches its final layout in the first pass. The comment documents the following field, so
    /// relocating it above that field is correct, but the relocation and the blank-line separation must not be
    /// spread across two passes (issue #637)
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationAfterFieldSemicolonAboveFollowingMemberInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _x; /** doc */
                                 private int _y;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _x;

                                    /** doc */
                                    private int _y;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the same one-pass separation applies to a delimited documentation comment written after a
    /// property's closing brace. The owning token differs from the field case, so it proves the behavior follows
    /// from the trivia position rather than from the preceding member's kind (issue #637)
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationAfterPropertyAboveFollowingMemberInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } /** doc */
                                 public int Other { get; set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; }

                                    /** doc */
                                    public int Other { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the interior lines of a multi-line delimited documentation comment are carried along unchanged
    /// while the comment itself is separated in one pass. Only the boundary in front of the comment moves, so the
    /// text the author wrote inside it is not re-laid-out (issue #637)
    /// </summary>
    [TestMethod]
    public void MovesMultilineDelimitedDocumentationAboveFollowingMemberInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _x; /** first
                                                   * second */
                                 private int _y;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _x;

                                    /** first
                                                      * second */
                                    private int _y;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that documentation already written above the following member is kept below the relocated comment
    /// and that the whole layout is still reached in one pass (issue #637)
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationAboveAlreadyDocumentedFollowingMemberInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _x; /** doc */
                                 /// <summary>Other.</summary>
                                 private int _y;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _x;

                                    /** doc */
                                    /// <summary>
                                    /// Other.
                                    /// </summary>
                                    private int _y;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several delimited documentation comments converge together rather than one per pass. Each one
    /// owns its own boundary, so a fix that only settled the first would still need a pass per comment (issue #637)
    /// </summary>
    [TestMethod]
    public void MovesSeveralDelimitedDocumentationCommentsInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _a; /** first */
                                 private int _b; /** second */
                                 private int _c;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _a;

                                    /** first */
                                    private int _b;

                                    /** second */
                                    private int _c;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment the author already put on its own line receives exactly one
    /// line break. This is the other side of the boundary the one-pass separation turns on: the comment does not
    /// share the preceding token's line, so only the blank line is missing (issue #637)
    /// </summary>
    [TestMethod]
    public void SeparatesOwnLineDelimitedDocumentationWithASingleLineBreak()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _x;
                                 /** doc */
                                 private int _y;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _x;

                                    /** doc */
                                    private int _y;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment preceded by a multi-line block comment in the gap is left
    /// alone. The boundary is measured on rendered lines, so the block comment's own lines already satisfy it;
    /// counting end-of-line trivia instead would insert a blank line here and change a file that is stable today
    /// (issue #637)
    /// </summary>
    [TestMethod]
    public void PreservesDelimitedDocumentationAfterMultilineBlockCommentInGap()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _x; /* a
                                                    b */
                                 /** doc */
                                 private int _y;
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment preceded by a multi-line block comment that ends on the
    /// comment's own line receives a single line break and no blank line. This is the one shape on which the
    /// rendered-line measure the boundary uses and the range measure in
    /// <see cref="Reihitsu.Core.TokenGapAnalysis.RequiredLineBreakCountForBlankLine"/> disagree, so the output is
    /// pinned here: the blank line the rule asks for is missing, the file is still a fixed point, and the layout
    /// is the same one the formatter produced before the one-pass separation existed. Closing that gap means
    /// changing a stable file and belongs to its own issue (issue #637)
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationBelowMultilineBlockCommentEndingOnItsLine()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _x; /* a
                                                    b */ /** doc */
                                 private int _y;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _x; /* a
                                                       b */
                                    /** doc */
                                    private int _y;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line banner comment followed by delimited documentation is left alone. Splitting the
    /// pair is the single line documentation behavior, and reaching it from here would mean widening the relocation
    /// filter instead of settling the boundary (issue #637)
    /// </summary>
    [TestMethod]
    public void PreservesOwnLineBannerAndDelimitedDocumentationPair()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _x;

                                 /* banner */ /** doc */
                                 private int _y;
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single line documentation comment written after a field's semicolon, with another field
    /// following it, still reaches its final layout in one pass. It travels the relocation path in the
    /// documentation-comment phase rather than the boundary path, so it proves that path was left untouched
    /// (issue #637)
    /// </summary>
    [TestMethod]
    public void MovesSingleLineDocumentationAfterFieldSemicolonAboveFollowingMemberInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _x; /// note
                                 private int _y;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _x;

                                    /// note
                                    private int _y;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single line documentation comment written after a field's semicolon stays on the field's line
    /// </summary>
    [TestMethod]
    public void PreservesSingleLineDocumentationAfterFieldSemicolon()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _fifth; /// Comment
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment written before a single field's semicolon stays in place.
    /// No split is involved here, which is what makes the split field's output identical to the declaration the
    /// author would have written by hand (issue #625)
    /// </summary>
    [TestMethod]
    public void PreservesDelimitedDocumentationBeforeSingleFieldSemicolon()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _only /** note */;
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a documentation comment inside an attribute list stays where it is. The closing bracket does
    /// not open the attribute list, so the comment documents nothing and must not be relocated (issue #591)
    /// </summary>
    [TestMethod]
    public void PreservesDocumentationInsideAttributeList()
    {
        const string input = """
                             internal class TestClass
                             {
                                 [System.Obsolete /** why */ ]
                                 public void Method()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    [System.Obsolete /** why */]
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an ordinary comment before a closing brace is not relocated. The exemption from relocation
    /// is scoped to documentation comments, which are the ones the compiler rejects in a position that documents
    /// nothing, so an ordinary comment in the same slot must be unaffected by it — it only gains the blank line
    /// every trailing scope comment is preceded by (issue #694)
    /// </summary>
    [TestMethod]
    public void KeepsOrdinaryCommentHandlingAtBlockEnd()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; }
                                 // trailing note
                             }
                             """;

        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; }

                                    // trailing note
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an ordinary single-line comment remains behind the preceding member
    /// </summary>
    [TestMethod]
    public void KeepsOrdinarySingleLineCommentBehindPrecedingMember()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } // trailing note

                                 public int Other { get; set; }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an ordinary multi-line comment remains behind the preceding member
    /// </summary>
    [TestMethod]
    public void KeepsOrdinaryMultilineCommentBehindPrecedingMember()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } /* trailing note */

                                 public int Other { get; set; }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that relocation preserves an ordinary block comment that precedes the documentation exterior
    /// </summary>
    [TestMethod]
    public void PreservesOrdinaryBlockCommentBeforeTrailingDocumentation()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } /* value note */ /// trailing documentation

                                 public int Other { get; set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; } /* value note */

                                    /// trailing documentation
                                    public int Other { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several off-position documentation comments are relocated in one pass
    /// </summary>
    [TestMethod]
    public void MovesSeveralTrailingDocumentationCommentsInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public int First { get; set; } /// first note

                                 public int Second { get; set; }

                                 public int Third { get; set; } /// third note

                                 public int Fourth { get; set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int First { get; set; }

                                    /// first note
                                    public int Second { get; set; }

                                    public int Third { get; set; }

                                    /// third note
                                    public int Fourth { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every off-position documentation comment sharing one owner is relocated in the same pass
    /// </summary>
    [TestMethod]
    public void MovesDocumentationCommentsSharingOneOwnerInOnePass()
    {
        const string input = """
                             /* first */ /// first documentation
                             /* second */ /// second documentation
                             internal class TestClass
                             {
                                 public int Value { get; set; }
                             }
                             """;
        const string expected = """
                                /* first */
                                /// first documentation
                                /* second */
                                /// second documentation
                                internal class TestClass
                                {
                                    public int Value { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}