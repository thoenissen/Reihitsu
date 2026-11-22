using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the positional pattern clause of a recursive pattern, which is laid out like a list pattern once it
/// spans several lines: one subpattern per line, one level deeper than the opening parenthesis, the closing parenthesis under the
/// opening one, and the opening parenthesis on the line of the pattern's type or of the token in front of it
/// </summary>
[TestClass]
public class PositionalPatternLayoutTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A type with a deconstructor, used as the type of positional patterns
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
                                               }


                                               """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: a wrapped positional pattern, with and without a type, gets one subpattern per line
    /// and keeps its opening parenthesis on the line of its type or its <c>is</c>; one written on one line stays on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-022")]
    public void SpecExampleWrappedPositionalPatternGetsOneSubpatternPerLine()
    {
        // Arrange
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
                                 bool M(object o, (int, int) t)
                                 {
                                     var a = o is P(1,
                                         2);
                                     var b = t is (
                                         1,
                                         2
                                         );
                                     var c = t is (1, 2);

                                     return a && b && c;
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
                                    bool M(object o, (int, int) t)
                                    {
                                        var a = o is P(
                                                          1,
                                                          2
                                                      );
                                        var b = t is (
                                                         1,
                                                         2
                                                     );
                                        var c = t is (1, 2);

                                        return a && b && c;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: the subpatterns of a positional pattern, with and without a type, also in a
    /// <c>case</c> label, are indented one level deeper than the opening parenthesis, and the closing parenthesis stands under it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-025")]
    public void SpecExamplePositionalPatternIndentsFromItsOpeningParenthesis()
    {
        // Arrange
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
                                 bool M(object o, (int, int) t)
                                 {
                                     var a = o is P(
                                     1,
                                             2
                                         );

                                     switch (o)
                                     {
                                         case (
                                         1,
                                         2
                                         ):
                                             break;
                                     }

                                     return a && t is (
                                     1,
                                     2
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
                                    bool M(object o, (int, int) t)
                                    {
                                        var a = o is P(
                                                          1,
                                                          2
                                                      );

                                        switch (o)
                                        {
                                            case (
                                                     1,
                                                     2
                                                 ):
                                                break;
                                        }

                                        return a && t is (
                                                             1,
                                                             2
                                                         );
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change's example: a wrapped positional pattern behind its type, behind <c>is</c>, with a comma that starts a
    /// line, in a switch-expression arm, and in a <c>case</c> label is laid out like a list pattern, and the opening parenthesis
    /// written below the type joins it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-022")]
    [Requirement("FMT-INDENT-025")]
    public void WrappedPositionalPatternsAreLaidOutLikeListPatternsInEveryPlaceAPatternStands()
    {
        // Arrange
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
                                 bool M(object o, (int, int) t)
                                 {
                                     var a = o is P(1,
                                         2);
                                     var b = t is (
                                         1,
                                         2
                                         );
                                     var c = o is P
                                         (1, 2);
                                     var d = t is (1
                                         , 2);

                                     return a && b && c && d;
                                 }

                                 int K((int, int) t)
                                 {
                                     return t switch
                                     {
                                         (1,
                                             2) => 1,
                                         _ => 0
                                     };
                                 }

                                 void N(object o)
                                 {
                                     switch (o)
                                     {
                                         case (1,
                                             2):
                                             break;
                                     }
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
                                    bool M(object o, (int, int) t)
                                    {
                                        var a = o is P(
                                                          1,
                                                          2
                                                      );
                                        var b = t is (
                                                         1,
                                                         2
                                                     );
                                        var c = o is P(1, 2);
                                        var d = t is (
                                                         1,
                                                         2
                                                     );

                                        return a && b && c && d;
                                    }

                                    int K((int, int) t)
                                    {
                                        return t switch
                                               {
                                                   (
                                                       1,
                                                       2
                                                   ) => 1,
                                                   _ => 0
                                               };
                                    }

                                    void N(object o)
                                    {
                                        switch (o)
                                        {
                                            case (
                                                     1,
                                                     2
                                                 ):
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: the opening parenthesis of a positional pattern clause written below the pattern's
    /// type joins the type's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-043")]
    public void SpecExamplePositionalPatternClauseJoinsTheLineOfItsType()
    {
        // Arrange
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
                                 bool M(object o)
                                 {
                                     return o is P
                                         (1, 2);
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
                                    bool M(object o)
                                    {
                                        return o is P(1, 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that blank lines between the pattern's type and the opening parenthesis of its positional clause go with the
    /// join
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-043")]
    public void BlankLinesBetweenTheTypeAndThePositionalClauseGoWithTheJoin()
    {
        // Arrange
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o)
                                                       {
                                                           return o is P

                                                               (1, 2);
                                                       }
                                                   }
                                                   """;
        const string expected = DeconstructableType + """
                                                      class C
                                                      {
                                                          bool M(object o)
                                                          {
                                                              return o is P(1, 2);
                                                          }
                                                      }
                                                      """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment between the pattern's type and the opening parenthesis of its positional clause blocks the join,
    /// and the parenthesis keeps its own line as a continuation line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-043")]
    public void CommentBetweenTheTypeAndThePositionalClauseBlocksTheJoin()
    {
        // Arrange
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o)
                                                       {
                                                           return o is P // c
                                                               (1, 2);
                                                       }
                                                   }
                                                   """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a conditional group between the pattern's type and the opening parenthesis of its positional clause blocks
    /// the join
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-043")]
    public void DirectiveBetweenTheTypeAndThePositionalClauseBlocksTheJoin()
    {
        // Arrange
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o)
                                                       {
                                                           return o is P
                                                   #if DEBUG
                                                   #endif
                                                               (1, 2);
                                                       }
                                                   }
                                                   """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: a comma that starts a line in a positional pattern moves behind the subpattern in
    /// front of it, and the pattern, then wrapped, gets one subpattern per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void SpecExampleCommaThatStartsALineInAPositionalPatternEndsTheSubpatternsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M((int, int) t)
                                 {
                                     return t is (1
                                         , 2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M((int, int) t)
                                    {
                                        return t is (
                                                        1,
                                                        2
                                                    );
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment behind a comma that starts a line in a positional pattern moves with the comma and stays
    /// behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void BlockCommentBehindACommaThatStartsALineInAPositionalPatternMovesWithTheComma()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M((int, int) t)
                                 {
                                     return t is (1
                                         , /* c */ 2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M((int, int) t)
                                    {
                                        return t is (
                                                        1, /* c */
                                                        2
                                                    );
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a positional pattern that spans several lines next to a combinator wraps the chain,
    /// so the combinator starts its own line, and the opening parenthesis stays on the combinator's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void SpecExampleMultiLinePositionalPatternNextToACombinatorWrapsTheChain()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is (1, 1) and (2,
                                         3);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(object o)
                                    {
                                        return o is (1, 1)
                                                 and (
                                                         2,
                                                         3
                                                     );
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the chain condition: positional patterns on one line next to a combinator leave the chain
    /// unwrapped
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void SingleLinePositionalPatternsNextToACombinatorKeepTheChainOnOneLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is (1, 1) and (2, 3);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a wrapped positional pattern behind <c>not</c> keeps its opening parenthesis on the <c>not</c> line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-022")]
    [Requirement("FMT-INDENT-025")]
    public void WrappedPositionalPatternBehindNotKeepsItsOpeningParenthesisOnTheNotLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is not (1,
                                         2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(object o)
                                    {
                                        return o is not (
                                                            1,
                                                            2
                                                        );
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped positional pattern with a designation, with named subpatterns, and with a property pattern clause
    /// on one line behind it gets one subpattern per line, and what follows the closing parenthesis stays behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-022")]
    [Requirement("FMT-INDENT-025")]
    public void WrappedPositionalPatternWithDesignationNamesOrPropertyClauseGetsOneSubpatternPerLine()
    {
        // Arrange
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o, (int, int) t)
                                                       {
                                                           var d = o is P(1,
                                                               2) p;
                                                           var e = t is (a: 1,
                                                               b: 2);
                                                           var m = o is P(1,
                                                               2) { X: 1 };

                                                           return d && e && m;
                                                       }
                                                   }
                                                   """;
        const string expected = DeconstructableType + """
                                                      class C
                                                      {
                                                          bool M(object o, (int, int) t)
                                                          {
                                                              var d = o is P(
                                                                                1,
                                                                                2
                                                                            ) p;
                                                              var e = t is (
                                                                               a: 1,
                                                                               b: 2
                                                                           );
                                                              var m = o is P(
                                                                                1,
                                                                                2
                                                                            ) { X: 1 };

                                                              return d && e && m;
                                                          }
                                                      }
                                                      """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped positional pattern nested in a positional pattern and one in an element of a list pattern each
    /// get one subpattern per line, indented from their own opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-022")]
    [Requirement("FMT-INDENT-025")]
    public void NestedWrappedPositionalPatternsIndentFromTheirOwnOpeningParenthesis()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(object o, object[] l)
                                 {
                                     var f = o is (1, (2,
                                         3));
                                     var g = l is [(1,
                                         2)];

                                     return f && g;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(object o, object[] l)
                                    {
                                        var f = o is (
                                                         1,
                                                         (
                                                             2,
                                                             3
                                                         )
                                                     );
                                        var g = l is [
                                                         (
                                                             1,
                                                             2
                                                         )
                                                     ];

                                        return f && g;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the construct: a parenthesized variable designation behind <c>var</c> is no positional
    /// pattern, so its wrapped line keeps the continuation indent
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-022")]
    public void WrappedParenthesizedVariableDesignationIsNotLaidOutAsAPositionalPattern()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is var (x,
                                         y);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the change's example: a comment in front of the closing parenthesis of a wrapped positional pattern goes into
    /// the subpatterns' column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-006")]
    public void CommentBeforeClosingParenthesisOfWrappedPositionalPatternIsIndentedIntoTheSubpatterns()
    {
        // Arrange
        const string input = DeconstructableType + """
                                                   class C
                                                   {
                                                       bool M(object o)
                                                       {
                                                           return o is P(1,
                                                               2

                                                               // last
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

                                                                               // last
                                                                           );
                                                          }
                                                      }
                                                      """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}