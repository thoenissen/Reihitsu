using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for lists whose elements decide whether they are multi-line, for the commas of multi-line argument
/// and attribute argument lists, which end the line of the element in front of them, together with a comment behind them
/// </summary>
[TestClass]
public class ListWrapByElementsTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the reported example: an argument list wrapped only in front of a comma gets one argument per line, with
    /// the comma at the end of the line of the argument in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void ArgumentListWrappedInFrontOfCommaGetsOneArgumentPerLineWithTrailingCommas()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1
                                         , 2, 3);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1,
                                            2,
                                            3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list wrapped only in front of its last comma gets one argument per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void ArgumentListWrappedInFrontOfLastCommaGetsOneArgumentPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1, 2
                                         , 3);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1,
                                            2,
                                            3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma in a list that already has one argument per line moves behind the argument in front
    /// of it instead of being split off onto its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void LeadingCommaInListWithOneArgumentPerLineMovesBehindPreviousArgument()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1
                                         , 2,
                                         3);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1,
                                            2,
                                            3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the leading comma of a two-argument list moves behind the first argument
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void LeadingCommaOfTwoArgumentListMovesBehindFirstArgument()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(a
                                         , b);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(a,
                                            b);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an attribute argument list wrapped only in front of a comma gets one argument per line with trailing
    /// commas
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void AttributeArgumentListWrappedInFrontOfCommaGetsOneArgumentPerLine()
    {
        // Arrange
        const string input = """
                             using System;

                             [AttributeUsage(AttributeTargets.All
                                 , AllowMultiple = true, Inherited = false)]
                             class C : Attribute
                             {
                                 private int _value;
                             }
                             """;
        const string expected = """
                                using System;

                                [AttributeUsage(AttributeTargets.All,
                                                AllowMultiple = true,
                                                Inherited = false)]
                                class C : Attribute
                                {
                                    private int _value;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a nested argument list wrapped in front of a comma gets trailing commas, and that the outer list,
    /// which the nested one spans several lines in, gets one argument per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void NestedArgumentListWrappedInFrontOfCommaGetsTrailingCommas()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(Bar(1
                                             , 2), 3);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(Bar(1,
                                                2),
                                            3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the argument list of an object creation wrapped in front of a comma gets one argument per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void ObjectCreationArgumentListWrappedInFrontOfCommaGetsOneArgumentPerLine()
    {
        // Arrange
        const string input = """
                             class B
                             {
                                 public B(int a, int b, int c)
                                 {
                                 }
                             }

                             class C
                             {
                                 B M()
                                 {
                                     return new B(1
                                                  , 2, 3);
                                 }
                             }
                             """;
        const string expected = """
                                class B
                                {
                                    public B(int a, int b, int c)
                                    {
                                    }
                                }

                                class C
                                {
                                    B M()
                                    {
                                        return new B(1,
                                                     2,
                                                     3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the argument list of a constructor initializer wrapped in front of a comma gets one argument per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void ConstructorInitializerArgumentListWrappedInFrontOfCommaGetsOneArgumentPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public C(int a, int b, int c)
                                 {
                                 }

                                 public C(int a)
                                     : this(a
                                            , 2, 3)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public C(int a, int b, int c)
                                    {
                                    }

                                    public C(int a)
                                        : this(a,
                                               2,
                                               3)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the argument list of a primary constructor's base type wrapped in front of a comma gets one argument
    /// per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void PrimaryConstructorBaseArgumentListWrappedInFrontOfCommaGetsOneArgumentPerLine()
    {
        // Arrange
        const string input = """
                             class B(int a, int b, int c)
                             {
                                 public int A => a + b + c;
                             }

                             class P(int a) : B(a
                                                , 2, 3)
                             {
                                 public int Q => a;
                             }
                             """;
        const string expected = """
                                class B(int a, int b, int c)
                                {
                                    public int A => a + b + c;
                                }

                                class P(int a) : B(a,
                                                   2,
                                                   3)
                                {
                                    public int Q => a;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a comma at the start of a line makes an argument or attribute argument list
    /// multi-line and moves behind the element in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void SpecificationExampleLeadingCommasMoveBehindPreviousElement()
    {
        // Arrange
        const string input = """
                             using System;

                             [AttributeUsage(AttributeTargets.All
                                 , AllowMultiple = true, Inherited = false)]
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1
                                         , 2, 3);
                                     Foo(1
                                         , 2,
                                         3);
                                     Foo(a
                                         , b);
                                     Foo(Bar(1
                                             , 2), 3);
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                [AttributeUsage(AttributeTargets.All,
                                                AllowMultiple = true,
                                                Inherited = false)]
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1,
                                            2,
                                            3);
                                        Foo(1,
                                            2,
                                            3);
                                        Foo(a,
                                            b);
                                        Foo(Bar(1,
                                                2),
                                            3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a comment behind a comma that moves goes with the comma, and a block comment in
    /// front of a comma on the element's own line keeps nothing
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void SpecificationExampleCommentsMoveWithTheirComma()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1
                                         , /* c */ 2, 3);
                                     Foo(1
                                         , // c
                                         2, 3);
                                     Foo(1 /* one */, 2,
                                         3);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1, /* c */
                                            2,
                                            3);
                                        Foo(1, // c
                                            2,
                                            3);
                                        Foo(1 /* one */,
                                            2,
                                            3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: an argument list wrapped only behind its opening parenthesis is single-line
    /// and joined, next to lists wrapped between their elements
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void SpecificationExampleListWrappedOnlyBehindOpeningParenthesisIsJoined()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(a, b,
                                         c);
                                     Foo(
                                         a, b);
                                     Foo(a, b, c);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(a,
                                            b,
                                            c);
                                        Foo(a, b);
                                        Foo(a, b, c);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an attribute argument list and a parameter list wrapped only behind their opening parenthesis are
    /// single-line and joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void AttributeArgumentAndParameterListsWrappedOnlyBehindOpeningParenthesisAreJoined()
    {
        // Arrange
        const string input = """
                             using System;

                             class A : Attribute
                             {
                                 public A(int a, int b)
                                 {
                                 }
                             }

                             [A(
                                 1, 2)]
                             class C
                             {
                                 void N(
                                     int a, int b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class A : Attribute
                                {
                                    public A(int a, int b)
                                    {
                                    }
                                }

                                [A(1, 2)]
                                class C
                                {
                                    void N(int a, int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: an indexer parameter list wrapped between two parameters gets one parameter per
    /// line, and one wrapped only behind its opening bracket is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void SpecificationExampleIndexerWrappedOnlyBehindOpeningBracketIsJoined()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int this[int a, int b,
                                          int c] => a;

                                 int this[
                                     string a, string b] => 0;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int this[int a,
                                             int b,
                                             int c] => a;

                                    int this[string a, string b] => 0;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list whose comma sits only in an inactive conditional branch is single-line, because its
    /// parsed elements start on one line, and stays as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void ArgumentCommaInInactiveBranchLeavesSingleLineListUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1, 2
                             #if X
                                         , 3
                             #endif
                                         );
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block comment behind a leading comma moves with the comma and stays behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void BlockCommentBehindLeadingArgumentCommaMovesWithTheComma()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1
                                         , /* c */ 2, 3);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1, /* c */
                                            2,
                                            3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment behind a leading comma moves with the comma and keeps ending its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void LineCommentBehindLeadingArgumentCommaMovesWithTheComma()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1
                                         , // c
                                         2, 3);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1, // c
                                            2,
                                            3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment in front of a comma on the argument's own line keeps nothing: the list wrapped behind
    /// a later comma gets one argument per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void BlockCommentInFrontOfCommaOnArgumentLineKeepsNothing()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1 /* one */, 2,
                                         3);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1 /* one */,
                                            2,
                                            3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break inside the text of a multi-line comment between two arguments does not make the list
    /// multi-line, so the list stays as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void LineBreakInsideMultiLineCommentBetweenArgumentsLeavesListSingleLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(1, /* x
                                         y */ 2, 3);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: a first element that a comment or a directive keeps off the line of the opening
    /// parenthesis stays on its own line, and the list becomes one element per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void SpecificationExampleHeldFirstElementMakesListOneElementPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void N( // c
                                     int a, int b)
                                 {
                                 }

                                 void M()
                                 {
                                     Foo( // c
                                         1, 2);
                                     Foo(
                             #pragma warning disable CS0168
                                         1, 2);
                             #pragma warning restore CS0168
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void N( // c
                                           int a,
                                           int b)
                                    {
                                    }

                                    void M()
                                    {
                                        Foo( // c
                                            1,
                                            2);
                                        Foo(
                                #pragma warning disable CS0168
                                            1,
                                            2);
                                #pragma warning restore CS0168
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a first argument that a line comment behind the opening parenthesis keeps off that line starts its own
    /// line, so the argument list is multi-line with one argument per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void LineCommentBehindOpeningParenthesisMakesArgumentListOneArgumentPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo( // c
                                         1, 2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo( // c
                                            1,
                                            2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a first argument behind a block comment on the line below the opening parenthesis starts its own line,
    /// so the argument list is multi-line with one argument per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void BlockCommentInFrontOfFirstArgumentOnLineBelowOpeningParenthesisMakesArgumentListOneArgumentPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(
                                         /* c */ 1, 2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(
                                            /* c */ 1,
                                            2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a first argument that a directive keeps off the line of the opening parenthesis starts its own line,
    /// so the argument list is multi-line with one argument per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void DirectiveInFrontOfFirstArgumentMakesArgumentListOneArgumentPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(
                             #pragma warning disable CS0168
                                         1, 2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(
                                #pragma warning disable CS0168
                                            1,
                                            2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a first argument that disabled text keeps off the line of the opening parenthesis starts its own line,
    /// so the argument list is multi-line with one argument per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void DisabledTextInFrontOfFirstArgumentMakesArgumentListOneArgumentPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(
                             #if X
                                         0,
                             #endif
                                         1, 2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(
                                #if X
                                            0,
                                #endif
                                            1,
                                            2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a first argument of an object creation that a line comment behind the opening parenthesis keeps off
    /// that line starts its own line, so the argument list is multi-line with one argument per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void LineCommentBehindObjectCreationOpeningParenthesisMakesArgumentListOneArgumentPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var d = new D( // c
                                         1, 2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var d = new D( // c
                                                      1,
                                                      2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a first attribute argument that a line comment behind the opening parenthesis keeps off that line
    /// starts its own line, so the attribute argument list is multi-line with one argument per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void LineCommentBehindAttributeOpeningParenthesisMakesAttributeArgumentListOneArgumentPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 [Attr( // c
                                     1, 2)]
                                 void M()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    [Attr( // c
                                          1,
                                          2)]
                                    void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a first parameter that a line comment behind the opening parenthesis keeps off that line starts its
    /// own line, so the parameter list is multi-line with one parameter per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void LineCommentBehindOpeningParenthesisMakesParameterListOneParameterPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M( // c
                                     int a, int b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M( // c
                                           int a,
                                           int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a first lambda parameter that a line comment behind the opening parenthesis keeps off that line starts
    /// its own line, so the lambda parameter list is multi-line with one parameter per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void LineCommentBehindOpeningParenthesisMakesLambdaParameterListOneParameterPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.Func<int, int, int> f = ( // c
                                         int a, int b) => a + b;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        System.Func<int, int, int> f = ( // c
                                                                        int a,
                                                                        int b) => a + b;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a first indexer parameter that a line comment behind the opening bracket keeps off that line starts its
    /// own line, so the indexer parameter list is multi-line with one parameter per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void LineCommentBehindOpeningBracketMakesIndexerParameterListOneParameterPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int this[ // c
                                     int a, int b] => a;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int this[ // c
                                             int a,
                                             int b] => a;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line comment behind the opening parenthesis that holds no line break of the gap leaves the first
    /// argument on that line, so the argument list stays single-line and as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void MultiLineCommentBehindOpeningParenthesisLeavesArgumentListSingleLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(/* a
                                         b */1, 2);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Tests
}