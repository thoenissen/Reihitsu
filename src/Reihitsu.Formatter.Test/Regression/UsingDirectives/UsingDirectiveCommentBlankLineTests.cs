using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.UsingDirectives;

/// <summary>
/// Full-pipeline tests for the blank line above a comment that leads a using directive of the same group: the formatter
/// writes it as for any comment, keeps an author's blank line above a comment on the directive's line, and no phase, run
/// alone on the output, removes it again
/// </summary>
[TestClass]
public class UsingDirectiveCommentBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the blank line above a single-line comment that leads a directive of the same group is kept, and that
    /// no phase run alone on the output changes it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveSingleLineCommentIsKept()
    {
        const string input = """
                             using System;

                             // About Linq
                             using System.Linq;

                             class C
                             {
                             }
                             """;
        const string expected = """
                                using System;

                                // About Linq
                                using System.Linq;

                                class C;
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line above a delimited documentation comment that leads a directive of the same group is
    /// kept, and that no phase run alone on the output changes it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveDelimitedDocumentationCommentIsKept()
    {
        const string input = """
                             using System;

                             /** About Text */
                             using System.Text;

                             class C
                             {
                             }
                             """;
        const string expected = """
                                using System;

                                /** About Text */
                                using System.Text;

                                class C;
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment that leads a directive of the same group gets exactly one blank line above it, with or
    /// without a blank line in the input, a single-line comment and a documentation comment alike
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void CommentLeadingDirectiveOfTheSameGroupGetsOneBlankLine()
    {
        const string input = """
                             using System;
                             // About Linq
                             using System.Linq;

                             /** About Text */
                             using System.Text;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;

                                // About Linq
                                using System.Linq;

                                /** About Text */
                                using System.Text;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several blank lines above a comment that leads a directive of the same group become one
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void SeveralBlankLinesAboveCommentLeadingDirectiveBecomeOne()
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

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line above a single-line documentation comment that leads a directive of the same group is
    /// written and kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void SingleLineDocumentationCommentLeadingDirectiveGetsOneBlankLine()
    {
        const string input = """
                             using System;
                             /// About Linq
                             using System.Linq;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;

                                /// About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line the author wrote above an ordinary comment on the directive's line is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveOrdinaryCommentOnTheDirectivesLineIsKept()
    {
        const string input = """
                             using System;

                             /* c */ using System.Linq;

                             class C
                             {
                             }
                             """;
        const string expected = """
                                using System;

                                /* c */ using System.Linq;

                                class C;
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an ordinary comment on the directive's line without a blank line above it gets none
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void OrdinaryCommentOnTheDirectivesLineWithoutBlankLineGetsNone()
    {
        const string input = """
                             using System;
                             /* c */ using System.Linq;

                             class C
                             {
                             }
                             """;
        const string expected = """
                                using System;
                                /* c */ using System.Linq;

                                class C;
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank lines above and below a comment that leads a directive of the same group are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLinesAboveAndBelowTheCommentAreKept()
    {
        AssertRuleResult("""
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
    /// Verifies that two comment paragraphs that lead a directive of the same group keep the blank line between them and
    /// get one above the first
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void TwoCommentParagraphsLeadingDirectiveGetOneBlankLineAboveTheFirst()
    {
        const string input = """
                             using System;
                             // A

                             // B
                             using System.Linq;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;

                                // A

                                // B
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment that leads a directive of the same group behind a predecessor with a trailing comment gets
    /// one blank line above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void CommentBehindPredecessorWithTrailingCommentGetsOneBlankLine()
    {
        const string input = """
                             using System; // trailing
                             // About Linq
                             using System.Linq;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System; // trailing

                                // About Linq
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line above a comment that leads a directive of the same group is kept through a reorder
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveCommentIsKeptThroughAReorder()
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

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line above a comment that leads a directive of the same group is kept inside a block-scoped
    /// namespace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveCommentIsKeptInsideABlockScopedNamespace()
    {
        AssertRuleResult("""
                         namespace N
                         {
                             using System;

                             // About Linq
                             using System.Linq;

                             class C
                             {
                                 int x;
                             }
                         }
                         """);
    }

    /// <summary>
    /// Verifies that the blank line above a comment that leads a directive of the same group is kept inside a file-scoped
    /// namespace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void BlankLineAboveCommentIsKeptInsideAFileScopedNamespace()
    {
        AssertRuleResult("""
                         namespace N;

                         using System;

                         // About Linq
                         using System.Linq;

                         class C
                         {
                             int x;
                         }
                         """);
    }

    #endregion // Methods
}