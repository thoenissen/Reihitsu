using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.SwitchCaseBraces;

/// <summary>
/// Full-pipeline tests for the switch-section brace requirements
/// </summary>
[TestClass]
public class SwitchCaseBraceRequirementTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that a switch without a multi-line section stays unbraced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-001")]
    public void SwitchWithoutMultiLineSectionStaysUnbraced()
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
                                             break;

                                         case 2:
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a final <c>goto case</c> goes inside the new block and, as an ordinary
    /// statement after another one, makes the section multi-line without blocking the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-002")]
    [Requirement("FMT-SWITCH-011")]
    public void FinalGotoCaseGoesInsideTheBlockAndDoesNotBlockTheBraces()
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

                                             goto case 2;

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
                                                {
                                                    A();

                                                    goto case 2;
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
    /// Verifies that one statement spanning two lines makes its section multi-line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-003")]
    public void SingleStatementSpanningTwoLinesMakesTheSectionMultiLine()
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
                                             A(1,
                                               2);
                                             break;

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
                                                {
                                                    A(1,
                                                      2);
                                                }
                                                break;

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
    /// Verifies that a section whose only statement is a wrapped <c>return</c> is not multi-line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-003")]
    public void WrappedReturnAloneDoesNotMakeTheSectionMultiLine()
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
                                             A();
                                             break;

                                         case 2:
                                             return Compute(1,
                                                            2);
                                     }

                                     return 0;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a wrap inside a list element, which the line-break phase joins, does not make the section multi-line,
    /// so no section is braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-003")]
    public void WrapJoinedInsideListElementDoesNotMakeTheSectionMultiLine()
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
                                             A(out
                                               var v);
                                             break;
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
                                                A(out var v);
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
    /// Verifies that a wrap after an assignment operator, which the line-break phase joins, does not make the section
    /// multi-line, so no section is braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-003")]
    public void WrapJoinedAfterAssignmentOperatorDoesNotMakeTheSectionMultiLine()
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
                                             var y =
                                                 1;
                                             break;
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
                                                var y = 1;
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
    /// Verifies that a wrapped member-access chain, which the line-break phase keeps, makes the section multi-line and
    /// braces the switch
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-003")]
    public void WrapKeptInMemberAccessChainMakesTheSectionMultiLine()
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
                                             a.B()
                                              .C();
                                             break;
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
                                                {
                                                    a.B()
                                                     .C();
                                                }
                                                break;

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
    /// Verifies that an object initializer written on one line, which the line-break phase lays out over several lines,
    /// makes the section multi-line and braces the switch
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-003")]
    public void ObjectInitializerExpandedByLineBreaksMakesTheSectionMultiLine()
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
                                             var y = new D { A = 1, B = 2 };
                                             break;
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
                                                {
                                                    var y = new D
                                                            {
                                                                A = 1,
                                                                B = 2
                                                            };
                                                }
                                                break;

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
    /// Verifies that a lambda block body written on one line, which the line-break phase lays out over several lines,
    /// makes the section multi-line and braces the switch
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-003")]
    public void LambdaBlockBodyExpandedByLineBreaksMakesTheSectionMultiLine()
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
                                             Run(() => { X(); Y(); });
                                             break;
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
                                                {
                                                    Run(() =>
                                                        {
                                                            X();
                                                            Y();
                                                        });
                                                }
                                                break;

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
    /// Verifies that a statement followed by a <c>return</c> on the same line makes the section multi-line, because the
    /// line-break phase puts the <c>return</c> on a later line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-003")]
    public void ReturnAfterStatementOnTheSameLineMakesTheSectionMultiLine()
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
                                             A(); return 1;
                                         case 2:
                                             return 2;
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
    /// Verifies that a comment between a statement and a <c>return</c> on the same line stays behind the statement while
    /// the <c>return</c> moves to a later line and makes the section multi-line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-003")]
    public void ReturnAfterStatementAndCommentOnTheSameLineMakesTheSectionMultiLine()
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
                                             A(); /* c */ return 1;
                                         case 2:
                                             return 2;
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
                                                    A(); /* c */

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
    /// Verifies that a property pattern spanning several lines in a label makes the section
    /// multi-line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-004")]
    public void MultiLinePropertyPatternInLabelMakesTheSectionMultiLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object o)
                                 {
                                     switch (o)
                                     {
                                         case Point
                                              {
                                                  X: 1
                                              }:
                                             A();
                                             break;

                                         case string s:
                                             B();
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
                                            case Point
                                                 {
                                                     X: 1
                                                 }:
                                                {
                                                    A();
                                                }
                                                break;

                                            case string s:
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
    /// Verifies that a label spanning two lines only because its <c>when</c> clause wraps does
    /// not make the section multi-line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-004")]
    public void WrappedWhenClauseDoesNotMakeTheSectionMultiLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1 when x > 0
                                                     && x < 10:
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
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>break</c>-only section with a one-line label does not make an otherwise
    /// single-line switch multi-line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-006")]
    public void BreakOnlySectionDoesNotMakeTheSwitchMultiLine()
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
                                             break;

                                         case 2:
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an existing one-line block is kept, laid out over several lines, and braces
    /// the other sections of the switch
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-008")]
    public void ExistingBlockIsKeptAndBracesTheOtherSections()
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
                                             { A(); }
                                             break;

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
                                                {
                                                    A();
                                                }
                                                break;

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
    /// Verifies that statements following an existing block stay outside it while the other
    /// section is braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-009")]
    public void StatementsAfterAnExistingBlockStayWhereTheyAre()
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
                                             }

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
                                                }

                                                B();
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
    /// Verifies that a section holding disabled text between directives stays unbraced while the
    /// other sections of the multi-line switch are braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-010")]
    public void SectionWithDirectiveStaysUnbracedWhileTheOthersAreBraced()
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
                             #if DEBUG
                                             A();
                             #endif
                                             B();
                                             break;

                                         case 2:
                                             C();
                                             D();
                                             break;

                                         case 3:
                                             E();
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
                                #if DEBUG
                                                A();
                                #endif
                                                B();
                                                break;

                                            case 2:
                                                {
                                                    C();
                                                    D();
                                                }
                                                break;

                                            case 3:
                                                {
                                                    E();
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
    /// Verifies that an active directive in a section also leaves that section unbraced while the
    /// other sections of the multi-line switch are braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-010")]
    public void SectionWithActiveDirectiveStaysUnbracedWhileTheOthersAreBraced()
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
                             #if DEBUG
                                             A();
                             #endif
                                             B();
                                             break;

                                         case 2:
                                             C();
                                             D();
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
                                #if DEBUG
                                                A();
                                #endif
                                                B();
                                                break;

                                            case 2:
                                                {
                                                    C();
                                                    D();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, new CSharpParseOptions(preprocessorSymbols: ["DEBUG"]));
    }

    /// <summary>
    /// Verifies that an ordinary <c>goto</c> into another section leaves the whole switch unbraced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-011")]
    public void GotoIntoAnotherSectionLeavesTheSwitchUnbraced()
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

                                             goto target;

                                         case 2:
                                             target:
                                             B();
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
    /// Verifies that equal label names inside the lambdas of two sections do not count as a
    /// <c>goto</c> into another section
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-012")]
    public void LabelsInsideLambdasDoNotBlockTheBraces()
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
                                             Action a = () =>
                                                        {
                                                            goto target;
                                                            target:
                                                            return;
                                                        };
                                             break;

                                         case 2:
                                             Action b = () =>
                                                        {
                                                            target:
                                                            return;
                                                        };
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
                                                    Action a = () =>
                                                               {
                                                                   goto target;
                                                                   target:
                                                                   return;
                                                               };
                                                }
                                                break;

                                            case 2:
                                                {
                                                    Action b = () =>
                                                               {
                                                                   target:
                                                                   return;
                                                               };
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
    /// Verifies that an inner switch of one-line sections stays unbraced while the outer switch is
    /// braced because the inner switch spans several lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-013")]
    public void NestedSwitchIsDecidedOnItsOwnSections()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x, int y)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             switch (y)
                                             {
                                                 case 1:
                                                     A();
                                                     break;

                                                 case 2:
                                                     B();
                                                     break;
                                             }

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
                                    void M(int x, int y)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    switch (y)
                                                    {
                                                        case 1:
                                                            A();
                                                            break;

                                                        case 2:
                                                            B();
                                                            break;
                                                    }
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
    /// Verifies that an inner switch with a multi-line section is braced on its own while it
    /// braces the outer switch through the lines it occupies
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-013")]
    public void NestedMultiLineSwitchIsBracedOnItsOwnSections()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x, int y)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             switch (y)
                                             {
                                                 case 1:
                                                     A();
                                                     B();
                                                     break;

                                                 case 2:
                                                     C();
                                                     break;
                                             }

                                             break;

                                         case 2:
                                             D();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x, int y)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    switch (y)
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
                                                break;

                                            case 2:
                                                {
                                                    D();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}