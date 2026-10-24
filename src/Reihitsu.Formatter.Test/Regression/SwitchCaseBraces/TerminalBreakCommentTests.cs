using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.SwitchCaseBraces;

/// <summary>
/// Full-pipeline tests for comments that lead the terminal <c>break</c> of a section the formatter
/// braces: the comment stays in front of the <c>break</c> and gets its blank line in the pass that
/// inserts the braces
/// </summary>
[TestClass]
public class TerminalBreakCommentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the reported shape: the comment in front of the terminal <c>break</c> stays behind the
    /// new block with a blank line above it in the first pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-PIPE-004")]
    public void CommentBeforeTerminalBreakGetsBlankLineInThePassThatInsertsTheBraces()
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
                                             B();
                                             // before break
                                             break;

                                         case 2:
                                             C();
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
                                                {
                                                    A();
                                                    B();
                                                }

                                                // before break
                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between two comments in front of the terminal <c>break</c> is kept
    /// when the section is braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void BlankLineBetweenCommentsBeforeTerminalBreakIsKept()
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
                                             B();
                                             // one

                                             // two
                                             break;

                                         case 2:
                                             C();
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
                                                {
                                                    A();
                                                    B();
                                                }

                                                // one

                                                // two
                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment in front of the terminal <c>break</c> gets its blank line in
    /// the pass that inserts the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-PIPE-004")]
    public void DelimitedCommentBeforeTerminalBreakGetsBlankLineInThePassThatInsertsTheBraces()
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
                                             B();
                                             /* before break */
                                             break;

                                         case 2:
                                             C();
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
                                                {
                                                    A();
                                                    B();
                                                }

                                                /* before break */
                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a one-line section braced only because another section is multi-line keeps the
    /// comment in front of its terminal <c>break</c> with a blank line above it, under several labels
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-PIPE-004")]
    public void CommentBeforeTerminalBreakOfSectionBracedByAnotherSectionGetsBlankLine()
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
                                             B();
                                             break;

                                         case 2:
                                         case 3:
                                             C();
                                             // before break
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
                                                {
                                                    A();
                                                    B();
                                                }
                                                break;

                                            case 2:
                                            case 3:
                                                {
                                                    C();
                                                }

                                                // before break
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment trailing the last statement stays on its line inside the block while an
    /// own-line comment in front of the terminal <c>break</c> gets its blank line behind the block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-PIPE-004")]
    public void TrailingCommentAndOwnLineCommentBeforeTerminalBreakAreBothKept()
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
                                             B(); // trailing
                                             // own line
                                             break;

                                         case 2:
                                             C();
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
                                                {
                                                    A();
                                                    B(); // trailing
                                                }

                                                // own line
                                                break;

                                            case 2:
                                                {
                                                    C();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment in front of the terminal <c>break</c> of a switch that stays unbraced
    /// gets its blank line, and the result is stable
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void CommentBeforeTerminalBreakInUnbracedSwitchGetsBlankLine()
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
                                             // before break
                                             break;

                                         case 2:
                                             C();
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

                                                // before break
                                                break;

                                            case 2:
                                                C();
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment that is the first content of a section holding only <c>break</c> gets no
    /// blank line, and the switch stays unbraced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void CommentAsFirstContentOfBreakOnlySectionGetsNoBlankLine()
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
                                             // nothing to do
                                             break;

                                         case 2:
                                             C();
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an already braced section with a comment in front of the terminal <c>break</c> and
    /// a blank line above it is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    public void AlreadyBracedSectionWithCommentBeforeTerminalBreakIsUnchanged()
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
                                             {
                                                 A();
                                                 B();
                                             }

                                             // one

                                             // two
                                             break;

                                         case 2:
                                             {
                                                 C();
                                             }
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Tests
}