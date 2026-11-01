using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for the blank lines above the opening brace of a block statement: a block statement that follows
/// another statement of its block or switch section is separated by the statement rules, and every other block keeps its
/// brace tight to the code above it
/// </summary>
[TestClass]
public class BlockStatementBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a block statement directly after a statement that ends with a closing brace gets the blank line that
    /// follows a closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void InsertsBlankLineAboveBlockStatementAfterClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                     {
                                         A();
                                     }
                                     {
                                         B();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a)
                                        {
                                            A();
                                        }

                                        {
                                            B();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block statement directly after another block statement gets the blank line that follows a closing
    /// brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void InsertsBlankLineBetweenTwoBlockStatements()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     A();

                                     {
                                         B();
                                     }
                                     {
                                         C();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        A();

                                        {
                                            B();
                                        }

                                        {
                                            C();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block statement after a closing brace inside another block statement gets the blank line, one
    /// nesting level deeper
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void InsertsBlankLineAboveNestedBlockStatementAfterClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     {
                                         while (a)
                                         {
                                             A();
                                         }
                                         {
                                             B();
                                         }
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        {
                                            while (a)
                                            {
                                                A();
                                            }

                                            {
                                                B();
                                            }
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the author's blank line above a block statement after a statement that does not end with a closing
    /// brace is kept, and that none is inserted where the author wrote none
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void KeepsAuthorBlankLineAboveBlockStatementAfterOtherStatement()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     A();

                                     {
                                         B();
                                     }

                                     A();
                                     {
                                         B();
                                     }
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a block statement after a statement that does not end with a closing
    /// brace when the author wrote none
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void InsertsNoBlankLineAboveBlockStatementAfterOtherStatement()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     A();
                                     {
                                         B();
                                     }
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that several blank lines the author wrote above a block statement after another statement collapse to one
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void CollapsesAuthorBlankLinesAboveBlockStatementAfterOtherStatementToOne()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     A();



                                     {
                                         B();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        A();

                                        {
                                            B();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block statement after a <c>break</c> statement gets the blank line that follows a <c>break</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void InsertsBlankLineAboveBlockStatementAfterBreak()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     while (a > 0)
                                     {
                                         break;
                                         {
                                             a++;
                                         }
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (a > 0)
                                        {
                                            break;

                                            {
                                                a++;
                                            }
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that behind a directive the blank line after a closing brace goes below the directive, and that the
    /// author's blank line between a directive and a block statement after another statement is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void PlacesBlankLineBelowDirectiveAndKeepsAuthorBlankLineBelowDirective()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                     {
                                     }
                             #if DEBUG
                                     {
                                         B();
                                     }
                             #endif
                                     A();
                             #pragma warning disable CS0168

                                     {
                                         B();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a)
                                        {
                                        }
                                #if DEBUG

                                        {
                                            B();
                                        }
                                #endif

                                        A();
                                #pragma warning disable CS0168

                                        {
                                            B();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that inside a switch section a block statement after a closing brace gets the blank line, and the author's
    /// blank line above a block statement after another statement is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void SeparatesBlockStatementsThatFollowAStatementInsideSwitchSection()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     switch (a)
                                     {
                                         case 1:
                                             if (a > 0)
                                             {
                                                 a++;
                                             }
                                             {
                                                 a--;
                                             }
                                             a++;

                                             {
                                                 a--;
                                             }

                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                if (a > 0)
                                                {
                                                    a++;
                                                }

                                                {
                                                    a--;
                                                }

                                                a++;

                                                {
                                                    a--;
                                                }

                                                break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line above a block statement that is the first statement of a switch section, and above the
    /// body of an <c>if</c> statement, is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void RemovesBlankLineAboveFirstStatementOfSwitchSectionAndIfBody()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     switch (a)
                                     {
                                         case 1:

                                             {
                                                 a++;
                                             }
                                             break;
                                     }

                                     if (a > 0)

                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                {
                                                    a++;
                                                }
                                                break;
                                        }

                                        if (a > 0)
                                        {
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line above a block statement that is the first statement of a block is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void RemovesBlankLineAboveBlockStatementThatIsFirstStatementOfBlock()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {

                                     {
                                         a++;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        {
                                            a++;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line above the block of a labeled statement is removed, because that block is not in the
    /// statement list of a block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void RemovesBlankLineAboveBlockOfLabeledStatement()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     L:

                                     {
                                         a++;
                                     }

                                     goto L;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        L:
                                        {
                                            a++;
                                        }

                                        goto L;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line above a top-level block of a file with top-level statements is removed, because that
    /// block is not in the statement list of a block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void RemovesBlankLineAboveTopLevelBlock()
    {
        const string input = """
                             var a = 1;

                             {
                                 a++;
                             }
                             """;
        const string expected = """
                                var a = 1;
                                {
                                    a++;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line comment above a block statement after another statement gets the blank line above it, and
    /// the author's blank line between the comment and the brace is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    [Requirement("FMT-BLANK-020")]
    public void KeepsAuthorBlankLineBetweenCommentAndBlockStatementAfterOtherStatement()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     A();
                                     // c

                                     {
                                         B();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        A();

                                        // c

                                        {
                                            B();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line comment above a block statement after a closing brace gets the blank line that follows a
    /// closing brace, and the author's blank line between the comment and the brace is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void KeepsAuthorBlankLineBetweenCommentAndBlockStatementAfterClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                     {
                                     }
                                     // c

                                     {
                                         B();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a)
                                        {
                                        }

                                        // c

                                        {
                                            B();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line comment above a block statement after a <c>break</c> gets the blank line that follows a
    /// <c>break</c>, and the author's blank line between the comment and the brace is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void KeepsAuthorBlankLineBetweenCommentAndBlockStatementAfterBreak()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     while (a > 0)
                                     {
                                         break;
                                         // c

                                         {
                                             a++;
                                         }
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (a > 0)
                                        {
                                            break;

                                            // c

                                            {
                                                a++;
                                            }
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted between an own-line comment and a block statement after a closing brace when
    /// the author wrote none; the blank line that follows the closing brace goes above the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void InsertsNoBlankLineBetweenCommentAndBlockStatementAfterClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                     {
                                     }
                                     // c
                                     {
                                         B();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a)
                                        {
                                        }

                                        // c
                                        {
                                            B();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment glued to the brace of a block statement after a <c>break</c> gets the blank line that
    /// follows a <c>break</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void InsertsBlankLineAboveCommentGluedToBlockStatementAfterBreak()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     while (a > 0)
                                     {
                                         break;
                                         /* c */ {
                                             a++;
                                         }
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (a > 0)
                                        {
                                            break;

                                            /* c */ {
                                                a++;
                                            }
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}