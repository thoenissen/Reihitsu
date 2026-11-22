using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.SwitchCaseBraces;

/// <summary>
/// Tests that the braces the formatter inserts around switch sections start the lines of a section's block, and that the
/// blank lines around and inside the new blocks are those the blank-line rules give any block, in one run
/// </summary>
[TestClass]
public class SwitchBraceLayoutOwnerTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the FMT-SWITCH-015 example: the inserted braces start the lines of a section's block and the terminal
    /// <c>break</c> starts the line below the closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-015")]
    public void InsertedBracesStartTheLinesOfASectionBlock()
    {
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
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment trailing the last label and a comment trailing the last statement keep their lines, with
    /// the inserted braces on the lines below them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-015")]
    public void InsertedBracesStartTheirLinesBehindTrailingComments()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: // label
                                             A();
                                             B(); // last
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
                                            case 1: // label
                                                {
                                                    A();
                                                    B(); // last
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the FMT-SWITCH-016 example: the blank line behind the label is removed as behind any opening brace, the run
    /// of blank lines in front of the comment that leads the terminal <c>break</c> collapses to one, and the sections are
    /// separated by a blank line, in one run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-016")]
    public void BlankLinesAroundInsertedBlocksAreThoseOfAnyBlock()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:

                                             // a
                                             A();
                                             B();


                                             // b
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
                                                    // a
                                                    A();
                                                    B();
                                                }

                                                // b
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

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line in front of a documentation comment, a delimited documentation comment, or a comment that
    /// leads the first statement on its line is removed behind the inserted opening brace, as behind any opening brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-016")]
    public void BlankLineInFrontOfFirstStatementIsRemovedBehindInsertedBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:

                                             /// d
                                             A();
                                             B();
                                             break;
                                         case 2:

                                             /** e */
                                             A();
                                             B();
                                             break;
                                         case 3:

                                             /* c */ A();
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
                                                {
                                                    /// d
                                                    A();
                                                    B();
                                                }
                                                break;

                                            case 2:
                                                {
                                                    /** e */
                                                    A();
                                                    B();
                                                }
                                                break;

                                            case 3:
                                                {
                                                    /* c */ A();
                                                    B();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the second FMT-SWITCH-016 example: without a comment in front of the first statement or the terminal
    /// <c>break</c>, the blank lines the author wrote there are not kept, in one run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-016")]
    public void BlankLinesWithoutCommentAroundWrappedStatementsAreNotKept()
    {
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

        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}