using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for continuation lines that no alignment requirement places: such a line is indented four columns right
/// of the column at which the line of the construct it continues begins, measured where the indentation phase puts that line;
/// lines that block indentation places by their scope are no continuation lines and keep their columns
/// </summary>
[TestClass]
public class ContinuationLineIndentationTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: an operand kept on its own line by a comment behind a parenthesis continues the
    /// statement, and an arm's expression kept on its own line behind the <c>=&gt;</c> continues the arm, also where it starts an
    /// expression of its own
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void SpecExampleOperandAndArmExpressionKeptOnTheirOwnLineAreIndentedOneLevel()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x, int y)
                                 {
                                     x = ( // c
                                     y + 3);

                                     var s = y switch
                                             {
                                                 1 =>
                                                 3 + 4,
                                                 _ => 0
                                             };

                                     return x + s;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x, int y)
                                    {
                                        x = ( // c
                                            y + 3);

                                        var s = y switch
                                                {
                                                    1 =>
                                                        3 + 4,
                                                    _ => 0
                                                };

                                        return x + s;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a continuation inside an element of a collection expression is indented from the line
    /// of the construct it continues, which starts inside the element, not from the statement's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void SpecExampleContinuationInsideACollectionElementIsIndentedFromTheElementsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int y)
                                 {
                                     int[] z = [
                                                   1,
                                                   ( // c
                                     y + 2)
                                               ];

                                     return z.Length;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int y)
                                    {
                                        int[] z = [
                                                      1,
                                                      ( // c
                                                          y + 2)
                                                  ];

                                        return z.Length;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a later declarator continues its declaration in a documented field that stays whole,
    /// behind an <c>#endif</c> in an event field, and in a local declaration, and the declarator in the disabled text keeps its
    /// column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void SpecExampleLaterDeclaratorsAreIndentedOneLevel()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 /// <summary>
                                 /// Q
                                 /// </summary>
                                 public int Q,
                                 R;

                                 public event Action A,
                             #if X
                                     B,
                             #endif
                                 D;

                                 void M()
                                 {
                                     int a = 1,
                                     b = 2;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    /// <summary>
                                    /// Q
                                    /// </summary>
                                    public int Q,
                                        R;

                                    public event Action A,
                                #if X
                                        B,
                                #endif
                                        D;

                                    void M()
                                    {
                                        int a = 1,
                                            b = 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the reported scenario: a declarator written behind an <c>#endif</c> in an event field and in a local declaration
    /// is indented one level as a continuation of its declaration, as without the directive, and the declarators in the disabled
    /// text keep their columns
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void DeclaratorBehindEndifInEventFieldAndLocalIsIndentedOneLevel()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 public event Action A,
                             #if X
                                     B,
                             #endif
                                     D;

                                 void M()
                                 {
                                     int a,
                             #if X
                                         b,
                             #endif
                                         c;
                                 }
                             }
                             """;
        const string todaysOutputOnMain = """
                                          using System;

                                          class C
                                          {
                                              public event Action A,
                                          #if X
                                                  B,
                                          #endif
                                              D;

                                              void M()
                                              {
                                                  int a,
                                          #if X
                                                      b,
                                          #endif
                                                  c;
                                              }
                                          }
                                          """;

        // Act & Assert
        AssertRuleResult(input);
        AssertRuleResult(todaysOutputOnMain, input);
    }

    /// <summary>
    /// Verifies that in a build that defines the symbol, the declarators inside the conditional group are code and continue their
    /// declaration as well, one level deeper than its first line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void DeclaratorsInsideAndBehindAnActiveConditionalGroupAreIndentedOneLevel()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 public event Action A,
                             #if X
                                 B,
                             #endif
                                 D;

                                 void M()
                                 {
                                     int a,
                             #if X
                                     b,
                             #endif
                                     c;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    public event Action A,
                                #if X
                                        B,
                                #endif
                                        D;

                                    void M()
                                    {
                                        int a,
                                #if X
                                            b,
                                #endif
                                            c;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CSharpParseOptions.Default.WithPreprocessorSymbols("X"));
    }

    /// <summary>
    /// Verifies that a declarator in the active branch of an <c>#else</c> or <c>#elif</c> group and the declarator behind the
    /// group continue their declaration, and the declarators in the disabled branches keep their columns
    /// </summary>
    /// <param name="branch">The directive that opens the active branch</param>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    [DataRow("#else", DisplayName = "Else branch")]
    [DataRow("#elif Y", DisplayName = "Elif branch")]
    public void DeclaratorInTheActiveBranchOfAnElseOrElifGroupIsIndentedOneLevel(string branch)
    {
        // Arrange
        var input = $"using System;\n\nclass C\n{{\n    public event Action A,\n#if X\n        B,\n{branch}\n    C,\n#endif\n    D;\n\n    void M()\n    {{\n        int a,\n#if X\n            b,\n{branch}\n        c,\n#endif\n        d;\n    }}\n}}";
        var expected = $"using System;\n\nclass C\n{{\n    public event Action A,\n#if X\n        B,\n{branch}\n        C,\n#endif\n        D;\n\n    void M()\n    {{\n        int a,\n#if X\n            b,\n{branch}\n            c,\n#endif\n            d;\n    }}\n}}";

        // Act & Assert
        AssertRuleResult(input, expected, CSharpParseOptions.Default.WithPreprocessorSymbols("Y"));
    }

    /// <summary>
    /// Verifies that the first declarator behind an <c>#endif</c> that stands directly behind the type of a local declaration
    /// continues the declaration, in a build that defines the symbol and in one that does not
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void FirstDeclaratorBehindEndifDirectlyBehindTheTypeIsIndentedOneLevel(bool defineX)
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int
                             #if X
                                         a = 1,
                             #endif
                                     b = 2;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int
                                #if X
                                            a = 1,
                                #endif
                                            b = 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, defineX ? CSharpParseOptions.Default.WithPreprocessorSymbols("X") : null);
    }

    /// <summary>
    /// Verifies the specification example: the lines inside the hole of a verbatim interpolated string begin inside the string's
    /// text, so they are indented from the line on which the string starts, and the text of the string keeps its columns
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void SpecExampleLinesInsideTheHoleOfAVerbatimInterpolatedStringAreIndentedFromTheStringsLine()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 string M(int x)
                                 {
                                     var s = $@"line one
                                   {
                                     x
                                     }";

                                     return s;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    string M(int x)
                                    {
                                        var s = $@"line one
                                      {
                                            x
                                            }";

                                        return s;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the lead's decision for the hole of a verbatim interpolated string inside a switch-expression arm: the lines
    /// inside the hole are indented four columns right of the arm's line, where the string's text starts
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void LinesInsideTheHoleOfAVerbatimInterpolatedStringInASwitchArmAreIndentedFromTheArmsLine()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 string M(int x)
                                 {
                                     return x switch
                                            {
                                                1 => $@"a
                                   {
                                     x
                                     }",
                                                _ => "",
                                            };
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    string M(int x)
                                    {
                                        return x switch
                                               {
                                                   1 => $@"a
                                      {
                                                       x
                                                       }",
                                                   _ => "",
                                               };
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example as whole-document formatting gives it: the continuation of a parenthesized expression
    /// that starts behind <c>var r = </c> is indented four columns right of the statement's line, and the <c>&amp;&amp;</c>
    /// aligns with its left operand
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void SpecExampleContinuationOfAParenthesizedExpressionInsideALogicalExpressionIsIndentedFromTheStatementsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int x, bool y)
                                 {
                                     var r = ( // c
                                     x + 3) > 0 &&
                                     y;

                                     return r;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(int x, bool y)
                                    {
                                        var r = ( // c
                                            x + 3) > 0
                                                && y;

                                        return r;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the RH5308 sentence of the specification as whole-document formatting gives it: the continuation of the
    /// parenthesized expression is indented four columns right of the statement's line, and the conditional's operators align
    /// as FMT-INDENT-020 states
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void ContinuationOfAParenthesizedExpressionInsideAConditionIsIndentedFromTheStatementsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     var s = ( // c
                                     x + 3) > 0 ? 1 :
                                     2;

                                     return s;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        var s = ( // c
                                            x + 3) > 0
                                                    ? 1
                                                    : 2;

                                        return s;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a continuation line of a continuation line is indented one more level: the operand behind the second
    /// comment continues the parenthesized expression that starts on the first continuation line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void ContinuationOfAContinuationLineIsIndentedOneMoreLevel()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x, int y)
                                 {
                                     x = ( // a
                                     y + ( // b
                                     3));

                                     return x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x, int y)
                                    {
                                        x = ( // a
                                            y + ( // b
                                                3));

                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>;</c> behind a conditional group of a statement is a continuation line, and the disabled text keeps its
    /// column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void SemicolonBehindAConditionalGroupIsAContinuationLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Get()
                                 {
                                     return 42
                             #if DEBUG
                                     + 1
                             #endif
                                     ;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Get()
                                    {
                                        return 42
                                #if DEBUG
                                        + 1
                                #endif
                                            ;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example of the other side: the attribute lists of a member and the token behind them, a label
    /// and its statement, <c>else</c>, and the <c>while</c> of a <c>do</c> statement are placed by block indentation and keep
    /// their columns
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void SpecExampleLinesPlacedByBlockIndentationAreNoContinuationLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 [System.Obsolete]
                                 [System.CLSCompliant(false)]
                                 public void M(int x)
                                 {
                                     L:
                                     x++;

                                     if (x > 0)
                                     {
                                     }
                                     else
                                     {
                                     }

                                     do
                                     {
                                     }
                                     while (x < 0);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the attribute list of a local function and the token behind it, a <c>catch</c> and a <c>finally</c>, and an
    /// enum member are placed by block indentation and keep their columns
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void OtherLinesPlacedByBlockIndentationAreNoContinuationLines()
    {
        // Arrange
        const string input = """
                             using System;

                             enum E
                             {
                                 A,
                                 B
                             }

                             class C
                             {
                                 void M(int x)
                                 {
                                     [Obsolete]
                                     void L()
                                     {
                                     }

                                     try
                                     {
                                         L();
                                     }
                                     catch (Exception)
                                     {
                                     }
                                     finally
                                     {
                                         x++;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: the condition of a <c>case</c> label's <c>when</c> clause, wrapped behind <c>when</c>,
    /// continues the label, which starts on the line of <c>case</c>, so it is indented one level deeper than <c>case</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void SpecExampleWhenConditionWrappedBehindWhenIsIndentedOneLevelDeeperThanCase()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1 when
                                         Check(value):
                                             break;
                                     }
                                 }

                                 bool Check(int v)
                                 {
                                     return v > 0;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1 when
                                                Check(value):
                                                break;
                                        }
                                    }

                                    bool Check(int v)
                                    {
                                        return v > 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>when</c> condition written left of the <c>case</c> keyword, and one written further right behind a
    /// combined pattern, both take the column one level deeper than <c>case</c>, and that an already indented condition stays
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void WhenConditionWrappedBehindWhenTakesTheContinuationColumnWhereverItIsWritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1 when
                                             Check(value):
                                             break;

                                         case 2 when
                             Check(value):
                                             break;

                                         case > 4 and < 10 when
                                                 Check(value):
                                             break;
                                     }
                                 }

                                 bool Check(int v)
                                 {
                                     return v > 0;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1 when
                                                Check(value):
                                                break;

                                            case 2 when
                                                Check(value):
                                                break;

                                            case > 4 and < 10 when
                                                Check(value):
                                                break;
                                        }
                                    }

                                    bool Check(int v)
                                    {
                                        return v > 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a first base type below the colon, a first type argument below <c>&lt;</c>, and the
    /// right part of a qualified name below its <c>.</c>, each kept there by a comment, are continuation lines, and the later
    /// base type aligns under the first
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void SpecExampleLinesACommentKeepsInsideABaseListATypeArgumentListOrAQualifiedNameAreContinuationLines()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             interface I1;

                             interface I2;

                             class C : // c
                             I1,
                             I2
                             {
                                 List< // c
                                 int> P => null;

                                 void N<T>()
                                     where T : System. // c
                                     IDisposable
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                interface I1;

                                interface I2;

                                class C : // c
                                    I1,
                                    I2
                                {
                                    List< // c
                                        int> P => null;

                                    void N<T>()
                                        where T : System. // c
                                            IDisposable
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a first element a comment keeps below the opening bracket of a type parameter list, a function-pointer
    /// parameter list, and a type argument list in a base type is a continuation line, as below a type argument list's
    /// <c>&lt;</c> in a member type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-034")]
    public void FirstElementACommentKeepsBelowTheOpeningBracketOfOtherAngleBracketListsIsAContinuationLine()
    {
        // Arrange
        const string input = """
                             using System;

                             class D< // c
                             T>
                             {
                                 unsafe void P(delegate*< // c
                                 int, void> f)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class D< // c
                                    T>
                                {
                                    unsafe void P(delegate*< // c
                                                  int, void> f)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}