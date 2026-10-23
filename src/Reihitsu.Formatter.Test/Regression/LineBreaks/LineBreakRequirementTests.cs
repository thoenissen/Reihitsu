using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the line-break requirements whose specified examples no other test
/// asserts as exact output
/// </summary>
[TestClass]
public class LineBreakRequirementTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that a comment, a directive, and disabled text in a join gap each keep the
    /// wrapped arguments as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    public void CommentDirectiveAndDisabledTextInJoinGapKeepTheWrap()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(out /* why */
                                         var value);
                                     Foo(a, // first
                                         b);
                                     Foo(a,
                             #if X
                                         b);
                             #else
                                         c);
                             #endif
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment between a declaration and its opening brace stays in place while
    /// the brace starts the next line, and that a comment behind an opening brace stays on the
    /// brace's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-003")]
    public void CommentsBeforeAndBehindOpeningBraceStayInPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M() /* c */ {
                                     Foo();
                                 }

                                 void O() { /* c */ }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M() /* c */
                                    {
                                        Foo();
                                    }

                                    void O()
                                    { /* c */
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line comment behind an opening brace stays on the brace's line
    /// when the brace moves onto its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-003")]
    public void LineCommentBehindOpeningBraceMovesWithTheBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M() { // c
                                     Foo();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    { // c
                                        Foo();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that statements sharing a line are split, that a comment trailing the previous
    /// statement stays on its line, and that an empty statement keeps its neighbor on its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void StatementsSharingALineAreSplitExceptNextToAnEmptyStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(); Qux();
                                     Bar(); /* d */ Baz();
                                     ; Foo();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo();
                                        Qux();
                                        Bar(); /* d */
                                        Baz();
                                        ; Foo();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the braces of a switch statement, of a switch section's block, and of a
    /// switch expression start their own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-008")]
    public void SwitchStatementSectionBlockAndSwitchExpressionBracesStartTheirOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int a)
                                 {
                                     switch (a) {
                                         case 1: { Foo(); break; }
                                     }

                                     return a switch { _ => 3 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                {
                                                    Foo();

                                                    break;
                                                }
                                        }

                                        return a switch
                                               {
                                                   _ => 3
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the semicolons of a field, an event field, a property with an initializer,
    /// and a delegate are joined onto the line that ends the declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-009")]
    public void DeclarationSemicolonsJoinTheDeclarationLine()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 int _a
                                     ;
                                 event Action E
                                     ;
                                 int P { get; set; }
                                     = 5
                                     ;
                             }
                             delegate void D()
                                 ;
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    int _a;
                                    event Action E;
                                    int P { get; set; } = 5;
                                }
                                delegate void D();
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment in front of a declaration semicolon keeps the semicolon on its own
    /// line, because the join would cross the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    [Requirement("FMT-BREAK-009")]
    public void DeclarationSemicolonAfterCommentIsNotJoined()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int _a // c
                                 ;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a directive in front of a declaration semicolon keeps the semicolon on its
    /// own line, because the join would cross the directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    [Requirement("FMT-BREAK-009")]
    public void DeclarationSemicolonAfterDirectiveIsNotJoined()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 event Action E
                             #if X
                             #endif
                                 ;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a constructor initializer sharing the parameter list's line moves onto its
    /// own line, and that one already on its own line is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-010")]
    public void ConstructorInitializerStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 C(int a)
                                     : base(a)
                                 {
                                 }

                                 C() : this(1)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    C(int a)
                                        : base(a)
                                    {
                                    }

                                    C()
                                        : this(1)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the opening parenthesis, the commas, and the closing parenthesis of a
    /// parameter list are moved onto the lines of the declaration and of the parameters
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void ParameterListDelimitersHugTheDeclarationAndParameters()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M
                                     (int a
                                      , int b)
                                 {
                                 }

                                 void N(int a, int b
                                     )
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a,
                                           int b)
                                    {
                                    }

                                    void N(int a, int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that wrapped attribute argument, parameter, and argument lists become one element
    /// per line with the first element on the opening line, while a one-line list stays on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-012")]
    public void WrappedListsBecomeOneElementPerLine()
    {
        // Arrange
        const string input = """
                             [Bar(
                                 1,
                                 2)]
                             class C
                             {
                                 void M(int a, int b,
                                        int c)
                                 {
                                     Foo(a, b,
                                         c);
                                     Foo(
                                         a,
                                         b);
                                     Foo(a, b, c);
                                 }
                             }
                             """;
        const string expected = """
                                [Bar(1,
                                     2)]
                                class C
                                {
                                    void M(int a,
                                           int b,
                                           int c)
                                    {
                                        Foo(a,
                                            b,
                                            c);
                                        Foo(a,
                                            b);
                                        Foo(a, b, c);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a variable value wrapped after <c>=</c>, an assignment wrapped before
    /// <c>=</c>, and a raw string literal wrapped after <c>=</c> start on the operator's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void AssignmentTargetOperatorAndValueAreJoined()
    {
        // Arrange
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var x =
                                         Foo();
                                     x
                                         = Bar();

                                     var w =
                                         """
                                         a
                                         """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var x = Foo();
                                        x = Bar();

                                        var w = """
                                                a
                                                """;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Characterizes the current layout of wrapped enum member values: a wrap before <c>=</c> is
    /// kept, a wrap after <c>=</c> is joined. A decided change will keep the operator and the
    /// value on the member's line, so this test carries no requirement until it is implemented
    /// </summary>
    [TestMethod]
    public void EnumMemberWrapBeforeEqualsIsKeptAndWrapAfterEqualsIsJoined()
    {
        // Arrange
        const string input = """
                             enum E
                             {
                                 A
                                     = 1,
                                 B =
                                     2,
                                 C = 3
                             }
                             """;
        const string expected = """
                                enum E
                                {
                                    A
                                    = 1,
                                    B = 2,
                                    C = 3
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a binary operator ending a line moves to the start of the continuation line,
    /// and that an operator already starting it is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void BinaryOperatorMovesToTheStartOfTheContinuationLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a &&
                                             b;
                                     var z = a + b
                                             + c * d;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a
                                                && b;
                                        var z = a + b
                                                + c * d;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a binary expression written on one line is never wrapped
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-017")]
    public void BinaryExpressionOnOneLineIsNotWrapped()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(bool a, bool b, int c, int d)
                                 {
                                     return a && b || c + d * c > d - c;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that collection, object, and anonymous-object initializers are laid out over
    /// several lines while an array initializer keeps the author's one-line layout
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-021")]
    public void ObjectInitializersAreExpandedAndArrayInitializersKeepTheirLayout()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var a = new List<int> { 1, 2 };
                                     var b = new Foo { A = 1 };
                                     var d = new { A = 1 };
                                     var c = new int[] { 1, 2, 3 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = new List<int>
                                                {
                                                    1,
                                                    2
                                                };
                                        var b = new Foo
                                                {
                                                    A = 1
                                                };
                                        var d = new
                                                {
                                                    A = 1
                                                };
                                        var c = new int[] { 1, 2, 3 };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a one-line collection expression stays on one line, while a wrapped
    /// collection expression and a wrapped property pattern put every element on its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-022")]
    public void WrappedCollectionExpressionAndPropertyPatternGetOneElementPerLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     int[] e = [1, 2, 3];
                                     int[] f = [1,
                                                2];

                                     return o is Point { X: 1,
                                                         Y: 2 } or null;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(object o)
                                    {
                                        int[] e = [1, 2, 3];
                                        int[] f = [
                                                      1,
                                                      2
                                                  ];

                                        return o is Point
                                                 {
                                                     X: 1,
                                                     Y: 2
                                                 }
                                                 or null;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every <c>where</c> clause of a type, a method, and an interface member
    /// without a body starts its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-023")]
    public void GenericConstraintsOfTypesMethodsAndBodilessMembersStartTheirOwnLines()
    {
        // Arrange
        const string input = """
                             class C<T> where T : class, new()
                             {
                                 void M<U, V>() where U : class where V : struct
                                 {
                                 }
                             }

                             interface I
                             {
                                 void M<T>() where T : class;
                             }
                             """;
        const string expected = """
                                class C<T>
                                    where T : class, new()
                                {
                                    void M<U, V>()
                                        where U : class
                                        where V : struct
                                    {
                                    }
                                }

                                interface I
                                {
                                    void M<T>()
                                        where T : class;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the <c>where</c> clauses of a delegate, a local function, and a bodiless
    /// partial method start their own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-023")]
    public void GenericConstraintsOfDelegatesLocalFunctionsAndPartialMethodsStartTheirOwnLines()
    {
        // Arrange
        const string input = """
                             delegate void D<T>() where T : class;

                             partial class C
                             {
                                 void M()
                                 {
                                     void L<T>() where T : struct
                                     {
                                     }

                                     L<int>();
                                 }

                                 partial void P<T>() where T : class;
                             }
                             """;
        const string expected = """
                                delegate void D<T>()
                                    where T : class;

                                partial class C
                                {
                                    void M()
                                    {
                                        void L<T>()
                                            where T : struct
                                        {
                                        }

                                        L<int>();
                                    }

                                    partial void P<T>()
                                        where T : class;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the arrow and the expression start of expression-bodied properties and of an
    /// expression-bodied indexer are joined onto the declaration line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-026")]
    public void ExpressionBodiedPropertiesAndIndexersAreJoinedWithTheirArrow()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int S
                                     => 5;

                                 int T =>
                                     5;

                                 int this[int i]
                                     => i;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int S => 5;

                                    int T => 5;

                                    int this[int i] => i;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}