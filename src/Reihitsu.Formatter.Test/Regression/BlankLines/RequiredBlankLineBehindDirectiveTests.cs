using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for the place of a blank line that a statement or switch-section rule requires in a gap that holds
/// a preprocessor directive: behind the last directive of the gap, in front of the comments between that directive and
/// the code, never directly in front of a directive, and not at all when a directive directly precedes the statement or
/// the switch section
/// </summary>
[TestClass]
public class RequiredBlankLineBehindDirectiveTests : FormatterTestsBase
{
    #region Methods

    #region Statement gaps with a comment behind the directive

    /// <summary>
    /// Verifies the spec example: the blank line in front of an <c>if</c> statement goes behind the <c>#endif</c> of a
    /// conditional group whose branch the build does not compile, in front of the comment that leads the statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-001")]
    public void BlankLineBeforeIfGoesBehindEndIfOfDisabledGroup()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #if NEVER
                                     a--;
                             #endif
                                     // why
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
                                        a++;
                                #if NEVER
                                        a--;
                                #endif

                                        // why
                                        if (a > 0)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line in front of an <c>if</c> statement goes behind the <c>#endif</c> of a conditional group
    /// whose compiled branch holds the preceding statement, in front of the comment that leads the <c>if</c> statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-001")]
    public void BlankLineBeforeIfGoesBehindEndIfOfActiveGroup()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #if !NEVER
                                     a--;
                             #endif
                                     // why
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
                                        a++;
                                #if !NEVER
                                        a--;
                                #endif

                                        // why
                                        if (a > 0)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line in front of a <c>return</c> statement goes behind a <c>#pragma</c> directive, in front
    /// of the comment that leads the statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-001")]
    public void BlankLineBeforeReturnGoesBehindPragmaInFrontOfComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #pragma warning disable CS0168
                                     // why
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
                                #pragma warning disable CS0168

                                        // why
                                        return;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line in front of a local declaration goes behind a <c>#pragma</c> directive, in front of the
    /// comment that leads the declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-002")]
    public void BlankLineBeforeLocalDeclarationGoesBehindPragmaInFrontOfComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #pragma warning disable CS0168
                                     // why
                                     var b = a;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                #pragma warning disable CS0168

