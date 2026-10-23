using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for the blank-line behavior no narrower document-level test pins: declaration and
/// <c>yield</c> separation, the switch-section rule, the blank line after a closing brace, the chained keywords,
/// the lines around documentation comments, and the text inside literals, comments, and disabled code
/// </summary>
[TestClass]
public class BlankLineRequirementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that an <c>unchecked</c> statement after another statement is preceded by a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-001")]
    public void UncheckedStatementIsPrecededByBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                                     unchecked
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
                                        a++;

                                        unchecked
                                        {
                                            a++;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a run of local declarations is separated from the statements around it, except for an
    /// assignment that directly follows a declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-002")]
    public void LocalDeclarationsAreSeparatedFromOtherStatementsExceptAFollowingAssignment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                                     var b = 1;
                                     var c = 2;
                                     a++;
                                     var d = 3;
                                     a = 4;
                                     var e = 5;
                                     M(e);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;

                                        var b = 1;
                                        var c = 2;

                                        a++;

                                        var d = 3;
                                        a = 4;

                                        var e = 5;

                                        M(e);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that consecutive <c>yield</c> statements stay together, while a <c>yield return</c> or
    /// <c>yield break</c> after another statement is preceded by a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-003")]
    public void YieldSequenceIsSeparatedFromOtherStatements()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 System.Collections.Generic.IEnumerable<int> M(int a)
                                 {
                                     a++;
                                     yield return 1;
                                     yield return 2;
                                     a++;
                                     yield break;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    System.Collections.Generic.IEnumerable<int> M(int a)
                                    {
                                        a++;

                                        yield return 1;
                                        yield return 2;
                                        a++;

                                        yield break;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that only a switch section after a section ending in <c>break</c> is preceded by a blank line, while
    /// a section after one ending in <c>return</c> keeps its compact form
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    public void OnlySectionsAfterABreakSectionAreSeparated()
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
                                             break;
                                         case 2:
                                             return;
                                         case 3:
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
                                                return;
                                            case 3:
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line after a closing brace is inserted behind a directive that stands between the
    /// brace and the next statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-005")]
    [Requirement("FMT-BLANK-016")]
    public void BlankLineAfterClosingBraceGoesBehindDirective()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     if (a > 0)
                                     {
                                     }
                             #pragma warning disable CS0168
                                     a++;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        if (a > 0)
                                        {
                                        }
                                #pragma warning disable CS0168

                                        a++;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a statement sharing its line with a preceding statement that ends with a closing brace gets the
    /// blank line as well once it is put on its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-005")]
    public void StatementSharingLineWithClosingBraceGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                                     if (a > 0) { a++; } a++;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;

                                        if (a > 0)
                                        {
                                            a++;
                                        }

                                        a++;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line after a closing brace goes behind the directive and in front of the comment that
    /// stands between that directive and the next statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-005")]
    public void BlankLineAfterClosingBraceGoesBetweenDirectiveAndComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     if (a > 0)
                                     {
                                     }
                             #pragma warning disable CS0168
                                     // why
                                     a++;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        if (a > 0)
                                        {
                                        }
                                #pragma warning disable CS0168

                                        // why
                                        a++;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line after a closing brace goes behind the last directive of a conditional group whose
    /// branch is disabled text, in front of the comment that leads the next statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-005")]
    public void BlankLineAfterClosingBraceGoesBehindLastDirectiveOfConditionalGroup()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     if (a > 0)
                                     {
                                     }
                             #if NEVER
                                     a--;
                             #endif
                                     // why
                                     a++;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        if (a > 0)
                                        {
                                        }
                                #if NEVER
                                        a--;
                                #endif

                                        // why
                                        a++;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a directive directly before an <c>if</c> or a <c>return</c> suppresses their blank line, while
    /// the blank line after the closing brace still goes behind the <c>#endif</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-006")]
    [Requirement("FMT-BLANK-005")]
    [Requirement("FMT-BLANK-016")]
    public void DirectiveDirectlyBeforeStatementSuppressesStatementKindBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #if !NEVER
                                     if (a > 0)
                                     {
                                     }
                             #endif
                                     a++;
                             #pragma warning disable CS0168
                                     return;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                #if !NEVER
                                        if (a > 0)
                                        {
                                        }
                                #endif

                                        a++;
                                #pragma warning disable CS0168
                                        return;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that blank lines directly before <c>catch</c>, <c>finally</c>, <c>else</c>, and the <c>while</c> of a
    /// <c>do</c> statement are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-008")]
    public void BlankLinesBeforeChainedKeywordsAreRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     try
                                     {
                                         a++;
                                     }

                                     catch
                                     {
                                     }

                                     finally
                                     {
                                     }
                                     if (a > 0)
                                     {
                                     }

                                     else
                                     {
                                     }
                                     do
                                     {
                                     }

                                     while (a > 0);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        try
                                        {
                                            a++;
                                        }
                                        catch
                                        {
                                        }
                                        finally
                                        {
                                        }

                                        if (a > 0)
                                        {
                                        }
                                        else
                                        {
                                        }

                                        do
                                        {
                                        }
                                        while (a > 0);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line directly before the opening brace of a method body is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-008")]
    public void BlankLineBeforeOpeningBraceIsRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()

                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between a documentation comment and an ordinary comment below it is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-010")]
    public void BlankLineBetweenDocumentationCommentAndNextCommentIsRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// Doc
                                 /// </summary>

                                 // comment
                                 void M()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// Doc
                                    /// </summary>
                                    // comment
                                    void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment that is the first content of a block or a switch section gets no blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void CommentFirstInBlockOrSwitchSectionGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     // first in block
                                     a++;
                                     switch (a)
                                     {
                                         case 1:
                                             // first in section
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
                                        // first in block
                                        a++;

                                        switch (a)
                                        {
                                            case 1:
                                                // first in section
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment moved off the line of the code before it gets one blank line above it,
    /// and that a delimited documentation comment behind an enum member is moved below a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void DocumentationWrittenBehindCodeIsSeparatedFromIt()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int A; /// <inheritdoc/>
                                 int B;
                             }

                             enum E
                             {
                                 First, /** doc */
                                 Second
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int A;

                                    /// <inheritdoc/>
                                    int B;
                                }

                                enum E
                                {
                                    First,

                                    /** doc */
                                    Second
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that blank lines inside disabled text, a verbatim string literal, and a multi-line block comment are
    /// kept, while only the blank line before the block comment is inserted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-015")]
    public void BlankLinesInsideLiteralsCommentsAndDisabledTextAreKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #if NEVER
                                     a++;



                                     a++;
                             #endif
                                     var s = @"x



                             y";
                                     /* one


                                        two */
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                #if NEVER
                                        a++;



                                        a++;
                                #endif
                                        var s = @"x



                                y";

                                        /* one


                                           two */
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}