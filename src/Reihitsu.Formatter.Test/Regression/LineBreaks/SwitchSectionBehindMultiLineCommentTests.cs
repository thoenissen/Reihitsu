using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for a switch section or a case label that stands behind the closing line of a comment that spans
/// several lines: it starts the line below the comment when the comment starts behind the token in front of it, and stays
/// behind the comment when the comment starts on a line of its own
/// </summary>
[TestClass]
public class SwitchSectionBehindMultiLineCommentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the multi-line comment example of FMT-BREAK-033: sections and labels move behind a comment that starts
    /// behind the opening brace, a statement, or a label, and stay behind a comment that starts on a line of its own
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-034")]
    [Requirement("SHARED-COMMENT-012")]
    public void SectionsAndLabelsBehindCommentTailsFollowTheExampleOfOneSectionPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     { /* a
                                       */ case 1:
                                             return 1;
                                         /* b
                                         */ case 2:
                                             return 2; /* c
                                             */ case 3: /* d
                                         */ case 4:
                                         /* e
                                         */ case 5:
                                             return 5;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        { /* a
                                          */
                                            case 1:
                                                return 1;
                                            /* b
                                            */ case 2:
                                                return 2; /* c
                                                */
                                            case 3: /* d
                                            */
                                            case 4:
                                            /* e
                                            */ case 5:
                                                return 5;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a section behind the closing line of a comment that starts on a line of its own below a
    /// <c>return</c> stays behind the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("SHARED-COMMENT-012")]
    public void SectionBehindTailOfCommentOnItsOwnLineAfterReturnStaysBehindTheComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             return 1;
                                         /* b
                                         */ case 2:
                                             return 2;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a section behind the closing line of a comment that starts on a line of its own below a
    /// <c>break</c> stays behind the comment, and the blank line in front of the section goes above the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BLANK-004")]
    [Requirement("SHARED-COMMENT-012")]
    public void SectionBehindTailOfCommentOnItsOwnLineAfterBreakStaysAndGetsTheBlankLineAboveTheComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             break;
                                         /* z
                                         */ case 2:
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                A();
                                                break;

                                            /* z
                                            */ case 2:
                                                B();
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a label behind the closing line of a comment that starts on a line of its own below the previous
    /// label stays behind the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-034")]
    [Requirement("SHARED-COMMENT-012")]
    public void LabelBehindTailOfCommentOnItsOwnLineStaysBehindTheComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                         /* l
                                         */ case 2:
                                             A();
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the first section behind the closing line of a comment that starts on a line of its own below the
    /// opening brace stays behind the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("SHARED-COMMENT-012")]
    public void FirstSectionBehindTailOfCommentOnItsOwnLineStaysBehindTheComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         /* f
                                         */ case 1:
                                             return 1;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the first section behind the closing line of a comment that starts behind the opening brace starts
    /// the line below the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("SHARED-COMMENT-012")]
    public void FirstSectionBehindTailOfCommentBehindOpeningBraceStartsTheLineBelow()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     { /* f
                                       */ case 1:
                                             return 1;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        { /* f
                                          */
                                            case 1:
                                                return 1;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line documentation comment behind a label starts a line of its own, and the label behind
    /// its closing line stays there
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-034")]
    [Requirement("SHARED-COMMENT-012")]
    public void LabelBehindTailOfDocumentationCommentBehindLabelStaysBehindTheComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: /** l
                                         */ case 2:
                                             A();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                            /** l
                                            */ case 2:
                                                A();
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}