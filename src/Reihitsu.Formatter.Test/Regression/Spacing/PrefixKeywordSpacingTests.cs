using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the space behind a modifier or prefix keyword in front of its operand: one space, whether the author
/// wrote the pair on one line or the line-break phase joined it. Each test puts the joined pair next to the same pair written
/// on one line, so both are shown to be spaced alike
/// </summary>
[TestClass]
public class PrefixKeywordSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that the lambda modifiers <c>static</c> and <c>async</c> get one space in front of a parameter list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void LambdaModifiersGetOneSpaceInFrontOfAParameterList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N(static
                                       (int v) => v);
                                     N(static(int v) => v);
                                     N(async
                                       (x) => x);
                                     N(async(x) => x);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N(static (int v) => v);
                                        N(static (int v) => v);
                                        N(async (x) => x);
                                        N(async (x) => x);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the lambda modifiers <c>static</c> and <c>async</c> get one space in front of a verbatim parameter name
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void LambdaModifiersGetOneSpaceInFrontOfAVerbatimParameter()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N(static
                                       @x => @x);
                                     N(static@x => @x);
                                     N(async
                                       @y => @y);
                                     N(async@y => @y);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N(static @x => @x);
                                        N(static @x => @x);
                                        N(async @y => @y);
                                        N(async @y => @y);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the argument modifiers <c>ref</c> and <c>in</c> get one space in front of a parenthesized operand
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void ArgumentModifiersGetOneSpaceInFrontOfAParenthesizedOperand()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     N(ref
                                       (a));
                                     N(ref(a));
                                     N(in
                                       (a));
                                     N(in(a));
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        N(ref (a));
                                        N(ref (a));
                                        N(in (a));
                                        N(in (a));
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the argument modifiers <c>ref</c>, <c>out</c>, and <c>in</c> get one space in front of a verbatim identifier
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void ArgumentModifiersGetOneSpaceInFrontOfAVerbatimIdentifier()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int @x, int @y, int @z)
                                 {
                                     N(ref
                                       @x);
                                     N(ref@x);
                                     N(out
                                       @y);
                                     N(out@y);
                                     N(in
                                       @z);
                                     N(in@z);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int @x, int @y, int @z)
                                    {
                                        N(ref @x);
                                        N(ref @x);
                                        N(out @y);
                                        N(out @y);
                                        N(in @z);
                                        N(in @z);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the argument modifiers <c>out</c> and <c>ref</c> get one space in front of a pointer indirection
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void ArgumentModifiersGetOneSpaceInFrontOfAPointerIndirection()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 unsafe void M(int* p)
                                 {
                                     N(out
                                       *p);
                                     N(out*p);
                                     N(ref
                                       *p);
                                     N(ref*p);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    unsafe void M(int* p)
                                    {
                                        N(out *p);
                                        N(out *p);
                                        N(ref *p);
                                        N(ref *p);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the parameter modifiers <c>this</c> and <c>params</c> get one space in front of a tuple type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void ThisAndParamsGetOneSpaceInFrontOfATupleType()
    {
        // Arrange
        const string input = """
                             static class C
                             {
                                 static void E(this
                                               (int, int) t)
                                 {
                                 }

                                 static void F(this(int, int) t)
                                 {
                                 }

                                 static void G(params
                                               (int, int)[] a)
                                 {
                                 }

                                 static void H(params(int, int)[] a)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                static class C
                                {
                                    static void E(this (int, int) t)
                                    {
                                    }

                                    static void F(this (int, int) t)
                                    {
                                    }

                                    static void G(params (int, int)[] a)
                                    {
                                    }

                                    static void H(params (int, int)[] a)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the parameter modifiers <c>ref</c>, <c>in</c>, and <c>readonly</c> get one space in front of a tuple type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void RefInAndReadonlyGetOneSpaceInFrontOfATupleType()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(ref
                                        (int, int) u)
                                 {
                                 }

                                 void N(ref(int, int) u)
                                 {
                                 }

                                 void O(in
                                        (int, int) v)
                                 {
                                 }

                                 void P(in(int, int) v)
                                 {
                                 }

                                 void Q(ref readonly
                                        (int, int) w)
                                 {
                                 }

                                 void R(ref readonly(int, int) w)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(ref (int, int) u)
                                    {
                                    }

                                    void N(ref (int, int) u)
                                    {
                                    }

                                    void O(in (int, int) v)
                                    {
                                    }

                                    void P(in (int, int) v)
                                    {
                                    }

                                    void Q(ref readonly (int, int) w)
                                    {
                                    }

                                    void R(ref readonly (int, int) w)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a run of whitespace behind the parameter modifier <c>scoped</c> becomes one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void ScopedGetsOneSpaceInFrontOfItsOperand()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(scoped   ref int x)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(scoped ref int x)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <c>await</c> gets one space in front of a parenthesized operand and a verbatim identifier, in an argument
    /// and in a statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void AwaitGetsOneSpaceInFrontOfItsOperand()
    {
        // Arrange
        const string input = """
                             using System.Threading.Tasks;

                             class C
                             {
                                 async Task M(Task t, Task @u)
                                 {
                                     N(await
                                       (t));
                                     N(await(t));
                                     N(await
                                       @u);
                                     N(await@u);
                                     await(t);
                                 }
                             }
                             """;
        const string expected = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    async Task M(Task t, Task @u)
                                    {
                                        N(await (t));
                                        N(await (t));
                                        N(await @u);
                                        N(await @u);
                                        await (t);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <c>stackalloc</c> gets one space in front of the bracket of an implicitly typed array
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void StackallocGetsOneSpaceInFrontOfItsBracket()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N(stackalloc
                                       [] { 1 });
                                     N(stackalloc[] { 1 });
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N(stackalloc [] { 1 });
                                        N(stackalloc [] { 1 });
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <c>var</c> gets one space in front of a parenthesized designation, in a pattern and in a deconstruction
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void VarGetsOneSpaceInFrontOfAParenthesizedDesignation()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object o)
                                 {
                                     N(o is var
                                       (x, y));
                                     N(o is var(u, w));

                                     var(a, b) = (1, 2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o)
                                    {
                                        N(o is var (x, y));
                                        N(o is var (u, w));

                                        var (a, b) = (1, 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the pattern <c>not</c> gets one space in front of a string, a character, a verbatim identifier, and a
    /// signed or complemented constant
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void PatternNotGetsOneSpaceInFrontOfAConstant()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 const int @o = 0;

                                 void M(object a, char c, int i)
                                 {
                                     N(a is not
                                       "x");
                                     N(a is not"x");
                                     N(c is not
                                       'c');
                                     N(c is not'c');
                                     N(i is not
                                       @o);
                                     N(i is not@o);
                                     N(i is not
                                       -1);
                                     N(i is not-1);
                                     N(i is not
                                       +1);
                                     N(i is not+1);
                                     N(i is not
                                       ~1);
                                     N(i is not~1);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    const int @o = 0;

                                    void M(object a, char c, int i)
                                    {
                                        N(a is not "x");
                                        N(a is not "x");
                                        N(c is not 'c');
                                        N(c is not 'c');
                                        N(i is not @o);
                                        N(i is not @o);
                                        N(i is not -1);
                                        N(i is not -1);
                                        N(i is not +1);
                                        N(i is not +1);
                                        N(i is not ~1);
                                        N(i is not ~1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the pattern <c>not</c> gets one space in front of a relational pattern's operator
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void PatternNotGetsOneSpaceInFrontOfARelationalPattern()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int i)
                                 {
                                     N(i is not
                                       > 5);
                                     N(i is not> 5);
                                     N(i is not
                                       < 5);
                                     N(i is not< 5);
                                     N(i is not
                                       >= 5);
                                     N(i is not>= 5);
                                     N(i is not
                                       <= 5);
                                     N(i is not<= 5);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int i)
                                    {
                                        N(i is not > 5);
                                        N(i is not > 5);
                                        N(i is not < 5);
                                        N(i is not < 5);
                                        N(i is not >= 5);
                                        N(i is not >= 5);
                                        N(i is not <= 5);
                                        N(i is not <= 5);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the pattern <c>not</c> gets one space in front of a parenthesized, a property, and a list pattern, and keeps
    /// the one space in front of a word
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void PatternNotGetsOneSpaceInFrontOfADelimitedPattern()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object o, int i, int[] a)
                                 {
                                     var b = i is not(1 or 2);
                                     var c = o is not{ };
                                     var d = a is not[1];
                                     var e = o is not  null;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o, int i, int[] a)
                                    {
                                        var b = i is not (1 or 2);
                                        var c = o is not { };
                                        var d = a is not [1];
                                        var e = o is not null;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the <c>in</c> of a <c>foreach</c> statement gets one space in front of a parenthesized collection
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void ForeachInGetsOneSpaceInFrontOfItsCollection()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[] xs)
                                 {
                                     foreach (var e in(xs))
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[] xs)
                                    {
                                        foreach (var e in (xs))
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the <c>in</c> of a <c>from</c> clause gets one space in front of a parenthesized collection and a string
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void FromInGetsOneSpaceInFrontOfItsCollection()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 void M(int[] xs)
                                 {
                                     N(from x in
                                       (xs) select x);
                                     N(from x in(xs) select x);
                                     N(from c in
                                       "abc" select c);
                                     N(from c in"abc" select c);
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] xs)
                                    {
                                        N(from x in (xs) select x);
                                        N(from x in (xs) select x);
                                        N(from c in "abc" select c);
                                        N(from c in "abc" select c);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the <c>in</c> of a <c>join</c> clause gets one space in front of a parenthesized collection
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void JoinInGetsOneSpaceInFrontOfItsCollection()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 void M(int[] xs)
                                 {
                                     var q = from x in xs join y in(xs) on x equals y select x;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] xs)
                                    {
                                        var q = from x in xs join y in (xs) on x equals y select x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that keywords inside a nested argument list and an interpolation hole get one space in front of their operand
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void NestedKeywordsGetOneSpaceInFrontOfTheirOperand()
    {
        // Arrange
        const string input = """
                             using System.Threading.Tasks;

                             class C
                             {
                                 async Task M(int a, Task<int> t)
                                 {
                                     N(O(ref(a)));

                                     var s = $"{await(t)}";
                                 }
                             }
                             """;
        const string expected = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    async Task M(int a, Task<int> t)
                                    {
                                        N(O(ref (a)));

                                        var s = $"{await (t)}";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the lambda modifiers <c>static</c> and <c>async</c> get one space also in front of a tuple return type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void LambdaModifiersGetOneSpaceInFrontOfATupleReturnType()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N(static
                                       (int, int) () => (1, 2));
                                     N(static(int, int) () => (1, 2));
                                     N(async
                                       (int, int) () => (1, 2));
                                     N(async(int, int) () => (1, 2));
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N(static (int, int) () => (1, 2));
                                        N(static (int, int) () => (1, 2));
                                        N(async (int, int) () => (1, 2));
                                        N(async (int, int) () => (1, 2));
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the <c>ref</c> and the <c>readonly</c> of a lambda's return type get one space in front of a tuple type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void RefAndReadonlyOfALambdaReturnTypeGetOneSpaceInFrontOfATupleType()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 static (int, int) s_t;

                                 void M()
                                 {
                                     N(ref
                                       (int, int) () => ref s_t);
                                     N(ref(int, int) () => ref s_t);
                                     N(ref readonly
                                       (int, int) () => ref s_t);
                                     N(ref readonly(int, int) () => ref s_t);
                                     N(static ref(int, int) () => ref s_t);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    static (int, int) s_t;

                                    void M()
                                    {
                                        N(ref (int, int) () => ref s_t);
                                        N(ref (int, int) () => ref s_t);
                                        N(ref readonly (int, int) () => ref s_t);
                                        N(ref readonly (int, int) () => ref s_t);
                                        N(static ref (int, int) () => ref s_t);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the <c>ref</c> of a ref expression inside an argument gets one space in front of a parenthesized operand
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void RefOfARefExpressionInAnArgumentGetsOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool b, ref int x, ref int y)
                                 {
                                     N(b ? ref
                                       (x) : ref y);
                                     N(b ? ref(x) : ref y);
                                     N(() => ref
                                       (x));
                                     N(() => ref(x));
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool b, ref int x, ref int y)
                                    {
                                        N(b ? ref (x) : ref y);
                                        N(b ? ref (x) : ref y);
                                        N(() => ref (x));
                                        N(() => ref (x));
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the <c>ref</c> of a ref expression in a statement gets one space in front of a parenthesized operand and
    /// of a pointer indirection, as in an argument
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void RefOfARefExpressionInAStatementGetsOneSpace()
    {
        // Arrange
        const string input = """
                             unsafe class C
                             {
                                 static ref int M(bool b, ref int x, ref int y)
                                 {
                                     ref var z = ref(x);
                                     ref var w = ref(b ? ref x : ref y);
                                     z = ref(y);

                                     return ref(x);
                                 }

                                 static ref int P(int* p)
                                 {
                                     return ref*p;
                                 }
                             }
                             """;
        const string expected = """
                                unsafe class C
                                {
                                    static ref int M(bool b, ref int x, ref int y)
                                    {
                                        ref var z = ref (x);
                                        ref var w = ref (b ? ref x : ref y);
                                        z = ref (y);

                                        return ref (x);
                                    }

                                    static ref int P(int* p)
                                    {
                                        return ref *p;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the parameter modifiers of a function pointer get one space in front of a tuple type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-BREAK-031")]
    public void FunctionPointerParameterModifiersGetOneSpaceInFrontOfATupleType()
    {
        // Arrange
        const string input = """
                             unsafe class C
                             {
                                 void M()
                                 {
                                     delegate*<ref
                                         (int, int), void> a = null;
                                     delegate*<ref(int, int), void> b = null;
                                     delegate*<in
                                         (int, int), out
                                         (int, int), void> c = null;
                                     delegate*<in(int, int), out(int, int), void> d = null;
                                     delegate*<ref readonly(int, int), void> e = null;
                                 }
                             }
                             """;
        const string expected = """
                                unsafe class C
                                {
                                    void M()
                                    {
                                        delegate*<ref (int, int), void> a = null;
                                        delegate*<ref (int, int), void> b = null;
                                        delegate*<in (int, int), out (int, int), void> c = null;
                                        delegate*<in (int, int), out (int, int), void> d = null;
                                        delegate*<ref readonly (int, int), void> e = null;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the rule: a keyword whose next token is its own parenthesis keeps the gap as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void KeywordsWithTheirOwnParenthesisKeepTheirGap()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 unsafe void M(int a)
                                 {
                                     var t = typeof(C);
                                     var d = default(int);
                                     var n = nameof(a);
                                     var s = sizeof(int);
                                     var c = checked(a + 1);
                                     var u = unchecked(a + 1);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the other side of the rule: a modifier of a member keeps the gap in front of a tuple type as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void MemberModifiersInFrontOfATupleTypeKeepTheirGap()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public(int, int) G;

                                 static (int, int) F()
                                 {
                                     return default;
                                 }

                                 static(int, int) H()
                                 {
                                     return default;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the <c>ref</c> and the <c>readonly</c> of a type outside a parameter and a lambda's return type are outside
    /// the rule and keep the gap as written: a local's, a <c>foreach</c> variable's, a field's, and a return type's
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void RefTypesOfDeclarationsKeepTheirGap()
    {
        // Arrange
        const string input = """
                             using System;

                             ref struct S
                             {
                                 static (int, int) s_t;
                                 static (int, int)[] s_a;
                                 ref(int, int) _f;
                                 ref readonly(int, int) _g;
                                 ref (int, int) _h;

                                 ref(int, int) P => ref s_t;

                                 ref(int, int) this[int i] => ref s_t;

                                 ref readonly(int, int) M()
                                 {
                                     ref(int, int) r = ref s_t;
                                     ref readonly(int, int) q = ref s_t;
                                     ref (int, int) u = ref s_t;

                                     foreach (ref(int, int) e in s_a.AsSpan())
                                     {
                                     }

                                     foreach (ref readonly(int, int) e in s_a.AsSpan())
                                     {
                                     }

                                     static ref(int, int) L()
                                     {
                                         return ref s_t;
                                     }

                                     return ref r;
                                 }
                             }

                             delegate ref(int, int) D();
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the modifier of a local function is a modifier of a declaration and keeps the gap in front of a tuple type
    /// as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void LocalFunctionModifierInFrontOfATupleTypeKeepsItsGap()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     static(int, int) K()
                                     {
                                         return default;
                                     }

                                     static (int, int) L()
                                     {
                                         return default;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the <c>readonly</c> of a member is a modifier of a declaration and keeps the gap in front of a tuple type as
    /// written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void MemberReadonlyInFrontOfATupleTypeKeepsItsGap()
    {
        // Arrange
        const string input = """
                             struct S
                             {
                                 public readonly(int, int) M()
                                 {
                                     return default;
                                 }

                                 public readonly (int, int) N()
                                 {
                                     return default;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the <c>this</c> and <c>base</c> of a constructor initializer and the <c>this</c> of an indexer declaration are
    /// not modifiers: the rule does not decide their gap, and it is kept as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    public void ConstructorInitializersAndIndexerDeclarationsKeepTheirGap()
    {
        // Arrange
        const string input = """
                             class B
                             {
                                 public B(int a)
                                 {
                                 }

                                 int this[int i] => i;
                             }

                             class E : B
                             {
                                 public E(int a)
                                     : base(a)
                                 {
                                 }

                                 public E()
                                     : base (1)
                                 {
                                 }

                                 public E(int a, int b)
                                     : this(a)
                                 {
                                 }

                                 public E(string s)
                                     : this (1)
                                 {
                                 }

                                 int this [int i, int j] => i;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment the keyword carries keeps its place, and the operand follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-SPACE-013")]
    public void CommentBehindTheKeywordKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             using System.Threading.Tasks;

                             class C
                             {
                                 async Task M(Task t)
                                 {
                                     await/* a */(t);
                                 }
                             }
                             """;
        const string expected = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    async Task M(Task t)
                                    {
                                        await/* a */ (t);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}