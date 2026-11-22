using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Tests for the blank line above a comment that starts its own line inside a declaration header or an argument list,
/// rather than between two statements or members. A comment glued to the token behind it gets no blank line
/// (FMT-BLANK-018); the other cases are characterization tests that pin today's output and name no requirement, because
/// the maintainer has decided to narrow that rule to statements and members
/// </summary>
[TestClass]
public class CommentInsideConstructBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that no blank line is inserted above a block comment glued to a constraint clause
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void NoBlankLineIsInsertedBeforeCommentGluedToConstraintClause()
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
    /// Verifies that no blank line is inserted above a block comment glued to an argument
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void NoBlankLineIsInsertedBeforeBlockCommentGluedToArgument()
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