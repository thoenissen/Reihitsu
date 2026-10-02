using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// A block comment that follows a comma keeps the single space after that comma when the list is split one element per line
/// </summary>
[TestClass]
public class ParameterListCommaBlockCommentSpacingTests : FormatterTestsBase
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

    #endregion // Methods
}