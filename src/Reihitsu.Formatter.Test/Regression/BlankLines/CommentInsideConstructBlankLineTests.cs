using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Characterization tests for the blank line the formatter currently inserts before a comment that starts its own
/// line inside a declaration header or an argument list, rather than between two statements or members. The
/// maintainer has decided to narrow that rule to statements and members, so these tests pin today's output and
/// name no requirement
/// </summary>
[TestClass]
public class CommentInsideConstructBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a block comment that starts the line of a constraint clause currently gets a blank line above it
    /// </summary>
    [TestMethod]
    public void BlankLineIsInsertedBeforeCommentLeadingConstraintClause()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void N<T>()
                                     /* c */ where T : class
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void N<T>()

                                        /* c */ where T : class
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment that starts the line of an argument currently gets a blank line above it
    /// </summary>
    [TestMethod]
    public void BlankLineIsInsertedBeforeBlockCommentLeadingArgument()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a, int b)
                                 {
                                     M(1,
                                       /* two */ 2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a, int b)
                                    {
                                        M(1,

                                          /* two */ 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment on its own line between two arguments currently gets a blank line above it
    /// </summary>
    [TestMethod]
    public void BlankLineIsInsertedBeforeLineCommentBetweenArguments()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a, int b)
                                 {
                                     M(1,
                                       // two
                                       2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a, int b)
                                    {
                                        M(1,

                                          // two
                                          2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}