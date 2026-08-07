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
                                 [System.Obsolete /** why */]
                                 public void Method()
                                 {
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an ordinary comment before a closing brace is handled exactly as before. The exemption is
    /// scoped to documentation comments, which are the ones the compiler rejects in a position that documents
    /// nothing, so an ordinary comment in the same slot must be unaffected by it
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

        AssertRuleResult(input);
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