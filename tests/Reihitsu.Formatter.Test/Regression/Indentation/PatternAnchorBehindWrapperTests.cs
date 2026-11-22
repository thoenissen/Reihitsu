using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for the anchor of a multi-line property or parenthesized pattern behind <c>and</c>, <c>or</c>, or
/// <c>not</c> where no <c>is</c>, subpattern, or parenthesized pattern introduces it — in a switch-expression arm, in an element
/// or a slice of a list pattern, and in a <c>case</c> label: its delimiters stand under the first token of the outermost pattern
/// the wrappers lead to
/// </summary>
[TestClass]
public class PatternAnchorBehindWrapperTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A type with two properties, used as the type of property patterns
    /// </summary>
    private const string PropertyType = """
                                        class P
                                        {
                                            public int A { get; set; }

                                            public int B { get; set; }
                                        }


                                        """;

    /// <summary>
    /// Two types with two properties each, used as the types of property patterns with a type
    /// </summary>
    private const string TypedPropertyTypes = """
                                              class P
                                              {
                                                  public int A { get; set; }

                                                  public int B { get; set; }
                                              }

                                              class Q
                                              {
                                                  public int A { get; set; }

                                                  public int B { get; set; }
                                              }


                                              """;

    /// <summary>
    /// The input of the specification example for property patterns with a type and parenthesized patterns behind a wrapper
    /// </summary>
    private const string TypedAndParenthesizedInput = """
                                                      class P
                                                      {
                                                          public int A { get; set; }

                                                          public int B { get; set; }
                                                      }

                                                      class Q
                                                      {
                                                          public int A { get; set; }

                                                          public int B { get; set; }
                                                      }

                                                      class S
                                                      {
                                                          public int Length => 0;

                                                          public object this[int index] => null;

                                                          public object Slice(int start, int length)
                                                          {
                                                              return null;
                                                          }
                                                      }

                                                      class C
                                                      {
                                                          int M(object o)
                                                          {
                                                              return o switch
                                                              {
                                                                  P { A: 1,
                                                                      B: 2 } or Q { A: 1,
                                                                      B: 2 } => 1,
                                                                  null or P { A: 5,
                                                                      B: 6 } => 2,
                                                                  not Q { A: 3,
                                                                      B: 4 } => 3,
                                                                  _ => 0
                                                              };
                                                          }

                                                          int K(int i)
                                                          {
                                                              return i switch
                                                              {
                                                                  0
                                                                  or
                                                                  (1 or
                                                                  2) => 1,
                                                                  _ => 0
                                                              };
                                                          }

                                                          bool L(S a, S b)
                                                          {
                                                              return a is [1, not Q { A: 1,
                                                                  B: 2 }]
                                                                  && a is [1, .. not Q { A: 1,
                                                                  B: 2 }]
                                                                  && b is [1, not (1 or
                                                                  2)]
                                                                  && b is [1, .. not (1 or
                                                                  2)];
                                                          }

                                                          void N(object o, int i)
                                                          {
                                                              switch (o)
                                                              {
                                                                  case null or P { A: 1,
                                                                  B: 2 }:
                                                                      break;

                                                                  case not Q { A: 1,
                                                                  B: 2 }:
                                                                      break;
                                                              }

                                                              switch (i)
                                                              {
                                                                  case > 5 and (< 10 or
                                                                  > 20):
                                                                      break;

                                                                  case 3 or (4 or
                                                                  5):
                                                                      break;

                                                                  case not (1 or
                                                                  2):
                                                                      break;
                                                              }
                                                          }
                                                      }
                                                      """;

    /// <summary>
    /// The required output of the specification example for property patterns with a type and parenthesized patterns behind a
    /// wrapper
    /// </summary>
    private const string TypedAndParenthesizedExpected = """
                                                         class P
                                                         {
                                                             public int A { get; set; }

                                                             public int B { get; set; }
                                                         }

                                                         class Q
                                                         {
                                                             public int A { get; set; }

                                                             public int B { get; set; }
                                                         }

                                                         class S
                                                         {
                                                             public int Length => 0;

                                                             public object this[int index] => null;

                                                             public object Slice(int start, int length)
                                                             {
                                                                 return null;
                                                             }
                                                         }

                                                         class C
                                                         {
                                                             int M(object o)
                                                             {
                                                                 return o switch
                                                                        {
                                                                            P
                                                                            {
                                                                                A: 1,
                                                                                B: 2
                                                                            }
                                                                            or Q
                                                                            {
                                                                                A: 1,
                                                                                B: 2
                                                                            } => 1,
                                                                            null
                                                                            or P
                                                                            {
                                                                                A: 5,
                                                                                B: 6
                                                                            } => 2,
                                                                            not Q
                                                                            {
                                                                                A: 3,
                                                                                B: 4
                                                                            } => 3,
                                                                            _ => 0
                                                                        };
                                                             }

                                                             int K(int i)
                                                             {
                                                                 return i switch
                                                                        {
                                                                            0
                                                                            or
                                                                            (
                                                                                1
                                                                                or 2
                                                                            ) => 1,
                                                                            _ => 0
                                                                        };
                                                             }

                                                             bool L(S a, S b)
                                                             {
                                                                 return a is [
                                                                                 1,
                                                                                 not Q
                                                                                 {
                                                                                     A: 1,
                                                                                     B: 2
                                                                                 }
                                                                             ]
                                                                        && a is [
                                                                                    1,
                                                                                    .. not Q
                                                                                       {
                                                                                           A: 1,
                                                                                           B: 2
                                                                                       }
                                                                                ]
                                                                        && b is [
                                                                                    1,
                                                                                    not
                                                                                    (
                                                                                        1
                                                                                        or 2
                                                                                    )
                                                                                ]
                                                                        && b is [
                                                                                    1,
                                                                                    .. not
                                                                                       (
                                                                                           1
                                                                                           or 2
                                                                                       )
                                                                                ];
                                                             }

                                                             void N(object o, int i)
                                                             {
                                                                 switch (o)
                                                                 {
                                                                     case null
                                                                          or P {
                                                                              A: 1,
                                                                              B: 2
                                                                          }:
                                                                         break;

                                                                     case not Q {
                                                                              A: 1,
                                                                              B: 2
                                                                          }:
                                                                         break;
                                                                 }

                                                                 switch (i)
                                                                 {
                                                                     case > 5
                                                                          and
                                                                          (
                                                                              < 10
                                                                              or > 20
                                                                          ):
                                                                         break;

                                                                     case 3
                                                                          or
                                                                          (
                                                                              4
                                                                              or 5
                                                                          ):
                                                                         break;

                                                                     case not
                                                                          (
                                                                              1
                                                                              or 2
                                                                          ):
                                                                         break;
                                                                 }
                                                             }
                                                         }
                                                         """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: behind <c>and</c> and <c>not</c> in a switch-expression arm, in an element of a list
    /// pattern, and in a <c>case</c> label, the braces of a property pattern stand under the first token of the arm's, the
    /// element's, or the label's pattern
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void SpecExampleBracesBehindAndOrNotStandUnderTheOutermostPatternsFirstToken()
    {
        // Arrange
        const string input = PropertyType + """
                                            class C
                                            {
                                                int M(P o)
                                                {
                                                    return o switch
                                                    {
                                                        P and { A: 1,
                                                            B: 2 } => 1,
                                                        not { A: 3,
                                                            B: 4 } => 2,
                                                        _ => 0
                                                    };
                                                }

                                                bool L(object[] a)
                                                {
                                                    return a is [P and { A: 1,
                                                        B: 2 }];
                                                }

                                                void N(P o)
                                                {
                                                    switch (o)
                                                    {
                                                        case P and {
                                                        A: 1
                                                        }:
                                                            break;

                                                        case not {
                                                        A: 2
                                                        }:
                                                            break;

                                                        case P and not {
                                                        A: 3
                                                        }:
                                                            break;
                                                    }
                                                }
                                            }
                                            """;
        const string expected = PropertyType + """
                                               class C
                                               {
                                                   int M(P o)
                                                   {
                                                       return o switch
                                                              {
                                                                  P
                                                                  and
                                                                  {
                                                                      A: 1,
                                                                      B: 2
                                                                  } => 1,
                                                                  not
                                                                  {
                                                                      A: 3,
                                                                      B: 4
                                                                  } => 2,
                                                                  _ => 0
                                                              };
                                                   }

                                                   bool L(object[] a)
                                                   {
                                                       return a is [
                                                                       P
                                                                       and
                                                                       {
                                                                           A: 1,
                                                                           B: 2
                                                                       }
                                                                   ];
                                                   }

                                                   void N(P o)
                                                   {
                                                       switch (o)
                                                       {
                                                           case P
                                                                and {
                                                                    A: 1
                                                                }:
                                                               break;

                                                           case not {
                                                                    A: 2
                                                                }:
                                                               break;

                                                           case P
                                                                and not {
                                                                    A: 3
                                                                }:
                                                               break;
                                                       }
                                                   }
                                               }
                                               """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example for typed and parenthesized patterns: behind <c>and</c>, <c>or</c>, and <c>not</c> in
    /// a switch-expression arm, in an element or a slice of a list pattern, and in a <c>case</c> label, the braces of a property
    /// pattern with a type and the parentheses of a parenthesized pattern stand under the first token of the arm's, the
    /// element's, or the label's pattern
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void SpecExampleTypedAndParenthesizedPatternsBehindAndOrNotStandUnderTheOutermostPatternsFirstToken()
    {
        // Act & Assert
        AssertRuleResult(TypedAndParenthesizedInput, TypedAndParenthesizedExpected);
    }

    /// <summary>
    /// Verifies that the braces of a property pattern behind <c>or</c> in a switch-expression arm stand under the arm's first
    /// token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void BracesBehindOrInASwitchArmStandUnderTheArmsFirstToken()
    {
        // Arrange
        const string input = PropertyType + """
                                            class C
                                            {
                                                int M(object o)
                                                {
                                                    return o switch
                                                    {
                                                        1 or { A: 1,
                                                            B: 2 } => 1,
                                                        _ => 0
                                                    };
                                                }
                                            }
                                            """;
        const string expected = PropertyType + """
                                               class C
                                               {
                                                   int M(object o)
                                                   {
                                                       return o switch
                                                              {
                                                                  1
                                                                  or
                                                                  {
                                                                      A: 1,
                                                                      B: 2
                                                                  } => 1,
                                                                  _ => 0
                                                              };
                                                   }
                                               }
                                               """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line parenthesized pattern behind <c>not</c> in a switch-expression arm stands under the
    /// <c>not</c>, its content four columns right of its opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void ParenthesizedPatternBehindNotInASwitchArmStandsUnderTheNot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(object o)
                                 {
                                     return o switch
                                     {
                                         not (1 or
                                             2) => 2,
                                         _ => 0
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(object o)
                                    {
                                        return o switch
                                               {
                                                   not
                                                   (
                                                       1
                                                       or 2
                                                   ) => 2,
                                                   _ => 0
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the braces of a property pattern behind <c>not</c> in the slice of a list pattern stand under the
    /// <c>not</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void BracesBehindNotInASliceStandUnderTheNot()
    {
        // Arrange
        const string input = PropertyType + """
                                            class C
                                            {
                                                bool L(object[] a)
                                                {
                                                    return a is [1, .. not { A: 1,
                                                        B: 2 }];
                                                }
                                            }
                                            """;
        const string expected = PropertyType + """
                                               class C
                                               {
                                                   bool L(object[] a)
                                                   {
                                                       return a is [
                                                                       1,
                                                                       .. not
                                                                          {
                                                                              A: 1,
                                                                              B: 2
                                                                          }
                                                                   ];
                                                   }
                                               }
                                               """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the braces of a property pattern behind <c>not</c> in an element of a list pattern in a <c>case</c> label,
    /// which the label does not introduce, stand under the <c>not</c> on a line of their own
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void BracesBehindNotInAListPatternElementOfACaseLabelStandUnderTheNot()
    {
        // Arrange
        const string input = """
                             class P
                             {
                                 public int A { get; set; }
                             }

                             class C
                             {
                                 void N(object o)
                                 {
                                     switch (o)
                                     {
                                         case [not {
                                         A: 4
                                         }]:
                                             break;
                                     }
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
                                    void N(object o)
                                    {
                                        switch (o)
                                        {
                                            case [
                                                     not
                                                     {
                                                         A: 4
                                                     }
                                                 ]:
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the wrappers: a property pattern with a type in a <c>case</c> label and in a switch arm, a
    /// property pattern that starts a switch arm, and a property pattern inside a parenthesized pattern of a <c>case</c> label
    /// keep the anchors they have without a wrapper
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void PropertyPatternsWithoutAWrapperKeepTheirAnchors()
    {
        // Arrange
        const string input = PropertyType + """
                                            class C
                                            {
                                                int M(object o)
                                                {
                                                    switch (o)
                                                    {
                                                        case P {
                                                                 A: 1
                                                             }:
                                                            break;

                                                        case
                                                            (
                                                                P
                                                                and
                                                                {
                                                                    A: 3
                                                                }
                                                            ):
                                                            break;
                                                    }

                                                    return o switch
                                                           {
                                                               P
                                                               {
                                                                   A: 3,
                                                                   B: 4
                                                               } => 3,
                                                               {
                                                                   A: 5,
                                                                   B: 6
                                                               } => 4,
                                                               _ => 0
                                                           };
                                                }
                                            }
                                            """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the braces of a property pattern with a type behind <c>or</c> in a switch-expression arm stand under
    /// the arm's first token, not under their type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void TypedBracesBehindOrInASwitchArmStandUnderTheArmsFirstToken()
    {
        // Arrange
        const string input = TypedPropertyTypes + """
                                                  class C
                                                  {
                                                      int M(object o)
                                                      {
                                                          return o switch
                                                          {
                                                              P { A: 1,
                                                                  B: 2 } or Q { A: 1,
                                                                  B: 2 } => 1,
                                                              _ => 0
                                                          };
                                                      }
                                                  }
                                                  """;
        const string expected = TypedPropertyTypes + """
                                                     class C
                                                     {
                                                         int M(object o)
                                                         {
                                                             return o switch
                                                                    {
                                                                        P
                                                                        {
                                                                            A: 1,
                                                                            B: 2
                                                                        }
                                                                        or Q
                                                                        {
                                                                            A: 1,
                                                                            B: 2
                                                                        } => 1,
                                                                        _ => 0
                                                                    };
                                                         }
                                                     }
                                                     """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the braces of a property pattern with a type behind <c>not</c> in a switch-expression arm stand under
    /// the <c>not</c>, not under their type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void TypedBracesBehindNotInASwitchArmStandUnderTheNot()
    {
        // Arrange
        const string input = TypedPropertyTypes + """
                                                  class C
                                                  {
                                                      int M(object o)
                                                      {
                                                          return o switch
                                                          {
                                                              not Q { A: 3,
                                                                  B: 4 } => 2,
                                                              _ => 0
                                                          };
                                                      }
                                                  }
                                                  """;
        const string expected = TypedPropertyTypes + """
                                                     class C
                                                     {
                                                         int M(object o)
                                                         {
                                                             return o switch
                                                                    {
                                                                        not Q
                                                                        {
                                                                            A: 3,
                                                                            B: 4
                                                                        } => 2,
                                                                        _ => 0
                                                                    };
                                                         }
                                                     }
                                                     """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the braces of a property pattern with a type behind <c>null or</c> in a switch-expression arm stand
    /// under the <c>null</c>, not under their type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void TypedBracesBehindOrAfterNullInASwitchArmStandUnderTheNull()
    {
        // Arrange
        const string input = TypedPropertyTypes + """
                                                  class C
                                                  {
                                                      int M(object o)
                                                      {
                                                          return o switch
                                                          {
                                                              null or P { A: 5,
                                                                  B: 6 } => 3,
                                                              _ => 0
                                                          };
                                                      }
                                                  }
                                                  """;
        const string expected = TypedPropertyTypes + """
                                                     class C
                                                     {
                                                         int M(object o)
                                                         {
                                                             return o switch
                                                                    {
                                                                        null
                                                                        or P
                                                                        {
                                                                            A: 5,
                                                                            B: 6
                                                                        } => 3,
                                                                        _ => 0
                                                                    };
                                                         }
                                                     }
                                                     """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line parenthesized pattern behind <c>or</c> in a switch-expression arm stands under the arm's
    /// first token, its content four columns right of its opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void ParenthesizedPatternBehindOrInASwitchArmStandsUnderTheArmsFirstToken()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int K(int i)
                                 {
                                     return i switch
                                     {
                                         0
                                         or
                                         (1 or
                                         2) => 1,
                                         _ => 0
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int K(int i)
                                    {
                                        return i switch
                                               {
                                                   0
                                                   or
                                                   (
                                                       1
                                                       or 2
                                                   ) => 1,
                                                   _ => 0
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the braces of a property pattern with a type behind <c>not</c> in an element of a list pattern stand
    /// under the <c>not</c>, not under their type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void TypedBracesBehindNotInAListPatternElementStandUnderTheNot()
    {
        // Arrange
        const string input = TypedPropertyTypes + """
                                                  class C
                                                  {
                                                      bool L(object[] a)
                                                      {
                                                          return a is [1, not Q { A: 1,
                                                              B: 2 }];
                                                      }
                                                  }
                                                  """;
        const string expected = TypedPropertyTypes + """
                                                     class C
                                                     {
                                                         bool L(object[] a)
                                                         {
                                                             return a is [
                                                                             1,
                                                                             not Q
                                                                             {
                                                                                 A: 1,
                                                                                 B: 2
                                                                             }
                                                                         ];
                                                         }
                                                     }
                                                     """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the braces of a property pattern with a type behind <c>not</c> in the slice of a list pattern stand
    /// under the <c>not</c>, not under their type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void TypedBracesBehindNotInASliceStandUnderTheNot()
    {
        // Arrange
        const string input = TypedPropertyTypes + """
                                                  class C
                                                  {
                                                      bool L(object[] a)
                                                      {
                                                          return a is [1, .. not Q { A: 1,
                                                              B: 2 }];
                                                      }
                                                  }
                                                  """;
        const string expected = TypedPropertyTypes + """
                                                     class C
                                                     {
                                                         bool L(object[] a)
                                                         {
                                                             return a is [
                                                                             1,
                                                                             .. not Q
                                                                                {
                                                                                    A: 1,
                                                                                    B: 2
                                                                                }
                                                                         ];
                                                         }
                                                     }
                                                     """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line parenthesized pattern behind <c>not</c> in an element of a list pattern stands under the
    /// <c>not</c>, its content four columns right of its opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void ParenthesizedPatternBehindNotInAListPatternElementStandsUnderTheNot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool L(int[] a)
                                 {
                                     return a is [1, not (1 or
                                         2)];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool L(int[] a)
                                    {
                                        return a is [
                                                        1,
                                                        not
                                                        (
                                                            1
                                                            or 2
                                                        )
                                                    ];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line parenthesized pattern behind <c>not</c> in the slice of a list pattern stands under the
    /// <c>not</c>, its content four columns right of its opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void ParenthesizedPatternBehindNotInASliceStandsUnderTheNot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool L(int[] a)
                                 {
                                     return a is [1, .. not (1 or
                                         2)];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool L(int[] a)
                                    {
                                        return a is [
                                                        1,
                                                        .. not
                                                           (
                                                               1
                                                               or 2
                                                           )
                                                    ];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the braces of a property pattern with a type behind <c>not</c> in a <c>case</c> label are laid out
    /// from the <c>not</c>, not from their type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void TypedBracesBehindNotInACaseLabelStandUnderTheNot()
    {
        // Arrange
        const string input = TypedPropertyTypes + """
                                                  class C
                                                  {
                                                      void N(object o)
                                                      {
                                                          switch (o)
                                                          {
                                                              case not Q { A: 1,
                                                              B: 2 }:
                                                                  break;
                                                          }
                                                      }
                                                  }
                                                  """;
        const string expected = TypedPropertyTypes + """
                                                     class C
                                                     {
                                                         void N(object o)
                                                         {
                                                             switch (o)
                                                             {
                                                                 case not Q {
                                                                          A: 1,
                                                                          B: 2
                                                                      }:
                                                                     break;
                                                             }
                                                         }
                                                     }
                                                     """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the braces of a property pattern with a type behind <c>null or</c> in a <c>case</c> label are laid out
    /// from the <c>null</c>, not from their type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void TypedBracesBehindOrAfterNullInACaseLabelStandUnderTheNull()
    {
        // Arrange
        const string input = TypedPropertyTypes + """
                                                  class C
                                                  {
                                                      void N(object o)
                                                      {
                                                          switch (o)
                                                          {
                                                              case null or P { A: 1,
                                                              B: 2 }:
                                                                  break;
                                                          }
                                                      }
                                                  }
                                                  """;
        const string expected = TypedPropertyTypes + """
                                                     class C
                                                     {
                                                         void N(object o)
                                                         {
                                                             switch (o)
                                                             {
                                                                 case null
                                                                      or P {
                                                                          A: 1,
                                                                          B: 2
                                                                      }:
                                                                     break;
                                                             }
                                                         }
                                                     }
                                                     """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line parenthesized pattern behind <c>not</c> in a <c>case</c> label stands under the
    /// <c>not</c>, not one level right of the <c>case</c> keyword
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void ParenthesizedPatternBehindNotInACaseLabelStandsUnderTheNot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void N(int o)
                                 {
                                     switch (o)
                                     {
                                         case not (1 or
                                         2):
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void N(int o)
                                    {
                                        switch (o)
                                        {
                                            case not
                                                 (
                                                     1
                                                     or 2
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
    /// Verifies that a multi-line parenthesized pattern behind <c>and</c> in a <c>case</c> label stands under the first
    /// token of the label's pattern, not one level right of the <c>case</c> keyword
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void ParenthesizedPatternBehindAndInACaseLabelStandsUnderTheLabelPatternsFirstToken()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void N(int o)
                                 {
                                     switch (o)
                                     {
                                         case > 5 and (< 10 or
                                         > 20):
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void N(int o)
                                    {
                                        switch (o)
                                        {
                                            case > 5
                                                 and
                                                 (
                                                     < 10
                                                     or > 20
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
    /// Verifies that a multi-line parenthesized pattern behind <c>or</c> in a <c>case</c> label stands under the first
    /// token of the label's pattern, not one level right of the <c>case</c> keyword
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-026")]
    public void ParenthesizedPatternBehindOrInACaseLabelStandsUnderTheLabelPatternsFirstToken()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void N(int o)
                                 {
                                     switch (o)
                                     {
                                         case 3 or (4 or
                                         5):
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void N(int o)
                                    {
                                        switch (o)
                                        {
                                            case 3
                                                 or
                                                 (
                                                     4
                                                     or 5
                                                 ):
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}