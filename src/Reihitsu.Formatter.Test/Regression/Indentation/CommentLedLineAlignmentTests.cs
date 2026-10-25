using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for alignments whose anchor stands on a line that a comment leads: the anchor is measured behind the comment, where
/// the indentation phase writes it, through the whole pipeline over two passes
/// </summary>
[TestClass]
public class CommentLedLineAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: a delimited comment leads the line of the call, and the wrapped argument aligns to the first
    /// argument behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public void ArgumentAlignsToFirstArgumentBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     /* c */ Call(1,
                             2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        /* c */ Call(1,
                                                     2);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the spec example: a line comment on the line above leaves the call at the start of its line,
    /// and the wrapped argument aligns to the first argument there
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public void ArgumentAlignsToFirstArgumentBelowLineCommentOnTheLineAbove()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     // c
                                     Call(1,
                             2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        // c
                                        Call(1,
                                             2);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment that leads the line of the call is counted like a delimited comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public void ArgumentAlignsToFirstArgumentBehindDelimitedDocumentationCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     /** d */ Call(1,
                             2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        /** d */ Call(1,
                                                      2);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two delimited comments that lead the line of the call are both counted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public void ArgumentAlignsToFirstArgumentBehindTwoCommentsThatLeadTheLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     /* c */ /* d */ Call(1,
                             2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        /* c */ /* d */ Call(1,
                                                             2);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the last line of a multi-line comment in front of the call is counted, and the wrapped argument aligns to
    /// the first argument where that line puts it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public void ArgumentAlignsToFirstArgumentBehindTheLastLineOfAMultiLineComment()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     /* a
                                     b */ Call(1,
                             2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        /* a
                                        b */ Call(1,
                                                  2);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec sentence: behind a delimited comment that leads the line, a wrapped chain link aligns to the first
    /// link behind the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public void ChainLinkAlignsToFirstLinkBehindCommentThatLeadsTheLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     /* c */ a.B()
                             .C();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        /* c */ a.B()
                                                 .C();
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the qualifier: a delimited comment followed by a line break is not in front of the chain's
    /// first token, so the chain starts its line and the wrapped link aligns there
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-030")]
    public void ChainLinkAlignsToFirstLinkBelowDelimitedCommentThatEndsItsLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     /* c */
                                     a.B()
                             .C();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        /* c */
                                        a.B()
                                         .C();
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}