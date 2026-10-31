using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the horizontal-spacing requirements, each built on the specified
/// example of its requirement
/// </summary>
[TestClass]
public class HorizontalSpacingRequirementTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that the whitespace in front of a comment that ends the line is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-001")]
    public void WhitespaceBeforeEndOfLineCommentIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     M(a);    // trailing
                                     M(a);// tight
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the gap after a cast, which no rule covers, is kept or removed as written and
    /// only collapsed when it holds several spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-002")]
    public void GapNoRuleCoversIsOnlyCollapsed()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object o)
                                 {
                                     var s = (string)o;
                                     var t = (string) o;
                                     var u = (string)  o;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o)
                                    {
                                        var s = (string)o;
                                        var t = (string) o;
                                        var u = (string) o;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that exactly one space follows the closing bracket of an attribute list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-003")]
    public void OneSpaceFollowsAttributeList()
    {
        // Arrange
        const string input = """
                             using System.Runtime.InteropServices;

                             class C
                             {
                                 void M([In]int a, [In]  int b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System.Runtime.InteropServices;

                                class C
                                {
                                    void M([In] int a, [In] int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace inside parentheses and brackets and before commas and semicolons
    /// is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-004")]
    public void NoSpaceInsideDelimitersOrBeforeCommasAndSemicolons()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int[] values, int a)
                                 {
                                     M( values , a );

                                     return values[ a ] ;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int[] values, int a)
                                    {
                                        M(values, a);

                                        return values[a];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that generic angle brackets and the nullable question mark are tightened
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-005")]
    public void GenericBracketsAndNullableQuestionMarkAreTight()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(int? a)
                                 {
                                     List < int > list = null;
                                     int ? b = a;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(int? a)
                                    {
                                        List<int> list = null;
                                        int? b = a;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the member-access dot, the conditional-access question mark, and the pointer
    /// arrow are tightened on both sides
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-006")]
    public void MemberAccessSymbolsAreTight()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 unsafe void M(C c, S* p)
                                 {
                                     var a = c . ToString();
                                     var b = c ?. ToString();
                                     var e = p -> V;
                                 }
                             }

                             struct S
                             {
                                 public int V;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    unsafe void M(C c, S* p)
                                    {
                                        var a = c.ToString();
                                        var b = c?.ToString();
                                        var e = p->V;
                                    }
                                }

                                struct S
                                {
                                    public int V;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that unary operators are tightened unless the operand starts with the same
    /// symbol, in which case the gap is only collapsed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-007")]
    public void UnaryOperatorsAreTightUnlessThatChangesTheOperator()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a, bool b, object o)
                                 {
                                     var x = - a;
                                     var y = ! b;
                                     var z = o !;
                                     var w = -  -a;
                                     var v = + +a;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a, bool b, object o)
                                    {
                                        var x = -a;
                                        var y = !b;
                                        var z = o!;
                                        var w = - -a;
                                        var v = + +a;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a prefix minus in front of an operand starting with a different symbol is
    /// tightened
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-007")]
    public void PrefixMinusBeforeDifferentSymbolIsTightened()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     var x = - +a;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = -+a;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comma gets one space after it except in rank-only array specifiers,
    /// unbound generic types, and interpolation alignment clauses
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-008")]
    public void CommaGetsOneSpaceExceptInCompactForms()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(int a)
                                 {
                                     var t = typeof(Dictionary< , >);
                                     int[ , ] r = new int[2,3];
                                     var s = $"{a , -10}|{a,  5}|{a,-3:N2}";
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(int a)
                                    {
                                        var t = typeof(Dictionary<,>);
                                        int[,] r = new int[2, 3];
                                        var s = $"{a,-10}|{a,5}|{a,-3:N2}";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that one space goes before a brace and around base-list and constructor-initializer
    /// colons, while interpolation braces keep their spacing
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-009")]
    public void OneSpaceBeforeBracesAndAroundDeclarationColons()
    {
        // Arrange
        const string input = """
                             class B
                             {
                                 public B(int a)
                                 {
                                 }
                             }

                             class C:B
                             {
                                 public C(int a)
                                     :base(a)
                                 {
                                 }

                                 string M(int a)
                                 {
                                     var values = new int[]{ a };

                                     return $"{a}";
                                 }
                             }
                             """;
        const string expected = """
                                class B
                                {
                                    public B(int a)
                                    {
                                    }
                                }

                                class C : B
                                {
                                    public C(int a)
                                        : base(a)
                                    {
                                    }

                                    string M(int a)
                                    {
                                        var values = new int[] { a };

                                        return $"{a}";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that binary, assignment, compound-assignment, and name-equals operators get one
    /// space on both sides
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-010")]
    public void OneSpaceAroundBinaryAndAssignmentOperators()
    {
        // Arrange
        const string input = """
                             using X=System.String;

                             class A : System.Attribute
                             {
                                 public int N { get; set; }
                             }

                             class C
                             {
                                 [A(N=1)]
                                 void M(int a)
                                 {
                                     var x=a+1;
                                     x+=a;

                                     var b=a==1&&a<2;
                                     var q=a??1;
                                 }
                             }
                             """;
        const string expected = """
                                using X = System.String;

                                class A : System.Attribute
                                {
                                    public int N { get; set; }
                                }

                                class C
                                {
                                    [A(N = 1)]
                                    void M(int a)
                                    {
                                        var x = a + 1;
                                        x += a;

                                        var b = a == 1 && a < 2;
                                        var q = a ?? 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a for-loop semicolon gets one space after it unless an empty clause follows
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-011")]
    public void OneSpaceAfterForLoopSemicolonUnlessTheClauseIsEmpty()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     for (int i = 0;i < a;i++)
                                     {
                                     }

                                     for (; ;)
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        for (int i = 0; i < a; i++)
                                        {
                                        }

                                        for (;;)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that one space follows statement keywords, <c>new</c>, and <c>operator</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    public void OneSpaceAfterStatementKeywordsNewAndOperator()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 int M(int a, object o)
                                 {
                                     if(a > 0)
                                     {
                                         return(a);
                                     }

                                     lock(o)
                                     {
                                         throw new  Exception();
                                     }
                                 }

                                 public static C operator+(C a, C b)
                                 {
                                     return a;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    int M(int a, object o)
                                    {
                                        if (a > 0)
                                        {
                                            return (a);
                                        }

                                        lock (o)
                                        {
                                            throw new Exception();
                                        }
                                    }

                                    public static C operator +(C a, C b)
                                    {
                                        return a;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the keyword exceptions that get no space: <c>return</c> and <c>throw</c> before
    /// <c>;</c>, an attribute target before its colon, and <c>new</c> before <c>(</c> or <c>[</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    public void KeywordExceptionsGetNoSpace()
    {
        // Arrange
        const string input = """
                             class C<T>
                                 where T : new ( )
                             {
                                 [return : System.Obsolete]
                                 C<T> M()
                                 {
                                     C<T> c = new  ();
                                     var arr = new  [] { 1 };

                                     return c ;
                                 }

                                 void N()
                                 {
                                     try
                                     {
                                     }
                                     catch
                                     {
                                         throw ;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C<T>
                                    where T : new()
                                {
                                    [return: System.Obsolete]
                                    C<T> M()
                                    {
                                        C<T> c = new();
                                        var arr = new[] { 1 };

                                        return c;
                                    }

                                    void N()
                                    {
                                        try
                                        {
                                        }
                                        catch
                                        {
                                            throw;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the rule's decision applies after a comment the left token carries, while the
    /// whitespace in front of that comment is only collapsed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-013")]
    public void CommentOfTheLeftTokenKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     var x = a  /* one */  + 1;
                                     var y = a/* two */  + 1;

                                     M(a /* three */ );
                                     M( /* four */ a);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var x = a /* one */ + 1;
                                        var y = a/* two */ + 1;

                                        M(a /* three */);
                                        M( /* four */a);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: the <c>fixed</c> statement gets one space between the keyword and its
    /// parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    public void OneSpaceAfterFixedKeyword()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 unsafe void M(int[] items)
                                 {
                                     fixed(int* p = items)
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    unsafe void M(int[] items)
                                    {
                                        fixed (int* p = items)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change-doc example: a block comment the <c>fixed</c> keyword carries stays on it, and one space
    /// is put between the comment and the parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    [Requirement("FMT-SPACE-013")]
    public void OneSpaceAfterFixedKeywordBehindBlockComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 unsafe void M(int[] items, int[] others)
                                 {
                                     fixed/* c */(int* p = items)
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    unsafe void M(int[] items, int[] others)
                                    {
                                        fixed/* c */ (int* p = items)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that each of two nested <c>fixed</c> statements gets one space between the keyword and its parenthesis.
    /// The inner statement is written at the column FMT-INDENT-004 gives an unbraced embedded statement, so only the
    /// spacing changes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    public void OneSpaceAfterEachOfNestedFixedKeywords()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 unsafe void M(int[] items, int[] others)
                                 {
                                     fixed(int* p = items)
                                         fixed(int* q = others)
                                         {
                                         }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    unsafe void M(int[] items, int[] others)
                                    {
                                        fixed (int* p = items)
                                            fixed (int* q = others)
                                            {
                                            }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several spaces between the <c>fixed</c> keyword and its parenthesis become one
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    public void SeveralSpacesAfterFixedKeywordBecomeOne()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 unsafe void M(int[] items, int[] others)
                                 {
                                     fixed   (int* p = items)
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    unsafe void M(int[] items, int[] others)
                                    {
                                        fixed (int* p = items)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>fixed</c> statement with one space behind the keyword is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    public void SpacedFixedKeywordIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 unsafe void M(int[] items, int[] others)
                                 {
                                     fixed (int* p = items)
                                     {
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: a fixed-size buffer whose <c>fixed</c> keyword carries a block comment glued
    /// to the type gets one space between the comment and the type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    [Requirement("FMT-SPACE-013")]
    public void OneSpaceBehindCommentGluedToFixedSizeBufferType()
    {
        // Arrange
        const string input = """
                             unsafe struct S
                             {
                                 public fixed/* c */int _b[4];
                             }
                             """;
        const string expected = """
                                unsafe struct S
                                {
                                    public fixed/* c */ int _b[4];
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a fixed-size buffer with a space between <c>fixed</c> and a block comment glued to the type keeps
    /// that space and gets one space between the comment and the type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    [Requirement("FMT-SPACE-013")]
    public void OneSpaceBehindSpacedCommentGluedToFixedSizeBufferType()
    {
        // Arrange
        const string input = """
                             unsafe struct S
                             {
                                 public fixed /* c */int _c[4];
                             }
                             """;
        const string expected = """
                                unsafe struct S
                                {
                                    public fixed /* c */ int _c[4];
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several spaces between a block comment behind <c>fixed</c> and the type of a fixed-size buffer
    /// become one
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    [Requirement("FMT-SPACE-013")]
    public void SeveralSpacesBehindCommentInFixedSizeBufferBecomeOne()
    {
        // Arrange
        const string input = """
                             unsafe struct S
                             {
                                 public fixed /* d */  int _d[4];
                             }
                             """;
        const string expected = """
                                unsafe struct S
                                {
                                    public fixed /* d */ int _d[4];
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a fixed-size buffer keeps exactly one space behind <c>fixed</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    public void FixedSizeBufferKeepsOneSpaceAfterFixedKeyword()
    {
        // Arrange
        const string input = """
                             unsafe struct S
                             {
                                 public fixed int _buffer[4];
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that several spaces behind <c>fixed</c> in a fixed-size buffer become one
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    public void SeveralSpacesInFixedSizeBufferBecomeOne()
    {
        // Arrange
        const string input = """
                             unsafe struct S
                             {
                                 public fixed  int _buffer[4];
                             }
                             """;
        const string expected = """
                                unsafe struct S
                                {
                                    public fixed int _buffer[4];
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}