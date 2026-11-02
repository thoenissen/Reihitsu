using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for the alignment of an anonymous method's block in an interpolation hole of a multi-line raw
/// string literal, under the anonymous method's anchor, as a block lambda in the same position is aligned
/// </summary>
[TestClass]
public class AnonymousMethodHoleAlignmentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that an anonymous method written on one line in a hole of an aligned literal is broken and its block aligned under <c>delegate</c>, not left in column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AnonymousMethodInHoleOfAlignedLiteralIsAlignedUnderDelegate()
    {
        // Arrange
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
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 Hello {M(delegate
                                                          {
                                                              return 1;
                                                          })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: in an aligned literal, the statements of an anonymous method are separated as anywhere else, a <c>static</c> one aligns under <c>delegate</c>, an <c>async</c> one under <c>async</c>, and one inside a lambda's expression body under its own <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void SpecExampleStatementsModifiersAndExpressionLambdaBodyInAlignedLiteral()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              Hello {M(delegate { int x = 1; return x; })}
                                              S {M(static delegate { return 1; })}
                                              T {M(async delegate { return 1; })}
                                              A {M(() => M(delegate { return 1; }))}
                                              """;

                                     return 0;
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
                                                              int x = 1;

                                                              return x;
                                                          })}
                                                 S {M(static delegate
                                                             {
                                                                 return 1;
                                                             })}
                                                 T {M(async delegate
                                                      {
                                                          return 1;
                                                      })}
                                                 A {M(() => M(delegate
                                                              {
                                                                  return 1;
                                                              }))}
                                                 """;

                                        return 0;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the statements of an anonymous method in a hole start their own lines, with the blank line in front of <c>return</c>, inside the block aligned under <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void StatementsOfAnonymousMethodInHoleAreSeparatedInsideTheAlignedBlock()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              Hello {M(delegate { int x = 1; return x; })}
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
                                                              int x = 1;

                                                              return x;
                                                          })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of a <c>static</c> anonymous method in a hole aligns under <c>delegate</c>, not under <c>static</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void StaticAnonymousMethodInHoleIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              S {M(static delegate { return 1; })}
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
                                                 S {M(static delegate
                                                             {
                                                                 return 1;
                                                             })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of an <c>async</c> anonymous method in a hole aligns under <c>async</c>, not under <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AsyncAnonymousMethodInHoleIsAlignedUnderAsync()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              T {M(async delegate { return 1; })}
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
                                                 T {M(async delegate
                                                      {
                                                          return 1;
                                                      })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of a <c>static async</c> anonymous method in a hole aligns under <c>async</c>, the anchor of an async anonymous method whatever stands in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void StaticAsyncAnonymousMethodInHoleIsAlignedUnderAsync()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              S {M(static async delegate { return 1; })}
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
                                                 S {M(static async delegate
                                                             {
                                                                 return 1;
                                                             })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of an anonymous method with a parameter list in a hole aligns under <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AnonymousMethodWithParameterListInHoleIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              P {M(delegate(int x) { return x; })}
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
                                                 P {M(delegate(int x)
                                                      {
                                                          return x;
                                                      })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method inside the expression body of a lambda in a hole aligns under its own <c>delegate</c>, because only a block body places it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AnonymousMethodInExpressionLambdaBodyInHoleIsAlignedUnderItsDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              A {M(() => M(delegate { return 1; }))}
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
                                                 A {M(() => M(delegate
                                                              {
                                                                  return 1;
                                                              }))}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a block lambda inside an anonymous method in a hole is aligned to the lambda, measured after the anonymous method's block moved
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-028")]
    public void SpecExampleBlockLambdaInsideAnonymousMethodFollowsTheAlignedBlock()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              Hello {M(delegate { return M(() => { return 1; }); })}
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
                                                              return M(() =>
                                                                       {
                                                                           return 1;
                                                                       });
                                                          })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: inside the block body of a lambda in the same hole, an anonymous method's block takes the scope depth relative to the lambda's brace, and outside a hole the scope depth of the statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void SpecExampleAnonymousMethodInsideBlockLambdaBodyOfTheSameHoleAndOutsideHoleKeepScopeDepth()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              G {M(() =>
                                                   {
                                                       return M(delegate
                                                       {
                                                           return 1;
                                                       });
                                                   })}
                                              """;

                                     return M(delegate
                                     {
                                         return 1;
                                     });
                                 }
                             }
                             """";

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an anonymous method inside the block body of another anonymous method in the same hole takes the scope depth relative to the outer block's brace, while the outer block aligns under its <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AnonymousMethodInsideAnonymousMethodOfTheSameHoleTakesScopeDepthOfTheOuterBody()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              Hello {M(delegate { return M(delegate { return 1; }); })}
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
                                                              return M(delegate
                                                              {
                                                                  return 1;
                                                              });
                                                          })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method in a hole of a raw literal nested inside a block lambda of an outer hole aligns under its <c>delegate</c>, because the lambda's body does not lie in the innermost hole
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AnonymousMethodInNestedLiteralInsideBlockLambdaOfOuterHoleIsAligned()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              O {M(() =>
                                                   {
                                                       var b = $"""
                                                                I {M(delegate { return 1; })}
                                                                """;

                                                       return 0;
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
                                                 O {M(() =>
                                                      {
                                                          var b = $"""
                                                                   I {M(delegate
                                                                        {
                                                                            return 1;
                                                                        })}
                                                                   """;

                                                          return 0;
                                                      })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method in a hole of a single-line raw literal takes the scope depth of the statement, as outside a hole
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AnonymousMethodInHoleOfSingleLineLiteralTakesScopeDepth()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var b = $"""Hello {M(delegate { return 1; })}""";
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var b = $"""Hello {M(delegate
                                        {
                                            return 1;
                                        })}""";
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two anonymous method arguments in one hole each align their block under their own <c>delegate</c>, the second argument aligned with the first
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void SeveralAnonymousMethodsInOneHoleAreEachAlignedUnderTheirDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              Hello {M2(delegate { return 1; }, delegate { return 2; })}
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
                                                 Hello {M2(delegate
                                                           {
                                                               return 1;
                                                           },
                                                           delegate
                                                           {
                                                               return 2;
                                                           })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the braces of an empty anonymous method in a hole align under <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void EmptyAnonymousMethodInHoleIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              E {M(delegate { })}
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
                                                 E {M(delegate
                                                      {
                                                      })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method in a double-brace hole of a <c>$$</c> literal aligns under <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AnonymousMethodInHoleOfDollarDollarLiteralIsAlignedUnderDelegate()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $$"""
                                               Hello {{M(delegate { return 1; })}}
                                               """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $$"""
                                                  Hello {{M(delegate
                                                            {
                                                                return 1;
                                                            })}}
                                                  """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method the author wrote over several lines in a literal the formatter moves aligns under <c>delegate</c>, a comment line inside it taking the column of the statement below and an end-of-line comment staying on its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AuthorWrittenAnonymousMethodWithCommentsInMovedLiteralIsAlignedAndKeepsItsComments()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                 Hello {M(delegate
                                 {
                                     // one
                                     return 1; // two
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
                                                              // one
                                                              return 1; // two
                                                          })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment behind the opening brace of an anonymous method in a hole stays behind that brace, which aligns under <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void BlockCommentBehindTheBraceOfAnonymousMethodInHoleStaysWithTheAlignedBrace()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 int M(System.Func<int> f)
                                 {
                                     var a = $"""
                                              Hello {M(delegate { /* c */ return 1; })}
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
                                                          { /* c */
                                                              return 1;
                                                          })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method in a hole of a literal the formatter moves to the left aligns under <c>delegate</c>, measured after the move
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AnonymousMethodInHoleOfLiteralMovedLeftIsAlignedUnderDelegate()
    {
        // Arrange
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
        const string expected = """"
                                class C
                                {
                                    int M(System.Func<int> f)
                                    {
                                        var a = $"""
                                                 Hello {M(delegate
                                                          {
                                                              return 1;
                                                          })}
                                                 """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}