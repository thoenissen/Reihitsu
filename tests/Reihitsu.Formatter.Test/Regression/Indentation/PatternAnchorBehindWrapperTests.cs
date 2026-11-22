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
                                                int M(object o)
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

                                                void N(object o)
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
                                                   int M(object o)
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

                                                   void N(object o)
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

    #endregion // Methods
}