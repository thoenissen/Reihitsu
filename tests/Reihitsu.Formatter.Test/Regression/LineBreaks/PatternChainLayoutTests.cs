using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the layout of a pattern chain: once a line break stands next to one of its combinators, or an operand
/// next to one is a property, list, or parenthesized pattern that spans several lines, every combinator starts its own line and
/// its right pattern joins it, aligned under the <c>is</c> or the first pattern; a chain that is not wrapped keeps its layout,
/// and a parenthesized pattern starts a chain of its own
/// </summary>
[TestClass]
public class PatternChainLayoutTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: every combinator at the end of a line moves to the start of the next one in an
    /// <c>is</c> expression, a <c>case</c> label, and a switch-expression arm, and the pattern behind <c>is</c> joins it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void SpecExampleEveryCombinatorStartsItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     var k = x is
                                     1 or
                                     2 or
                                     3;

                                     switch (x)
                                     {
                                         case 1 or
                                         2:
                                             break;
                                     }

                                     return x switch
                                            {
                                                1 or
                                                3 => 4,
                                                _ => 0,
                                            };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        var k = x is 1
                                                  or 2
                                                  or 3;

                                        switch (x)
                                        {
                                            case 1
                                                 or 2:
                                                break;
                                        }

                                        return x switch
                                               {
                                                   1
                                                   or 3 => 4,
                                                   _ => 0,
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a chain wrapped at one combinator is wrapped at every combinator, a combinator in
    /// front of a multi-line property pattern starts its own line and the pattern's brace keeps its own line below it, and the
    /// type behind the <c>is</c> of a type test joins the <c>is</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void SpecExampleAChainWrappedAtOneCombinatorIsWrappedAtEveryCombinator()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int x, object o, string s)
                                 {
                                     var k = x is 1 or 2 or
                                     3;
                                     var a = s is null or
                                     {
                                     Length: 2
                                     };
                                     var t = o is
                                     string;

                                     return k && a && t;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(int x, object o, string s)
                                    {
                                        var k = x is 1
                                                  or 2
                                                  or 3;
                                        var a = s is null
                                                  or
                                                  {
                                                      Length: 2
                                                  };
                                        var t = o is string;

                                        return k && a && t;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example of the other side: a chain with no line break next to a combinator keeps its layout,
    /// and a type-test <c>is</c> that starts its line keeps its place
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void SpecExampleAChainThatIsNotWrappedKeepsItsLayout()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int x, object o)
                                 {
                                     var k = x is 1 or 2 or 3;
                                     var t = o
                                             is string;

                                     return k && t;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a combinator in front of a multi-line parenthesized pattern starts its own line, the opening parenthesis
    /// keeps its own line below it, and the inner chain, wrapped by its own combinator, aligns under its first pattern
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    [Requirement("FMT-INDENT-026")]
    public void CombinatorInFrontOfAMultiLineParenthesizedPatternStartsItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     var c = o is int or
                                     (
                                     long
                                     or short
                                     );

                                     return c;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(object o)
                                    {
                                        var c = o is int
                                                  or
                                                  (
                                                      long
                                                      or short
                                                  );

                                        return c;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the combinators of nested binary patterns belong to one chain: an <c>and</c> inside the right operand of an
    /// <c>or</c> starts its line under the <c>is</c> as well
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void CombinatorsOfNestedBinaryPatternsBelongToOneChain()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int x)
                                 {
                                     var k = x is 1 or
                                     2 and
                                     3;

                                     return k;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(int x)
                                    {
                                        var k = x is 1
                                                  or 2
                                                  and 3;

                                        return k;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parenthesized pattern starts a chain of its own: the outer chain is wrapped by its own combinator, and the
    /// inner chain, written on one line, keeps its layout
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void ParenthesizedPatternStartsAChainOfItsOwn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int x)
                                 {
                                     var k = x is 1 or
                                     (2 or 3);

                                     return k;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(int x)
                                    {
                                        var k = x is 1
                                                  or (2 or 3);

                                        return k;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain inside an argument is laid out alike: the combinator starts its line under the <c>is</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void ChainInsideAnArgumentStartsEachCombinatorOnItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     Use(x is 1 or
                                     2);
                                 }

                                 void Use(bool b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        Use(x is 1
                                              or 2);
                                    }

                                    void Use(bool b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the lead's decision for a chain whose only line break stands next to a combinator in front of a multi-line
    /// property pattern: the whole chain counts as wrapped, so every combinator starts its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void ChainWhoseOnlyBreakIsNextToAMultiLinePropertyPatternIsWrappedAtEveryCombinator()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(string s)
                                 {
                                     var a = s is null or "" or
                                     {
                                     Length: 2
                                     };

                                     return a;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(string s)
                                    {
                                        var a = s is null
                                                  or ""
                                                  or
                                                  {
                                                      Length: 2
                                                  };

                                        return a;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example of the parenthesized pattern's content: the pattern inside the parentheses is indented
    /// four columns right of the opening parenthesis, and the combinator inside it aligns with the first pattern it combines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void SpecExampleContentOfAParenthesizedPatternIsIndentedFourColumnsRightOfTheParenthesis()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     var d = o is
                                               (
                                     int
                                     or long
                                               );

                                     return d;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(object o)
                                    {
                                        var d = o is
                                                  (
                                                      int
                                                      or long
                                                  );

                                        return d;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: in a <c>case</c> label, the brace of a multi-line property pattern without a type
    /// that stands on the line behind its combinator joins the combinator, because FMT-BREAK-022 leaves it on the line it is
    /// written on there, while behind <c>is</c> the brace keeps its own line below the combinator
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void SpecExampleBraceOfAPropertyPatternInACaseLabelJoinsItsCombinator()
    {
        // Arrange
        const string input = """
                             class P
                             {
                                 public int A { get; set; }
                             }

                             class C
                             {
                                 bool M(object o)
                                 {
                                     switch (o)
                                     {
                                         case P and
                                         {
                                         A: 1
                                         }:
                                             break;

                                         case 1 or
                                         {
                                         A: 2
                                         }:
                                             break;
                                     }

                                     return o is P and
                                     {
                                     A: 3
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class P
                                {
                                    public int A { get; set; }
                                }

                                class C
                                {
                                    bool M(object o)
                                    {
                                        switch (o)
                                        {
                                            case P
                                                 and {
                                                     A: 1
                                                 }:
                                                break;

                                            case 1
                                                 or {
                                                     A: 2
                                                 }:
                                                break;
                                        }

                                        return o is P
                                                 and
                                                 {
                                                     A: 3
                                                 };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example's statement that the same input with each brace written behind its combinator gives
    /// the same output: one tree has one layout, wherever the author broke the line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void SpecExampleBraceWrittenBehindItsCombinatorGivesTheSameOutput()
    {
        // Arrange
        const string input = """
                             class P
                             {
                                 public int A { get; set; }
                             }

                             class C
                             {
                                 bool M(object o)
                                 {
                                     switch (o)
                                     {
                                         case P and {
                                         A: 1
                                         }:
                                             break;

                                         case 1 or {
                                         A: 2
                                         }:
                                             break;
                                     }

                                     return o is P and {
                                     A: 3
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class P
                                {
                                    public int A { get; set; }
                                }

                                class C
                                {
                                    bool M(object o)
                                    {
                                        switch (o)
                                        {
                                            case P
                                                 and {
                                                     A: 1
                                                 }:
                                                break;

                                            case 1
                                                 or {
                                                     A: 2
                                                 }:
                                                break;
                                        }

                                        return o is P
                                                 and
                                                 {
                                                     A: 3
                                                 };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}