                                        // why
                                        var b = a;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line in front of an expression statement behind a local declaration goes behind a
    /// <c>#pragma</c> directive, in front of the comment that leads the statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-002")]
    public void BlankLineBehindLocalDeclarationGoesBehindPragmaInFrontOfComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     var b = a;
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
                                        var b = a;
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
    /// Verifies that the blank line in front of a <c>yield return</c> statement goes behind a <c>#pragma</c> directive, in
    /// front of the comment that leads the statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-003")]
    public void BlankLineBeforeYieldGoesBehindPragmaInFrontOfComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 IEnumerable<int> M(int a)
                                 {
                                     a++;
                             #pragma warning disable CS0168
                                     // why
                                     yield return a;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    IEnumerable<int> M(int a)
                                    {
                                        a++;
                                #pragma warning disable CS0168

                                        // why
                                        yield return a;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line behind a <c>break</c> statement goes behind a <c>#pragma</c> directive, in front of the
    /// comment that leads the next statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-004")]
    public void BlankLineBehindBreakGoesBehindPragmaInFrontOfComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     while (a > 0)
                                     {
                                         break;
                             #pragma warning disable CS0168
                                         // why
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
                                        while (a > 0)
                                        {
                                            break;
                                #pragma warning disable CS0168

                                            // why
                                            a++;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line in front of a <c>break</c> statement goes behind a <c>#pragma</c> directive, in front of
    /// the comment that leads the <c>break</c> statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-004")]
    public void BlankLineBeforeBreakGoesBehindPragmaInFrontOfComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     while (a > 0)
                                     {
                                         a++;
                             #pragma warning disable CS0168
                                         // why
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
                                        while (a > 0)
                                        {
                                            a++;
                                #pragma warning disable CS0168

                                            // why
                                            break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: the blank lines in front of a local declaration and behind a <c>break</c> go behind the
    /// <c>#if</c>, in front of the comment that leads the statement, while the <c>while</c> statement directly behind the
    /// <c>#endif</c> gets none
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-002")]
    [Requirement("FMT-BLANK-004")]
    [Requirement("FMT-BLANK-006")]
    public void BlankLinesBeforeDeclarationAndBehindBreakGoBehindIfDirective()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #if !NEVER
                                     // setup
                                     var b = a;
                             #endif
                                     while (a > 0)
                                     {
                                         break;
                             #if !NEVER
                                         // scope
                                         {
                                             a++;
                                         }
                             #endif
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
                                #if !NEVER

                                        // setup
                                        var b = a;
                                #endif
                                        while (a > 0)
                                        {
                                            break;
                                #if !NEVER

                                            // scope
                                            {
                                                a++;
                                            }
                                #endif
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line in front of an <c>if</c> statement goes behind the <c>#if</c> when the preceding
    /// statement ends with a trailing comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-001")]
    public void BlankLineBeforeIfGoesBehindIfDirectiveBehindTrailingComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++; // tail
                             #if !NEVER
                                     // why
                                     if (a > 0)
                                     {
                                     }
                             #endif
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++; // tail
                                #if !NEVER

                                        // why
                                        if (a > 0)
                                        {
                                        }
                                #endif
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line in front of an <c>if</c> statement goes behind the last of two directives with comments
    /// between them, in front of the comment that leads the statement, and not in front of the first directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-001")]
    public void BlankLineBeforeIfGoesBehindLastOfTwoDirectivesWithComments()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #if !NEVER
                                     // c1
                             #pragma warning disable CS0168
                                     // c2
                                     if (a > 0)
                                     {
                                     }
                             #endif
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
                                        // c1
                                #pragma warning disable CS0168

                                        // c2
                                        if (a > 0)
                                        {
                                        }
                                #endif
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Statement gaps with a comment behind the directive

    #region Statement gaps that keep their layout

    /// <summary>
    /// Verifies that a blank line the author wrote in front of a directive is kept and counted, so no second blank line is
    /// inserted behind the directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-001")]
    public void AuthorBlankLineInFrontOfDirectiveIsKeptAndNotDoubled()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;

                             #pragma warning disable CS0168
                                     // why
                                     if (a > 0)
                                     {
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the blank line in front of a comment that leads the gap satisfies the statement's blank line, so no
    /// second blank line is inserted behind the directive that follows the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-001")]
    public void BlankLineInFrontOfLeadingCommentIsNotDoubledBehindDirective()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                                     // lead
                             #pragma warning disable CS0168
                                     // why
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
                                        a++;

                                        // lead
                                #pragma warning disable CS0168
                                        // why
                                        if (a > 0)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a directive directly in front of an <c>if</c> statement suppresses the statement's blank line while the
    /// comment in front of the directive keeps its own blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-006")]
    [Requirement("FMT-BLANK-011")]
    public void DirectiveDirectlyBeforeIfBehindCommentGetsNoStatementBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                                     // c
                             #pragma warning disable CS0168
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
                                        a++;

                                        // c
                                #pragma warning disable CS0168
                                        if (a > 0)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block statement directly behind an <c>#if</c> behind a <c>break</c> gets no blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-006")]
    [Requirement("FMT-BLANK-004")]
    public void BlockStatementDirectlyBehindIfDirectiveBehindBreakGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     while (a > 0)
                                     {
                                         break;
                             #if !NEVER
                                         {
                                             a++;
                                         }
                             #endif
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a statement gap whose blank line already stands behind the last directive is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    public void BlankLineBehindLastDirectiveIsLeftUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #if NEVER
                                     a--;
                             #endif

                                     // why
                                     if (a > 0)
                                     {
                                     }

                                     while (a > 0)
                                     {
                                         break;
                             #if !NEVER

                                         // scope
                                         {
                                             a++;
                                         }
                             #endif
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Statement gaps that keep their layout

    #region Switch-section gaps

    /// <summary>
    /// Verifies the spec example: a directive, or a whole conditional group whose branch the build does not compile,
    /// directly in front of a switch section behind a section that ends in <c>break</c> gets no blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-006")]
    [Requirement("FMT-BLANK-004")]
    public void DirectivesDirectlyInFrontOfSwitchSectionsLeaveTheSwitchUnchanged()
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
                             #pragma warning disable CS0168
                                         case 2:
                                             a--;
                                             break;
                             #if NEVER
                                         case 3:
                                             break;
                             #endif
                                         case 4:
                                             a++;
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>#pragma</c> directive directly in front of a switch section behind a section that ends in
    /// <c>break</c> suppresses the section's blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-006")]
    [Requirement("FMT-BLANK-004")]
    public void PragmaDirectlyInFrontOfSwitchSectionGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             value++;
                                             break;
                             #pragma warning disable CS0168
                                         case 2:
                                             value--;
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a conditional group whose branch the build does not compile, directly in front of a switch section
    /// behind a section that ends in <c>break</c>, suppresses the section's blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-006")]
    [Requirement("FMT-BLANK-004")]
    public void DisabledConditionalGroupDirectlyInFrontOfSwitchSectionGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             value++;
                                             break;
                             #if NEVER
                                         case 3:
                                             break;
                             #endif
                                         case 2:
                                             value--;
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an <c>#if</c> directive whose branch the build compiles, directly in front of a switch section behind a
    /// section that ends in <c>break</c>, suppresses the section's blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-006")]
    [Requirement("FMT-BLANK-004")]
    public void ActiveIfDirectiveDirectlyInFrontOfSwitchSectionGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             value++;
                                             break;
                             #if !NEVER
                                         case 2:
                                             value--;
                                             break;
                             #endif
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the spec example: with a comment between the directive and the switch section, the section's blank line
    /// goes behind the directive, in front of the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-006")]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-004")]
    public void SwitchSectionBlankLineGoesBehindPragmaInFrontOfComment()
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
                             #pragma warning disable CS0168
                                         // two
                                         case 2:
                                             a--;
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
                                #pragma warning disable CS0168

                                            // two
                                            case 2:
                                                a--;
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the section's blank line goes behind the <c>#endif</c> of a disabled conditional group, in front of
    /// the comment that leads the switch section
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-016")]
    [Requirement("FMT-BLANK-004")]
    public void SwitchSectionBlankLineGoesBehindEndIfOfDisabledGroupInFrontOfComment()
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
                             #if NEVER
                                         case 3:
                                             break;
                             #endif
                                         // two
                                         case 2:
                                             a--;
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
                                #if NEVER
                                            case 3:
                                                break;
                                #endif

                                            // two
                                            case 2:
                                                a--;
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a switch section behind a section that ends in <c>return</c> gets no blank line, also when a directive
    /// and a comment stand in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    public void SwitchSectionBehindReturnSectionWithDirectiveAndCommentGetsNoBlankLine()
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
                                             return;
                             #pragma warning disable CS0168
                                         // two
                                         case 2:
                                             a--;
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a switch section behind a section that ends in <c>break</c> and without a directive in front of it
    /// still gets its blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    public void SwitchSectionWithoutDirectiveBehindBreakSectionGetsBlankLine()
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
                                             a--;
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
                                                a--;
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Switch-section gaps

    #endregion // Methods
}