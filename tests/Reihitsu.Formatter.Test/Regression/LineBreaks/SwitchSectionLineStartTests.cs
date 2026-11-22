using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the lines that a switch section, each of its labels, and its first statement start on, and for
/// the blank line in front of a section that stands on the line of the preceding section's <c>break</c>
/// </summary>
[TestClass]
public class SwitchSectionLineStartTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that sections on one line behind a <c>break</c> each start their own line, that their first statements
    /// start the line behind their labels, and that the blank line in front of the second section appears in the first run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-035")]
    [Requirement("FMT-BLANK-004")]
    public void SectionsOnOneLineBehindBreakGetTheirLinesAndBlankLineInOneRun()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x) { case 1: A(); break; case 2: B(); break; }
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

                                            case 2:
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
    /// Verifies that sections on one line behind a <c>return</c> each start their own line without a blank line between
    /// them, and that their first statements start the line behind their labels
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-035")]
    public void SectionsOnOneLineBehindReturnStartTheirOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x) { case 1: return 1; case 2: return 2; }

                                     return 0;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                return 1;
                                            case 2:
                                                return 2;
                                        }

                                        return 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the first example of FMT-BREAK-033: sections behind <c>return</c>, <c>goto case</c> with a trailing
    /// comment, and <c>throw</c> start their own lines, and the comment stays on the line of the statement it trails
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    public void SectionsBehindReturnGotoCaseAndThrowStartTheirOwnLines()
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
                                             return 1; case 2:
                                             goto case 1; /* c */ case 3:
                                             throw null;
                                         default:
                                             return 0;
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
                                        {
                                            case 1:
                                                return 1;
                                            case 2:
                                                goto case 1; /* c */
                                            case 3:
                                                throw null;
                                            default:
                                                return 0;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the second example of FMT-BREAK-033: a section behind the last line of a multi-line comment is moved onto
    /// its own line, and a section behind a single-line comment already starts its line and is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("SHARED-COMMENT-012")]
    public void SectionBehindMultiLineCommentTailMovesAndSectionBehindLineCommentStays()
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
                                             return 1; /* a
                                             b */ case 2:
                                             return 2; // c
                                         case 3:
                                             return 3;
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
                                        {
                                            case 1:
                                                return 1; /* a
                                                b */
                                            case 2:
                                                return 2; // c
                                            case 3:
                                                return 3;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the third example of FMT-BREAK-033: a delimited documentation comment in front of a section behind a
    /// statement is moved onto a line of its own together with the section's label
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    public void DelimitedDocumentationCommentInFrontOfSectionStaysInFrontOfTheLabel()
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
                                             return 1; /** d */ case 2:
                                             return 2;
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
                                        {
                                            case 1:
                                                return 1;

                                            /** d */ case 2:
                                                return 2;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that sections behind <c>continue</c> and <c>goto default</c> start their own lines without a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-035")]
    public void SectionsBehindContinueAndGotoDefaultStartTheirOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[] xs)
                                 {
                                     foreach (var x in xs)
                                     {
                                         switch (x) { case 1: continue; case 2: goto default; default: A(); break; }
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[] xs)
                                    {
                                        foreach (var x in xs)
                                        {
                                            switch (x)
                                            {
                                                case 1:
                                                    continue;
                                                case 2:
                                                    goto default;
                                                default:
                                                    A();
                                                    break;
                                            }
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a section on the line of a <c>break</c> behind a delimited comment gets its blank line in the first
    /// run and the comment stays on the line of the <c>break</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BLANK-004")]
    public void SectionBehindBreakAndDelimitedCommentGetsBlankLineInOneRun()
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
                                             break; /* c */ case 2:
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
                                                break; /* c */

                                            case 2:
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
    /// Verifies that a section behind the last line of a multi-line comment that trails a <c>break</c> gets its blank line
    /// in the first run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BLANK-004")]
    [Requirement("SHARED-COMMENT-012")]
    public void SectionBehindBreakAndMultiLineCommentTailGetsBlankLineInOneRun()
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
                                             break; /* a
                                             b */ case 2:
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
                                                break; /* a
                                                b */

                                            case 2:
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
    /// Verifies the new example of FMT-BLANK-004: a section on the line of the <c>break</c> statement gets the blank line
    /// in the same run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    public void SectionOnLineOfBreakGetsBlankLineInSameRun()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     switch (a)
                                     {
                                         case 1:
                                             a++;
                                             break; case 2:
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
                                                a++;
                                                break;

                                            case 2:
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a section behind a <c>break</c> that ends its line with a single-line comment keeps its line and gets
    /// the blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BLANK-004")]
    public void SectionBehindBreakWithLineCommentKeepsItsLineAndGetsBlankLine()
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
                                             break; // c
                                         case 2:
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
                                                break; // c

                                            case 2:
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
    /// Verifies that a section behind a directive in active code already starts its line and is kept, while the first
    /// statements behind the labels of both sections start their own lines; a section in the disabled branch is disabled
    /// text and stays as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-035")]
    public void SectionBehindDirectiveKeepsItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: return 1;
                             #if !DEBUG
                                         case 2: return 2;
                             #else
                                         case 3: return 3;
                             #endif
                                     }

                                     return 0;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                return 1;
                                #if !DEBUG
                                            case 2:
                                                return 2;
                                #else
                                            case 3: return 3;
                                #endif
                                        }

                                        return 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of FMT-BREAK-034: labels that share a line are stacked, a comment that trailed a label stays on
    /// that label's line, and a delimited documentation comment in front of a label moves with the label
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-034")]
    public void LabelsSharingALineAreStacked()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: case 2:
                                             A();
                                             break;
                                         case 3: /* c */ default:
                                             B();
                                             break;
                                         case 4: /** f */ case 5:
                                             B();
                                             break;
                                     }
                                 }

                                 void A()
                                 {
                                 }

                                 void B()
                                 {
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
                                            case 2:
                                                A();
                                                break;

                                            case 3: /* c */
                                            default:
                                                B();
                                                break;

                                            case 4:
                                            /** f */ case 5:
                                                B();
                                                break;
                                        }
                                    }

                                    void A()
                                    {
                                    }

                                    void B()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>default</c> label followed by a <c>case</c> label on its line is stacked, and that the first
    /// statement moves behind the labels
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-034")]
    [Requirement("FMT-BREAK-035")]
    public void DefaultLabelFollowedByCaseLabelIsStacked()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         default: case 1: A();
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
                                            default:
                                            case 1:
                                                A();
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a label behind the last line of a multi-line comment that trails the label before it is moved onto
    /// its own line, and the comment stays on the line of the label it trails
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-034")]
    [Requirement("SHARED-COMMENT-012")]
    public void LabelBehindMultiLineCommentTailIsMoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 3: /* a
                                         b */ case 4:
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
                                            case 3: /* a
                                            b */
                                            case 4:
                                                A();
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that labels behind a single-line comment already start their lines and are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-034")]
    public void LabelBehindLineCommentKeepsItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: // one
                                         case 2:
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
    /// Verifies the main example of FMT-BREAK-035: a first statement behind its label, a first statement behind a delimited
    /// documentation comment, a <c>break</c> as first statement, and a first statement behind a pattern label with a guard
    /// and a trailing comment each start their own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-035")]
    public void FirstStatementsStartTheirOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x, object o)
                                 {
                                     switch (x)
                                     {
                                         case 1: A(); break;
                                         case 2: /** d */ A();
                                             break;
                                         default: break;
                                     }

                                     switch (o)
                                     {
                                         case int i when i > 0: /* c */ A();
                                             break;
                                     }
                                 }

                                 void A()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x, object o)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                A();
                                                break;

                                            case 2:
                                                /** d */ A();
                                                break;

                                            default:
                                                break;
                                        }

                                        switch (o)
                                        {
                                            case int i when i > 0: /* c */
                                                A();
                                                break;
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the third visible output change of the change doc: a first statement behind a comment, a <c>break</c> as
    /// first statement, and a first statement behind a pattern label with a guard move behind their labels
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-035")]
    public void FirstStatementsMoveBehindPlainAndGuardedLabels()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x, object o)
                                 {
                                     switch (x)
                                     {
                                         case 1: /* c */ A();
                                             break;

                                         default: break;
                                     }

                                     switch (o)
                                     {
                                         case int i when i > 0: A();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x, object o)
                                    {
                                        switch (x)
                                        {
                                            case 1: /* c */
                                                A();
                                                break;

                                            default:
                                                break;
                                        }

                                        switch (o)
                                        {
                                            case int i when i > 0:
                                                A();
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the sixth visible output change of the change doc: a delimited documentation comment in front of a first
    /// statement and in front of a later label moves with it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-034")]
    [Requirement("FMT-BREAK-035")]
    public void DelimitedDocumentationCommentMovesWithFirstStatementAndLabel()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: /** d */ A();
                                             break;

                                         case 4: /** f */ case 5:
                                             return 5;
                                     }

                                     return 0;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                /** d */ A();
                                                break;

                                            case 4:
                                            /** f */ case 5:
                                                return 5;
                                        }

                                        return 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the target shapes of a delimited documentation comment in front of a first statement and a later label
    /// are kept as they are
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-034")]
    [Requirement("FMT-BREAK-035")]
    public void DelimitedDocumentationCommentStartingItsLineIsKept()
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
                                             /** d */ A();
                                             break;

                                         case 4:
                                         /** f */ case 5:
                                             return 5;
                                     }

                                     return 0;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a first statement behind a pattern label whose guard stands on its own line moves behind the guard
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-035")]
    public void FirstStatementBehindGuardOnItsOwnLineStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object o)
                                 {
                                     switch (o)
                                     {
                                         case int n // keep wrapped
                                             when n > 0: A();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o)
                                    {
                                        switch (o)
                                        {
                                            case int n // keep wrapped
                                                when n > 0:
                                                A();
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a local declaration and a labeled statement as first statements start their own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-035")]
    public void LocalDeclarationAndLabeledStatementAsFirstStatementStartTheirOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: var y = x + 1;
                                             break;

                                         case 2: L: A();
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
                                                var y = x + 1;
                                                break;

                                            case 2:
                                                L: A();
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block as first statement on its label's line starts its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-035")]
    public void BlockAsFirstStatementStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: { A(); } break;
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
    /// Verifies that a section behind a section whose body is a block ending in <c>break</c> starts its own line without a
    /// blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BLANK-004")]
    public void SectionBehindBlockBodyStartsItsOwnLineWithoutBlankLine()
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

                                                 break;
                                             } case 2:
                                             {
                                                 B();
                                             }
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

                                                    break;
                                                }
                                            case 2:
                                                {
                                                    B();
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
    /// Verifies that an empty statement behind stacked labels stays on the last label's line while the labels are stacked
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-034")]
    [Requirement("FMT-BREAK-035")]
    public void EmptyFirstStatementBehindStackedLabelsStaysOnTheLabelsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 2: case 3: ; break;
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
                                            case 2:
                                            case 3:; break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a section with several statements on the line of the next section, braced by the switch-brace phase,
    /// gets exactly one blank line in front of the next section in the first run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-035")]
    [Requirement("FMT-BLANK-004")]
    public void BracedSectionEndingInBreakGetsOneBlankLineInOneRun()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x) { case 1: A(); B(); break; case 2: C(); break; }
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a section with several statements ending in <c>return</c> on the line of the next section, braced by
    /// the switch-brace phase, is followed by the next section without a blank line, and that no blank line stands in
    /// front of the switch's closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-035")]
    [Requirement("FMT-BLANK-004")]
    public void BracedSectionEndingInReturnGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x) { case 1: A(); B(); return 1; case 2: return 2; }

                                     return 0;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();

                                                    return 1;
                                                }
                                            case 2:
                                                {
                                                    return 2;
                                                }
                                        }

                                        return 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that braced sections with stacked labels on the line of the switch's closing brace get no blank line in
    /// front of that brace in the first run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-034")]
    [Requirement("FMT-BLANK-004")]
    public void BracedSectionsOnLineOfClosingBraceGetNoBlankLineBeforeIt()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x) { case 1: case 2: A(); B(); break; case 3: C(); break; }
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
                                            case 2:
                                                {
                                                    A();
                                                    B();
                                                }
                                                break;

                                            case 3:
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
    /// Verifies that a switch nested in a section of a switch, both on one line, gets every section and first statement on
    /// its own line, and that the section behind the nested switch starts its own line without a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-035")]
    public void NestedSwitchSectionsStartTheirOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x, int y)
                                 {
                                     switch (x)
                                     {
                                         case 1: switch (y) { case 1: return 1; default: return 2; } case 2: return 0;
                                     }

                                     return 3;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x, int y)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    switch (y)
                                                    {
                                                        case 1:
                                                            return 1;
                                                        default:
                                                            return 2;
                                                    }
                                                }
                                            case 2:
                                                {
                                                    return 0;
                                                }
                                        }

                                        return 3;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the sections of a switch inside a lambda start their own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-035")]
    public void SwitchSectionsInsideLambdaStartTheirOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     System.Action a = () =>
                                                       {
                                                           switch (x) { case 1: return; case 2: return; }
                                                       };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        System.Action a = () =>
                                                          {
                                                              switch (x)
                                                              {
                                                                  case 1:
                                                                      return;
                                                                  case 2:
                                                                      return;
                                                              }
                                                          };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the sections of a switch inside a local function start their own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-035")]
    public void SwitchSectionsInsideLocalFunctionStartTheirOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int L(int y)
                                     {
                                         switch (y) { case 1: return 1; default: return 0; }
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int L(int y)
                                        {
                                            switch (y)
                                            {
                                                case 1:
                                                    return 1;
                                                default:
                                                    return 0;
                                            }
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a switch whose sections, labels, and first statements already start their own lines is kept as it is
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-034")]
    [Requirement("FMT-BREAK-035")]
    public void SwitchWhoseSectionsAlreadyStartTheirLinesIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x, object o)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                         case 2:
                                             return 1;
                                         case 3: // three
                                             A();
                                             break;

                                         default:
                                             break;
                                     }

                                     switch (o)
                                     {
                                         case int i when i > 0:
                                             return i;
                                     }

                                     return 0;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that two delimited documentation comments in front of a label move with it together, and that an ordinary
    /// comment in front of the first delimited documentation comment stays on the previous label's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-034")]
    public void CommentRunFromFirstDelimitedDocumentationCommentMovesWithLabel()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: /* c */ /** d */ case 2:
                                             A();
                                             break;

                                         case 3: /** a */ /** b */ case 4:
                                             A();
                                             break;
                                     }
                                 }

                                 void A()
                                 {
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
                                            case 1: /* c */
                                            /** d */ case 2:
                                                A();
                                                break;

                                            case 3:
                                            /** a */ /** b */ case 4:
                                                A();
                                                break;
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the comments from the first delimited documentation comment on the label's line up to the first
    /// statement move with the statement as written and in order, that a delimited documentation comment spanning lines
    /// moves as a whole, and that an ordinary comment in front of the first delimited documentation comment stays on the
    /// label's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-035")]
    public void CommentRunFromFirstDelimitedDocumentationCommentMovesWithFirstStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: /* c */ /** d */ A();
                                             break;

                                         case 2: /** d */ /* c */ A();
                                             break;

                                         case 3: /** a
                                             b */ A();
                                             break;
                                     }
                                 }

                                 void A()
                                 {
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
                                            case 1: /* c */
                                                /** d */ A();
                                                break;

                                            case 2:
                                                /** d */ /* c */ A();
                                                break;

                                            case 3:
                                                /** a
                                                b */ A();
                                                break;
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an ordinary comment in front of a delimited documentation comment stays on the label's line while
    /// the documentation comment and the ordinary comment behind it move with the first statement, next to a label and a
    /// first statement that each move with one delimited documentation comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-034")]
    [Requirement("FMT-BREAK-035")]
    public void OrdinaryCommentStaysOnLabelLineWhileCommentsFromDocumentationCommentMove()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: /** d */ A();
                                             break;

                                         case 4: /** f */ case 5:
                                             return 5;

                                         case 6: /* c */ /** d */ /* e */ A();
                                             break;
                                     }

                                     return 0;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                /** d */ A();
                                                break;

                                            case 4:
                                            /** f */ case 5:
                                                return 5;

                                            case 6: /* c */
                                                /** d */ /* e */ A();
                                                break;
                                        }

                                        return 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment behind the <c>break</c> of a section the switch-brace phase braces stays on the
    /// <c>break</c>'s line, and that the next section starts its own line behind exactly one blank line in the first run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BLANK-004")]
    public void BracedSectionEndingInBreakKeepsTrailingCommentAndGetsOneBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x) { case 1: A(); B(); break; /* c */ case 2: C(); break; }
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
                                                break; /* c */

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
    /// Verifies that a multi-line comment behind the <c>break</c> of a section the switch-brace phase braces stays behind
    /// the <c>break</c>, and that the section behind the comment's last line starts its own line behind exactly one blank
    /// line in the first run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BLANK-004")]
    public void BracedSectionEndingInBreakKeepsTrailingMultiLineCommentAndGetsOneBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: A(); B(); break; /* a
                                         b */ case 2: C(); break;
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
                                                break; /* a
                                            b */

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
    /// Verifies that a comment behind the <c>return</c> of a section the switch-brace phase braces stays on the
    /// <c>return</c>'s line, and that the next section starts its own line without a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BLANK-004")]
    public void BracedSectionEndingInReturnKeepsTrailingCommentAndGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x) { case 1: A(); B(); return 1; /* c */ case 2: return 2; }

                                     return 0;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();

                                                    return 1; /* c */
                                                }
                                            case 2:
                                                {
                                                    return 2;
                                                }
                                        }

                                        return 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line comment behind the <c>return</c> of a section the switch-brace phase braces stays behind
    /// the <c>return</c>, and that the section behind the comment's last line starts its own line without a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BLANK-004")]
    public void BracedSectionEndingInReturnKeepsTrailingMultiLineCommentAndGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: A(); B(); return 1; /* a
                                         b */ case 2: return 2;
                                     }

                                     return 0;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    B();

                                                    return 1; /* a
                                            b */
                                                }
                                            case 2:
                                                {
                                                    return 2;
                                                }
                                        }

                                        return 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment behind the <c>break</c> of a section the switch-brace phase braces stays on the
    /// <c>break</c>'s line in front of the switch's closing brace, with no blank line in front of that brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    public void BracedSectionEndingInBreakKeepsTrailingCommentInFrontOfClosingBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x) { case 1: C(); break; case 2: A(); B(); break; /* c */ }
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
                                                    C();
                                                }
                                                break;

                                            case 2:
                                                {
                                                    A();
                                                    B();
                                                }
                                                break; /* c */
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line comment behind the <c>break</c> of a section the switch-brace phase braces stays behind
    /// the <c>break</c> in front of the switch's closing brace, and that the brace starts its own line with no blank line
    /// in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    public void BracedSectionEndingInBreakKeepsTrailingMultiLineCommentInFrontOfClosingBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: C(); break;
                                         case 2: A(); B(); break; /* a
                                         b */ }
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
                                                    C();
                                                }
                                                break;

                                            case 2:
                                                {
                                                    A();
                                                    B();
                                                }
                                                break; /* a
                                            b */
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}