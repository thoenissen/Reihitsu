using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.RawStringAlignment;

/// <summary>
/// Tests for the lines inside the interpolation holes of multi-line interpolated raw strings, and for literals and comments nested in such a hole, through the whole pipeline over two passes
/// </summary>
[TestClass]
public class RawStringInterpolationHoleTests : FormatterTestsBase
{
    #region Hole lines

    /// <summary>
    /// Verifies the spec example: in a misaligned interpolated raw string, the lines of a multi-line hole that the indentation phase would give the block column move only by the literal's shift, and a second pass keeps them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-INDENT-011")]
    public void MisalignedLiteralMovesHoleLinesAtTheBlockColumnOnlyWithTheLiteral()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "x";
                                     var a = $"""
                                 Hello {
                                     name
                                 }
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = $"""
                                                 Hello {
                                                     name
                                                 }
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: in an aligned interpolated raw string, the lines of a multi-line hole keep their columns instead of taking the block column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-006")]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-INDENT-011")]
    public void AlignedLiteralKeepsTheColumnsOfItsHoleLines()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "x";
                                     var a = $"""
                                              Hello {
                                                      name
                                              }
                                              """;
                                 }
                             }
                             """";

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the spec example: a wrapped argument inside a hole keeps its alignment to the opening parenthesis, measured after the literal moved
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void WrappedArgumentInHoleKeepsItsAlignmentAfterTheShift()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                 Hello {string.Concat("a",
                                     "b")}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Hello {string.Concat("a",
                                                                      "b")}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: for a literal inside a lambda body, the hole lines take neither the block column inside the body nor a column the lambda sets, and move only with the literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-INDENT-011")]
    public void LiteralInsideLambdaBodyMovesHoleLinesOnlyWithTheLiteral()
    {
        const string input = """"
                             class C
                             {
                                 void M(System.Action f)
                                 {
                                     var name = "x";

                                     M(() =>
                                       {
                                           var s = $"""
                                       Hello {
                                           name
                                       }
                                       """;
                                       });
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M(System.Action f)
                                    {
                                        var name = "x";

                                        M(() =>
                                          {
                                              var s = $"""
                                                       Hello {
                                                           name
                                                       }
                                                       """;
                                          });
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a comment that starts a hole line takes the column of the token below it, after that token moved with the literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void CommentStartingAHoleLineTakesTheColumnOfTheTokenBelowItAfterTheShift()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "x";
                                     var a = $"""
                                 Hello {
                                 // c
                                     name
                                 }
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = $"""
                                                 Hello {
                                                     // c
                                                     name
                                                 }
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that in an aligned literal a comment starting a hole line moves to the column of the token below it, while that token and the hole's closing brace keep their columns
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-006")]
    [Requirement("FMT-RAW-007")]
    public void CommentInAlignedLiteralMovesToTheColumnOfTheTokenBelowIt()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "x";
                                     var a = $"""
                                              Hello {
                                                      // c
                                                  name
                                              }
                                              """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = $"""
                                                 Hello {
                                                     // c
                                                     name
                                                 }
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment in front of the closing brace of a hole ends one level deeper than that brace, measured after the brace moved with the literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void CommentInFrontOfTheClosingBraceOfAHoleIsIndentedOneLevelDeeperThanTheBrace()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "x";
                                     var a = $"""
                                 Hello {
                                     name

                                 // c
                                 }
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = $"""
                                                 Hello {
                                                     name

                                                     // c
                                                 }
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the lines of a multi-line hole of a double-dollar interpolated raw string move only with the literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void DoubleDollarLiteralMovesHoleLinesOnlyWithTheLiteral()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "x";
                                     var a = $$"""
                                 {"a": {{
                                     name
                                 }}}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = $$"""
                                                  {"a": {{
                                                      name
                                                  }}}
                                                  """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a hole line holding both the expression and the hole's closing brace moves only with the literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void HoleClosedOnTheLineOfItsExpressionMovesOnlyWithTheLiteral()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "x";
                                     var a = $"""
                                 Hello {
                                     name}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = $"""
                                                 Hello {
                                                     name}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that hole lines started by the comma of an alignment clause and by the colon of a format clause move only with the literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void AlignmentAndFormatClauseLinesMoveOnlyWithTheLiteral()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1.5;
                                     var a = $"""
                                 Value {x
                                     ,10
                                     :N2}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1.5;
                                        var a = $"""
                                                 Value {x
                                                     ,10
                                                     :N2}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that when the literal moves to the left, its hole lines move left by the same number of columns
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void LeftwardShiftMovesHoleLinesOnlyWithTheLiteral()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "x";
                                     var a = $"""
                                                     Hello {
                                                             name
                                                     }
                                                     """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = $"""
                                                 Hello {
                                                         name
                                                 }
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the hole lines of a literal passed as an argument are left to the hole and move only with the literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-INDENT-013")]
    public void LiteralAsArgumentMovesHoleLinesOnlyWithTheLiteral()
    {
        const string input = """"
                             class C
                             {
                                 void M(string s)
                                 {
                                     var name = "x";

                                     M($"""
                                 Hello {
                                     name
                                 }
                                 """);
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M(string s)
                                    {
                                        var name = "x";

                                        M($"""
                                           Hello {
                                               name
                                           }
                                           """);
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the hole lines of a literal that is an element of an array initializer take neither the block column nor a column the initializer sets, and move only with the literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void LiteralInsideArrayInitializerMovesHoleLinesOnlyWithTheLiteral()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "x";
                                     var list = new[]
                                                {
                                                    $"""
                                 Hello {
                                     name
                                 }
                                 """
                                                };
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var list = new[]
                                                   {
                                                       $"""
                                                        Hello {
                                                            name
                                                        }
                                                        """
                                                   };
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that in an aligned literal a wrapped argument inside a hole that is out of alignment is aligned to the opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void WrappedArgumentInHoleOfAlignedLiteralIsRealignedToTheParenthesis()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                              Hello {string.Concat("a",
                                                  "b")}
                                              """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Hello {string.Concat("a",
                                                                      "b")}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a binary operator starting a hole line aligns to its left operand inside the hole, measured after the literal moved
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void WrappedBinaryOperatorInHoleAlignsToItsLeftOperandAfterTheShift()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;
                                     var a = $"""
                                 Sum {x
                                     + 1}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;
                                        var a = $"""
                                                 Sum {x
                                                      + 1}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the question mark and colon of a conditional starting hole lines are indented one level deeper than its condition inside the hole, measured after the literal moved
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void WrappedConditionalInHoleIsIndentedFromItsConditionAfterTheShift()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var c = true;
                                     var a = $"""
                                 Value {(c
                                     ? 1
                                     : 2)}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var c = true;
                                        var a = $"""
                                                 Value {(c
                                                             ? 1
                                                             : 2)}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the tabs in front of a hole line of an aligned literal are replaced by four spaces each, and the line is not re-indented otherwise
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void TabInFrontOfHoleLineOfAlignedLiteralBecomesFourSpaces()
    {
        const string source = """"
                              class C
                              {
                                  void M()
                                  {
                                      var name = "x";
                                      var a = $"""
                                               Hello {
                              →→→→         name
                                               }
                                               """;
                                  }
                              }
                              """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = $"""
                                                 Hello {
                                                         name
                                                 }
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(source.Replace("→", "\t"), expected);
    }

    /// <summary>
    /// Verifies that the lines an earlier phase breaks inside a hole, with no alignment placing them, are left unchanged by a second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-004")]
    public void AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass()
    {
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 Hello {M(delegate { return 1; })}
                                 """;
                                 }
                             }
                             """";

        AssertSecondPassIsStable(input);
    }

    /// <summary>
    /// Verifies that a block lambda broken onto several lines inside a hole of an aligned literal is aligned to the lambda and stays so on a second pass
    /// </summary>
    [TestMethod]
    public void BlockLambdaBrokenInsideHoleOfAlignedLiteralKeepsItsAlignment()
    {
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              Hello {M(() => { return 1; })}
                                              """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 Hello {M(() =>
                                                          {
                                                              return 1;
                                                          })}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a nested raw string whose opening quotes start a hole line moves with the outer literal, and its own lines are aligned under those quotes in the same pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-RAW-008")]
    public void NestedLiteralOpeningAHoleLineMovesWithTheOuterLiteralAndIsAligned()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                 Outer {
                                     """
                                     inner
                                     """}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Outer {
                                                     """
                                                     inner
                                                     """}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment above a tab-indented hole line of an aligned literal takes the column that line has after its tab becomes four spaces, and a second pass keeps it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void CommentAboveTabIndentedHoleLineTakesItsColumnAfterTheTabReplacement()
    {
        const string source = """"
                              class C
                              {
                                  void M()
                                  {
                                      var name = "x";
                                      var a = $"""
                                               Hello {
                                               // c
                                               →name
                                               }
                                               """;
                                  }
                              }
                              """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = $"""
                                                 Hello {
                                                     // c
                                                     name
                                                 }
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(source.Replace("→", "\t"), expected);
    }

    /// <summary>
    /// Verifies that a wrapped argument anchored on a tab-indented hole line of an aligned literal is aligned to the column the anchor has after its tab becomes four spaces, and a second pass keeps it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void ArgumentAnchoredOnATabIndentedHoleLineAlignsAfterTheTabReplacement()
    {
        const string source = """"
                              class C
                              {
                                  void M()
                                  {
                                      var a = $"""
                                               Hello {
                                               →string.Concat("a",
                                               "b")}
                                               """;
                                  }
                              }
                              """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Hello {
                                                     string.Concat("a",
                                                                   "b")}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(source.Replace("→", "\t"), expected);
    }

    /// <summary>
    /// Verifies that both lines of a two-line documentation comment that starts hole lines take the column of the token below them after it moved with the literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void DocumentationCommentLinesInHoleMoveWithTheTokenBelowThem()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "x";
                                     var a = $"""
                                 Hello {
                                     /// first
                                     /// second
                                     name
                                 }
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = $"""
                                                 Hello {
                                                     /// first
                                                     /// second
                                                     name
                                                 }
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a constraint clause of a local function in an anonymous-method body inside a hole of an aligned literal is indented one level deeper than its declaration's line, which keeps its column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-INDENT-017")]
    public void ConstraintClauseInHoleOfAlignedLiteralIsIndentedFromItsDeclaration()
    {
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              Hello {M(delegate
                                              {
                                                  void L<T>()
                                                  where T : class
                                                  {
                                                  }

                                                  return 1;
                                              })}
                                              """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 Hello {M(delegate
                                                 {
                                                     void L<T>()
                                                         where T : class
                                                     {
                                                     }

                                                     return 1;
                                                 })}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a constraint clause of a local function in an anonymous-method body inside a hole of a misaligned literal is indented one level deeper than its declaration's line after that line moved with the literal, and a second pass keeps it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-INDENT-017")]
    public void ConstraintClauseInHoleOfMisalignedLiteralIsIndentedFromItsDeclarationAfterTheShift()
    {
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 Hello {M(delegate
                                 {
                                     void L<T>()
                                         where T : class
                                     {
                                     }

                                     return 1;
                                 })}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 Hello {M(delegate
                                                 {
                                                     void L<T>()
                                                         where T : class
                                                     {
                                                     }

                                                     return 1;
                                                 })}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped argument anchored on a hole line of an aligned literal, where a tab and a comment precede the anchor's call, is aligned to the column the anchor has after the tab becomes four spaces, and a second pass keeps it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    public void ArgumentAnchoredBehindATabIndentedCommentOnAHoleLineAlignsAfterTheTabReplacement()
    {
        const string source = """"
                              class C
                              {
                                  void M()
                                  {
                                      var a = $"""
                                               Hello {
                                               →/* c */ string.Concat("a",
                                               "b")}
                                               """;
                                  }
                              }
                              """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Hello {
                                                     /* c */ string.Concat("a",
                                                                           "b")}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(source.Replace("→", "\t"), expected);
    }

    /// <summary>
    /// Verifies that a nested raw string whose opening quotes start a tab-indented hole line of an aligned literal is aligned under the column those quotes have after the tab becomes four spaces, and a second pass keeps it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-RAW-008")]
    public void NestedLiteralOpeningATabIndentedHoleLineOfAlignedLiteralIsAlignedAfterTheTabReplacement()
    {
        const string source = """"
                              class C
                              {
                                  void M()
                                  {
                                      var a = $"""
                                               Outer {
                                               →"""
                                                   inner
                                                   """}
                                               """;
                                  }
                              }
                              """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Outer {
                                                     """
                                                     inner
                                                     """}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(source.Replace("→", "\t"), expected);
    }

    /// <summary>
    /// Verifies that a nested raw string whose opening quotes start a tab-indented hole line of a misaligned literal moves with the outer literal and is aligned under the column those quotes have after the tab becomes four spaces, and a second pass keeps it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-RAW-008")]
    public void NestedLiteralOpeningATabIndentedHoleLineOfMisalignedLiteralIsAlignedAfterTheTabReplacement()
    {
        const string source = """"
                              class C
                              {
                                  void M()
                                  {
                                      var a = $"""
                                  Outer {
                                  →"""
                                      inner
                                      """}
                                  """;
                                  }
                              }
                              """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Outer {
                                                     """
                                                     inner
                                                     """}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(source.Replace("→", "\t"), expected);
    }

    #endregion // Hole lines

    #region Nested raw strings

    /// <summary>
    /// Verifies the spec example: a raw string nested in a hole of a misaligned literal is aligned under its opening quotes in the same pass as the outer literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-008")]
    public void NestedRawStringIsAlignedInTheSamePassAsTheOuterLiteral()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                 Outer {"""
                                     inner
                                     """}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Outer {"""
                                                        inner
                                                        """}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: when the outer literal is already aligned, only the nested raw string moves
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-008")]
    public void NestedRawStringOfAlignedLiteralIsAligned()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                              Outer {"""
                                                  inner
                                                  """}
                                              """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Outer {"""
                                                        inner
                                                        """}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that raw strings nested three deep, the middle one interpolated, are all aligned in the same pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-008")]
    public void RawStringsNestedThreeDeepAreAlignedInOnePass()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                 A {$"""
                                     B {"""
                                         inner
                                         """}
                                     """}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 A {$"""
                                                     B {"""
                                                        inner
                                                        """}
                                                     """}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a UTF-8 raw string nested in a hole of a misaligned literal is aligned in the same pass as the outer literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-008")]
    public void NestedUtf8RawStringIsAlignedInTheSamePassAsTheOuterLiteral()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                 Outer {"""
                                     inner
                                     """u8.Length}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Outer {"""
                                                        inner
                                                        """u8.Length}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped argument whose opening parenthesis stands on the closing line of a raw string nested in the hole is aligned to it after both literals moved, and a second pass keeps it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-RAW-008")]
    public void ArgumentAnchoredOnTheClosingLineOfANestedLiteralKeepsItsAlignment()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                 Outer {"""
                                     inner
                                     """ + string.Concat("a",
                                         "b")}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Outer {"""
                                                        inner
                                                        """ + string.Concat("a",
                                                                            "b")}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    #endregion // Nested raw strings

    #region Nested text

    /// <summary>
    /// Verifies the spec example: the continuation line of a verbatim string nested in a hole stays in its column when the outer literal moves
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-009")]
    public void ContinuationLineOfVerbatimStringInHoleStaysAsWritten()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                 Hello {@"a
                             b"}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Hello {@"a
                                b"}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: the second line of a comment nested in a hole stays in its column when the outer literal moves
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-009")]
    public void FollowingLineOfCommentInHoleStaysAsWritten()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "x";
                                     var a = $"""
                                 Hello {name /* a
                               b */
                                 }
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = $"""
                                                 Hello {name /* a
                                  b */
                                                 }
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the continuation line of an interpolated verbatim string nested in a hole stays in its column when the outer literal moves
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-009")]
    public void ContinuationLineOfInterpolatedVerbatimStringInHoleStaysAsWritten()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "x";
                                     var a = $"""
                                 Hello {$@"a
                             b{name}"}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "x";
                                        var a = $"""
                                                 Hello {$@"a
                                b{name}"}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped argument whose opening parenthesis stands on the kept continuation line of a verbatim string nested in the hole is aligned to it, and a second pass keeps it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-007")]
    [Requirement("FMT-RAW-009")]
    public void ArgumentAnchoredOnTheContinuationLineOfAVerbatimStringKeepsItsAlignment()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = $"""
                                 Hello {@"a
                             b" + string.Concat("a",
                                 "b")}
                                 """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = $"""
                                                 Hello {@"a
                                b" + string.Concat("a",
                                                   "b")}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    #endregion // Nested text
}