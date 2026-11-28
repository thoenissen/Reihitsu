using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for the blank lines before the closing parenthesis of the positional pattern clause of a recursive
/// pattern, which are removed as before the closing bracket of a list pattern, while the closing parenthesis of any other
/// node keeps them
/// </summary>
[TestClass]
public class PositionalPatternClosingParenthesisBlankLineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A type with deconstructors for one and two subpatterns and a property, used as the type of positional patterns
    /// </summary>
    private const string DeconstructableType = """
                                               class P
                                               {
                                                   public int X { get; set; }

                                                   public void Deconstruct(out int a, out int b)
                                                   {
                                                       a = 0;
                                                       b = 0;
                                                   }

                                                   public void Deconstruct(out int a)
                                                   {
                                                       a = 0;
                                                   }
                                               }


                                               """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: the blank line before the closing parenthesis of a positional pattern clause is
    /// removed, with and without a type and in a switch expression arm
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void SpecExampleRemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClause()
    {
        const string input = """
                             class P
                             {
                                 public void Deconstruct(out int a, out int b)
                                 {
                                     a = 0;
                                     b = 0;
                                 }
                             }

                             class C
                             {
                                 void M(object o, (int, int) t)
                                 {
                                     var a = o is P(
                                         1,
                                         2

                                     );
                                     var b = t is (
                                         1,
                                         2

                                     );
                                     var c = o switch
                                     {
                                         P(
                                             1,
                                             2

                                         ) => 1,
                                         _ => 0,
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class P
                                {
                                    public void Deconstruct(out int a, out int b)
                                    {
                                        a = 0;
                                        b = 0;
                                    }
                                }

                                class C
                                {
                                    void M(object o, (int, int) t)
                                    {
                                        var a = o is P(
                                                          1,
                                                          2
                                                      );
                                        var b = t is (
                                                         1,
                                                         2
                                                     );
                                        var c = o switch
                                                {
                                                    P(
                                                         1,
                                                         2
                                                     ) => 1,
                                                    _ => 0,
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: in a positional pattern clause and a list pattern with no element, the blank line
    /// behind the opening token is also the one directly before the closing token, and it is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void SpecExampleRemovesBlankLineInConstructWithNoElement()
    {
        const string input = """
                             class P
                             {
                                 public void Deconstruct()
                                 {
                                 }
                             }

                             class C
                             {
                                 void M(object o, int[] arr)
                                 {
                                     var a = o is P(

                                     );
                                     var b = arr is [

                                     ];
                                 }
                             }
                             """;
        const string expected = """
                                class P
                                {
                                    public void Deconstruct()
                                    {
                                    }
                                }

                                class C
                                {
                                    void M(object o, int[] arr)
                                    {
                                        var a = o is P(
                                                      );
                                        var b = arr is [
                                                       ];
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: the closing parenthesis of an argument list is not named, so the blank line in
    /// front of it is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void SpecExampleKeepsBlankLineBeforeClosingParenthesisOfArgumentList()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var b = N(1,
                                               2

                                               );
                                 }

                                 bool N(int a, int b)
                                 {
                                     return true;
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that several blank lines before the closing parenthesis of a positional pattern clause are all removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void RemovesSeveralBlankLinesBeforeClosingParenthesisOfPositionalPatternClause()
    {
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o)
                                                       {
                                                           return o is P(
                                                               1,
                                                               2


                                                           );
                                                       }
                                                   }
                                                   """;
        const string expected = DeconstructableType + """
                                                      class C
                                                      {
                                                          bool M(object o)
                                                          {
                                                              return o is P(
                                                                               1,
                                                                               2
                                                                           );
                                                          }
                                                      }
                                                      """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line before the closing parenthesis of a positional pattern clause in a <c>case</c> label is
    /// removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void RemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClauseInCaseLabel()
    {
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       int M(object o)
                                                       {
                                                           switch (o)
                                                           {
                                                               case P(
                                                                   1,
                                                                   2

                                                               ):
                                                                   {
                                                                       return 1;
                                                                   }
                                                           }

                                                           return 0;
                                                       }
                                                   }
                                                   """;
        const string expected = DeconstructableType + """
                                                      class C
                                                      {
                                                          int M(object o)
                                                          {
                                                              switch (o)
                                                              {
                                                                  case P(
                                                                            1,
                                                                            2
                                                                        ):
                                                                      {
                                                                          return 1;
                                                                      }
                                                              }

                                                              return 0;
                                                          }
                                                      }
                                                      """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank lines before the closing parenthesis of a positional pattern clause nested in another one and
    /// before the closing parenthesis of the outer clause are both removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void RemovesBlankLinesBeforeClosingParenthesesOfNestedPositionalPatternClauses()
    {
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o)
                                                       {
                                                           return o is P(
                                                               P(
                                                                   1,
                                                                   2

                                                               ),
                                                               3

                                                           );
                                                       }
                                                   }
                                                   """;
        const string expected = DeconstructableType + """
                                                      class C
                                                      {
                                                          bool M(object o)
                                                          {
                                                              return o is P(
                                                                               P(
                                                                                    1,
                                                                                    2
                                                                                ),
                                                                               3
                                                                           );
                                                          }
                                                      }
                                                      """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line before the closing parenthesis of a positional pattern clause behind <c>not</c> is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void RemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClauseBehindNot()
    {
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o)
                                                       {
                                                           return o is not P(
                                                               1,
                                                               2

                                                           );
                                                       }
                                                   }
                                                   """;
        const string expected = DeconstructableType + """
                                                      class C
                                                      {
                                                          bool M(object o)
                                                          {
                                                              return o is not P(
                                                                                   1,
                                                                                   2
                                                                               );
                                                          }
                                                      }
                                                      """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line before the closing parenthesis of a positional pattern clause followed by a property
    /// pattern clause is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void RemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClauseFollowedByPropertyClause()
    {
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o)
                                                       {
                                                           return o is P(
                                                               1,
                                                               2

                                                           ) { X: 1 };
                                                       }
                                                   }
                                                   """;
        const string expected = DeconstructableType + """
                                                      class C
                                                      {
                                                          bool M(object o)
                                                          {
                                                              return o is P(
                                                                               1,
                                                                               2
                                                                           ) { X: 1 };
                                                          }
                                                      }
                                                      """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line before the closing parenthesis of a positional pattern clause followed by a designation
    /// is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void RemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClauseFollowedByDesignation()
    {
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o)
                                                       {
                                                           return o is P(
                                                               1,
                                                               2

                                                           ) p;
                                                       }
                                                   }
                                                   """;
        const string expected = DeconstructableType + """
                                                      class C
                                                      {
                                                          bool M(object o)
                                                          {
                                                              return o is P(
                                                                               1,
                                                                               2
                                                                           ) p;
                                                          }
                                                      }
                                                      """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line before the closing parenthesis of a positional pattern clause with one subpattern is
    /// removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void RemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClauseWithOneSubpattern()
    {
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o)
                                                       {
                                                           return o is P(
                                                               1

                                                           );
                                                       }
                                                   }
                                                   """;
        const string expected = DeconstructableType + """
                                                      class C
                                                      {
                                                          bool M(object o)
                                                          {
                                                              return o is P(
                                                                               1
                                                                           );
                                                          }
                                                      }
                                                      """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line before the closing parenthesis of a positional pattern clause is removed when the last
    /// subpattern's line ends with a comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void RemovesBlankLineBeforeClosingParenthesisOfPositionalPatternClauseBehindEndOfLineComment()
    {
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o)
                                                       {
                                                           return o is P(
                                                               1,
                                                               2 // c

                                                           );
                                                       }
                                                   }
                                                   """;
        const string expected = DeconstructableType + """
                                                      class C
                                                      {
                                                          bool M(object o)
                                                          {
                                                              return o is P(
                                                                               1,
                                                                               2 // c
                                                                           );
                                                          }
                                                      }
                                                      """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line behind the opening parenthesis of a positional pattern clause that holds a subpattern is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void KeepsBlankLineBehindOpeningParenthesisOfPositionalPatternClause()
    {
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o)
                                                       {
                                                           return o is P(

                                                               1,
                                                               2
                                                           );
                                                       }
                                                   }
                                                   """;
        const string expected = DeconstructableType + """
                                                      class C
                                                      {
                                                          bool M(object o)
                                                          {
                                                              return o is P(

                                                                               1,
                                                                               2
                                                                           );
                                                          }
                                                      }
                                                      """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change example: between a comment on its own line and the closing parenthesis of a positional pattern
    /// clause the blank line is removed, and the one above the comment is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenOwnLineCommentAndClosingParenthesisOfPositionalPatternClause()
    {
        const string input = """
                             class P
                             {
                                 public void Deconstruct(out int a, out int b)
                                 {
                                     a = 0;
                                     b = 0;
                                 }
                             }

                             class C
                             {
                                 void M(object o)
                                 {
                                     var a = o is P(
                                         1,
                                         2
                                         // last

                                     );
                                 }
                             }
                             """;
        const string expected = """
                                class P
                                {
                                    public void Deconstruct(out int a, out int b)
                                    {
                                        a = 0;
                                        b = 0;
                                    }
                                }

                                class C
                                {
                                    void M(object o)
                                    {
                                        var a = o is P(
                                                          1,
                                                          2

                                                          // last
                                                      );
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change example: the blank line above a comment glued to the closing parenthesis of a positional pattern
    /// clause is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesBlankLineAboveCommentGluedToClosingParenthesisOfPositionalPatternClause()
    {
        const string input = """
                             class P
                             {
                                 public void Deconstruct(out int a, out int b)
                                 {
                                     a = 0;
                                     b = 0;
                                 }
                             }

                             class C
                             {
                                 void M(object o)
                                 {
                                     var b = o is P(
                                         1,
                                         2

                                         /* last */ );
                                 }
                             }
                             """;
        const string expected = """
                                class P
                                {
                                    public void Deconstruct(out int a, out int b)
                                    {
                                        a = 0;
                                        b = 0;
                                    }
                                }

                                class C
                                {
                                    void M(object o)
                                    {
                                        var b = o is P(
                                                          1,
                                                          2
                                                      /* last */ );
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the author's blank line above a multi-line comment glued to the closing parenthesis of a positional
    /// pattern clause is removed. Only the blank lines are asserted: no requirement decides the column of a multi-line
    /// comment's continuation line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-018")]
    public void RemovesBlankLineAboveMultiLineCommentGluedToClosingParenthesisOfPositionalPatternClause()
    {
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o)
                                                       {
                                                           return o is P(
                                                               1,
                                                               2

                                                               /* z
                                                                */ );
                                                       }
                                                   }
                                                   """;

        AssertBlankLinesAbove(input, ("/* z", false));
    }

    /// <summary>
    /// Verifies the change example: in front of the closing parenthesis of a positional pattern clause the blank line below
    /// an <c>#endif</c> is removed, and the one above the <c>#if</c> is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    public void RemovesOnlyBlankLineBelowDirectiveBeforeClosingParenthesisOfPositionalPatternClause()
    {
        const string input = """
                             class P
                             {
                                 public void Deconstruct(out int a, out int b)
                                 {
                                     a = 0;
                                     b = 0;
                                 }
                             }

                             class C
                             {
                                 void M(object o)
                                 {
                                     var c = o is P(
                                         1,
                                         2

                             #if DEBUG
                             #endif

                                     );
                                 }
                             }
                             """;
        const string expected = """
                                class P
                                {
                                    public void Deconstruct(out int a, out int b)
                                    {
                                        a = 0;
                                        b = 0;
                                    }
                                }

                                class C
                                {
                                    void M(object o)
                                    {
                                        var c = o is P(
                                                          1,
                                                          2

                                #if DEBUG
                                #endif
                                                      );
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing parenthesis of a parenthesized variable designation is not named, so the blank line in front
    /// of it is kept. Only the blank lines are asserted: no requirement decides the columns of a parenthesized variable
    /// designation's lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-019")]
    public void KeepsBlankLineBeforeClosingParenthesisOfParenthesizedVariableDesignation()
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is var (
                                         x,
                                         y

                                     );
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, (");", true));
    }

    #endregion // Methods
}