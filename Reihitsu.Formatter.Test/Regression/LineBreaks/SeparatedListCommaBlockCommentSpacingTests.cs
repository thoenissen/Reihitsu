using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// The whitespace between a comma and a following block comment is kept as written when the list is split one element per line;
/// only whitespace that would end the comma's line is removed
/// </summary>
[TestClass]
public class SeparatedListCommaBlockCommentSpacingTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the space between a comma and a following block comment is kept when a wrapped parameter list is split
    /// </summary>
    [TestMethod]
    public void CommaFollowedByBlockCommentKeepsSpaceWhenParameterListIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a, /* c */ int b,
                                                 int c)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a, /* c */
                                                    int b,
                                                    int c)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment before the comma keeps its spacing when a wrapped parameter list is split
    /// </summary>
    [TestMethod]
    public void BlockCommentBeforeCommaKeepsSpacingWhenParameterListIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a /* c */, int b,
                                                 int c)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a /* c */,
                                                    int b,
                                                    int c)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment that already ends its line keeps the space after the comma
    /// </summary>
    [TestMethod]
    public void CommaFollowedByLineEndingBlockCommentKeepsSpace()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a, /* c */
                                                 int b,
                                                 int c)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the space between a comma and a following block comment is kept when a wrapped argument list is split
    /// </summary>
    [TestMethod]
    public void CommaFollowedByBlockCommentKeepsSpaceWhenArgumentListIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a, int b, int c)
                                 {
                                 }

                                 internal void B()
                                 {
                                     A(1, /* c */ 2,
                                       3);
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a, int b, int c)
                                    {
                                    }

                                    internal void B()
                                    {
                                        A(1, /* c */
                                          2,
                                          3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space between a comma and a following block comment is kept when a wrapped parameter list with the comment on a later element is split
    /// </summary>
    [TestMethod]
    public void CommaFollowedByBlockCommentOnLaterElementKeepsSpaceWhenParameterListIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a,
                                                 int b, /* c */ int c)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a,
                                                    int b, /* c */
                                                    int c)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space between a comma and a following block comment is kept when a wrapped attribute argument list is split
    /// </summary>
    [TestMethod]
    public void CommaFollowedByBlockCommentKeepsSpaceWhenAttributeArgumentListIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             [System.AttributeUsage(System.AttributeTargets.All)]
                             internal sealed class MarkAttribute : System.Attribute
                             {
                                 public MarkAttribute(int a, int b, int c)
                                 {
                                 }
                             }

                             [Mark(1, /* c */ 2,
                                   3)]
                             internal class Example;
                             """;
        const string expected = """
                                namespace Demo;

                                [System.AttributeUsage(System.AttributeTargets.All)]
                                internal sealed class MarkAttribute : System.Attribute
                                {
                                    public MarkAttribute(int a, int b, int c)
                                    {
                                    }
                                }

                                [Mark(1, /* c */
                                      2,
                                      3)]
                                internal class Example;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space between a comma and a following block comment is kept in every nested argument list that is split
    /// </summary>
    [TestMethod]
    public void CommaFollowedByBlockCommentKeepsSpaceWhenNestedArgumentListsAreSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal int A(int a, int b, int c)
                                 {
                                     return a;
                                 }

                                 internal void B()
                                 {
                                     A(1, /* c */ A(2, /* d */ 3,
                                                  4),
                                       5);
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal int A(int a, int b, int c)
                                    {
                                        return a;
                                    }

                                    internal void B()
                                    {
                                        A(1, /* c */
                                          A(2, /* d */
                                            3,
                                            4),
                                          5);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space between a leading comma and a following block comment is kept when the comma moves to the previous parameter line
    /// </summary>
    [TestMethod]
    public void LeadingCommaFollowedByBlockCommentKeepsSpaceWhenMovedToPreviousParameterLine()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a
                                                 , /* c */ int b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a, /* c */
                                                    int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma followed only by whitespace leaves no trailing whitespace when it moves to the previous parameter line
    /// </summary>
    [TestMethod]
    public void LeadingCommaFollowedByWhitespaceOnlyDropsWhitespaceWhenMovedToPreviousParameterLine()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a
                                                 ,   int b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a,
                                                    int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the spaces before and between two block comments after a comma are kept when a wrapped parameter list is split
    /// </summary>
    [TestMethod]
    public void CommaFollowedByTwoBlockCommentsKeepsBothSpacesWhenParameterListIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a, /* c1 */ /* c2 */ int b,
                                                 int c)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a, /* c1 */ /* c2 */
                                                    int b,
                                                    int c)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space between a comma and a following multi-line block comment is kept when a wrapped parameter list is split
    /// </summary>
    [TestMethod]
    public void CommaFollowedByMultiLineBlockCommentKeepsSpaceWhenParameterListIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a, /* c
                                                    d */ int b,
                                                 int c)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a, /* c
                                                       d */
                                                    int b,
                                                    int c)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space between a comma and a block comment glued to the next parameter is kept when a wrapped parameter list is split
    /// </summary>
    [TestMethod]
    public void CommaFollowedByBlockCommentGluedToNextParameterKeepsSpaceWhenParameterListIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a, /* c */int b,
                                                 int c)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a, /* c */
                                                    int b,
                                                    int c)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the author's whitespace between a comma and a following block comment is kept as written when a wrapped parameter list is split
    /// </summary>
    [TestMethod]
    public void CommaFollowedByTwoSpacesAndBlockCommentKeepsBothSpacesWhenParameterListIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a,  /* c */ int b,
                                                 int c)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a,  /* c */
                                                    int b,
                                                    int c)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no space is added between a comma and a block comment glued to it when a wrapped parameter list is split
    /// </summary>
    [TestMethod]
    public void CommaGluedToBlockCommentStaysGluedWhenParameterListIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a,/* c */ int b,
                                                 int c)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a,/* c */
                                                    int b,
                                                    int c)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace after a comma without a comment leaves no trailing whitespace when a wrapped parameter list is split
    /// </summary>
    [TestMethod]
    public void CommaFollowedByWhitespaceOnlyLeavesNoTrailingWhitespaceWhenParameterListIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a,   int b,
                                                 int c)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a,
                                                    int b,
                                                    int c)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace after the last block comment following a comma is removed when a wrapped parameter list is split
    /// </summary>
    [TestMethod]
    public void CommaFollowedByBlockCommentAndWhitespaceLeavesNoTrailingWhitespaceWhenParameterListIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a, /* c */    int b,
                                                 int c)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a, /* c */
                                                    int b,
                                                    int c)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an already split parameter list keeps the author's whitespace between a comma and a following block comment
    /// </summary>
    [TestMethod]
    public void SplitParameterListWithTwoSpacesBeforeBlockCommentIsStable()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a,  /* c */
                                                 int b,
                                                 int c)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the space between a comma and a following block comment is kept when a wrapped parameter list with a directive in a later gap is split
    /// </summary>
    [TestMethod]
    public void CommaFollowedByBlockCommentKeepsSpaceWhenParameterListWithDirectiveIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a, /* c */ int b,
                             #if DEBUG
                                                 int c,
                             #endif
                                                 int d)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a, /* c */
                                                    int b,
                                #if DEBUG
                                                    int c,
                                #endif
                                                    int d)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space between a comma and a following block comment is kept when a wrapped argument list with disabled text in a later gap is split
    /// </summary>
    [TestMethod]
    public void CommaFollowedByBlockCommentKeepsSpaceWhenArgumentListWithDisabledTextIsSplit()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void A(int a, int b, int c)
                                 {
                                 }

                                 internal void B()
                                 {
                                     A(1, /* c */ 2,
                             #if false
                                       4,
                             #endif
                                       3);
                                 }
                             }
                             """;
        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void A(int a, int b, int c)
                                    {
                                    }

                                    internal void B()
                                    {
                                        A(1, /* c */
                                          2,
                                #if false
                                          4,
                                #endif
                                          3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}