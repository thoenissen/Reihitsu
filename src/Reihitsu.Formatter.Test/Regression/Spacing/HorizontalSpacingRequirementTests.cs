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

    /// <summary>
    /// Verifies the example of the requirement: every attribute-target colon gets no space before it and one space behind it, and a named
    /// attribute argument's colon is decided by its own rule
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-016")]
    public void AttributeTargetColonsGetNoSpaceBeforeAndOneSpaceBehind()
    {
        // Arrange
        const string input = """
                             using System;

                             [assembly:CLSCompliant(true)]

                             class C
                             {
                                 [field :NonSerialized]
                                 int _f;

                                 [return:Obsolete]
                                 int M()
                                 {
                                     return 1;
                                 }

                                 [method  :  Obsolete]
                                 void N([param :Obsolete] int a, [Obsolete(message:"x")] int b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                [assembly: CLSCompliant(true)]

                                class C
                                {
                                    [field: NonSerialized]
                                    int _f;

                                    [return: Obsolete]
                                    int M()
                                    {
                                        return 1;
                                    }

                                    [method: Obsolete]
                                    void N([param: Obsolete] int a, [Obsolete(message: "x")] int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of the requirement: every arrow gets one space on each side, whatever construct it belongs to
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public void EveryArrowGetsOneSpaceOnEachSide()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 int _p;

                                 int A
                                 {
                                     get =>_p;
                                     set=>_p = value;
                                 }

                                 int P =>1;

                                 void M(int x)
                                 {
                                     Func<int, int> f = y=>y;
                                     Func<int, int> g = (int y)  =>y;
                                     var s = x switch
                                             {
                                                 1=>2,
                                                 _ =>3
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    int _p;

                                    int A
                                    {
                                        get => _p;
                                        set => _p = value;
                                    }

                                    int P => 1;

                                    void M(int x)
                                    {
                                        Func<int, int> f = y => y;
                                        Func<int, int> g = (int y) => y;
                                        var s = x switch
                                                {
                                                    1 => 2,
                                                    _ => 3
                                                };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of the requirement: the conditional operator gets one space on each side of <c>?</c> and <c>:</c>, while a
    /// nullable type, a conditional access, and <c>??</c> keep their spacing
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-018")]
    public void ConditionalOperatorGetsOneSpaceOnEachSide()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(bool b, int? n, C c)
                                 {
                                     var r = b?1:2;
                                     var s = c?.M(b, n) ?? 0;

                                     return b ? -1:n.Value;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(bool b, int? n, C c)
                                    {
                                        var r = b ? 1 : 2;
                                        var s = c?.M(b, n) ?? 0;

                                        return b ? -1 : n.Value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of the requirement: name colons get no space before and one space behind them, while an interpolation's
    /// format colon keeps its spacing
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-019")]
    public void NameColonsGetNoSpaceBeforeAndOneSpaceBehind()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool b, int c, object o)
                                 {
                                     M(b:b, c :1, o: o);

                                     var t = (A:1, B :2);
                                     var p = o is string { Length:1 };
                                     var q = $"{c:N2}";
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool b, int c, object o)
                                    {
                                        M(b: b, c: 1, o: o);

                                        var t = (A: 1, B: 2);
                                        var p = o is string { Length: 1 };
                                        var q = $"{c:N2}";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of the requirement: every <c>when</c> gets one space in front of it, and the space behind it stays as
    /// written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-020")]
    public void WhenGetsOneSpaceInFrontOfIt()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(object o, bool b)
                                 {
                                     switch (o)
                                     {
                                         case (1, 2)when b:
                                             break;

                                         case int i  when(i > 0):
                                             break;
                                     }

                                     return o switch
                                            {
                                                "x"when b => 1,
                                                _ => 0
                                            };
                                 }

                                 void N()
                                 {
                                     try
                                     {
                                     }
                                     catch (System.Exception e)when (e != null)
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(object o, bool b)
                                    {
                                        switch (o)
                                        {
                                            case (1, 2) when b:
                                                break;

                                            case int i when(i > 0):
                                                break;
                                        }

                                        return o switch
                                               {
                                                   "x" when b => 1,
                                                   _ => 0
                                               };
                                    }

                                    void N()
                                    {
                                        try
                                        {
                                        }
                                        catch (System.Exception e) when (e != null)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of the requirement: every declared name gets one space behind its type or pattern
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    public void DeclaredNamesGetOneSpaceBehindTheirTypeOrPattern()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int?a, int[]b, (int, int)c, object o)
                                 {
                                     int?z = null;

                                     N(out int?w);

                                     var p = o is int[]d;
                                     var q = o is { }e && o is [1]f;

                                     foreach (int?g in new int?[0])
                                     {
                                     }

                                     try
                                     {
                                     }
                                     catch (System.Exception?h)
                                     {
                                     }

                                     var r = from int?i in new int?[0] select i;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int? a, int[] b, (int, int) c, object o)
                                    {
                                        int? z = null;

                                        N(out int? w);

                                        var p = o is int[] d;
                                        var q = o is { } e && o is [1] f;

                                        foreach (int? g in new int?[0])
                                        {
                                        }

                                        try
                                        {
                                        }
                                        catch (System.Exception? h)
                                        {
                                        }

                                        var r = from int? i in new int?[0] select i;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of the requirement: a one-line accessor list gets one space behind its <c>{</c> and between its
    /// accessors, while an initializer's braces keep their spacing
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-022")]
    public void OneLineAccessorListsGetOneSpaceInside()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 int P {get;set;}

                                 int Q {[Obsolete] get;private set;}

                                 int[] R = new[] {1, 2};
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    int P { get; set; }

                                    int Q { [Obsolete] get; private set; }

                                    int[] R = new[] {1, 2 };
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of the requirement: a relational pattern's operator gets one space in front of its operand
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-023")]
    public void RelationalPatternOperatorsGetOneSpaceBehind()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int x)
                                 {
                                     return x is >5 and <=  10;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    bool M(int x)
                                    {
                                        return x is > 5 and <= 10;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of the requirement: modifiers and prefix keywords get one space in front of their operand
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void ModifiersAndPrefixKeywordsGetOneSpaceBehind()
    {
        // Arrange
        const string input = """
                             using System.Linq;
                             using System.Threading.Tasks;

                             static class C
                             {
                                 static void E(this(int, int) t, params(int, int)[] a)
                                 {
                                 }

                                 static async Task M(int a, int[] xs, object o, Task t)
                                 {
                                     N(async(x) => x, static(int v) => v);
                                     N(ref(a), in(a));
                                     await(t);

                                     var b = o is not"x" && o is not> 5;
                                     var c = o is var(u, w);

                                     foreach (var e in(xs))
                                     {
                                     }

                                     var q = from z in(xs) select z;
                                 }

                                 static unsafe void F(int* p)
                                 {
                                     N(out*p);

                                     var s = stackalloc[] { 1 };
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;
                                using System.Threading.Tasks;

                                static class C
                                {
                                    static void E(this (int, int) t, params (int, int)[] a)
                                    {
                                    }

                                    static async Task M(int a, int[] xs, object o, Task t)
                                    {
                                        N(async (x) => x, static (int v) => v);
                                        N(ref (a), in (a));
                                        await (t);

                                        var b = o is not "x" && o is not > 5;
                                        var c = o is var (u, w);

                                        foreach (var e in (xs))
                                        {
                                        }

                                        var q = from z in (xs) select z;
                                    }

                                    static unsafe void F(int* p)
                                    {
                                        N(out *p);

                                        var s = stackalloc [] { 1 };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the second example of the requirement: a lambda's return type behind its modifiers, a function pointer's
    /// parameter, ref expressions, and a deconstruction get one space; a local's <c>ref</c> type and a local function's modifier
    /// keep their gap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void ModifiersInFrontOfTypesAndRefExpressionsGetOneSpaceBehind()
    {
        // Arrange
        const string input = """
                             unsafe class C
                             {
                                 static (int, int) s_t;

                                 static ref int M(bool b, ref int x, ref int y)
                                 {
                                     var(u, w) = s_t;
                                     N(ref(int, int) () => ref s_t, static(int, int) () => (1, 2));

                                     delegate*<ref(int, int), void> f = null;
                                     ref var z = ref(b ? ref x : ref y);

                                     return ref(x);
                                 }

                                 static void L()
                                 {
                                     ref(int, int) r = ref s_t;

                                     static(int, int) K()
                                     {
                                         return default;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                unsafe class C
                                {
                                    static (int, int) s_t;

                                    static ref int M(bool b, ref int x, ref int y)
                                    {
                                        var (u, w) = s_t;
                                        N(ref (int, int) () => ref s_t, static (int, int) () => (1, 2));

                                        delegate*<ref (int, int), void> f = null;
                                        ref var z = ref (b ? ref x : ref y);

                                        return ref (x);
                                    }

                                    static void L()
                                    {
                                        ref(int, int) r = ref s_t;

                                        static(int, int) K()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of the requirement through the full pipeline: <c>with</c> gets one space on each side, and the
    /// initializer behind it starts its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-025")]
    public void WithGetsOneSpaceOnEachSide()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(P p, P[] ps)
                                 {
                                     var a = p.Q()with { X = 1 };
                                     var b = ps[0]with { X = 2 };
                                     var c = p!   with { X = 3 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(P p, P[] ps)
                                    {
                                        var a = p.Q() with
                                                      {
                                                          X = 1
                                                      };
                                        var b = ps[0] with
                                                      {
                                                          X = 2
                                                      };
                                        var c = p! with
                                                   {
                                                       X = 3
                                                   };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of the requirement: an explicit lambda return type gets one space in front of the parameter list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-026")]
    public void LambdaReturnTypesGetOneSpaceInFrontOfTheParameterList()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     N(int(x) => x, int[](x) => x, List<int>(x) => x, int?(x) => x, (int, int)(x) => x);
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        N(int (x) => x, int[] (x) => x, List<int> (x) => x, int? (x) => x, (int, int) (x) => x);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of the requirement: a function pointer's calling convention gets one space behind the <c>*</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-027")]
    public void FunctionPointerCallingConventionsGetOneSpaceBehindTheAsterisk()
    {
        // Arrange
        const string input = """
                             unsafe class C
                             {
                                 void M(delegate*unmanaged[Cdecl]<int, void> f, delegate*managed<int> g)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                unsafe class C
                                {
                                    void M(delegate* unmanaged[Cdecl]<int, void> f, delegate* managed<int> g)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a keyword used as an attribute target is spaced by the attribute-target rule, which decides before the keyword
    /// rule, while the same keyword in a statement keeps the keyword rule's space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-012")]
    [Requirement("FMT-SPACE-016")]
    [Requirement("FMT-SPACE-002")]
    public void AttributeTargetRuleDecidesBeforeTheKeywordRule()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 [return :Obsolete]
                                 int M(int a)
                                 {
                                     return(a);
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    [return: Obsolete]
                                    int M(int a)
                                    {
                                        return (a);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}