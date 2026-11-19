using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the opening parenthesis of an argument list written on the line below the code in front of it: the
/// line-break phase joins it onto that line, and an input with a comment, a directive, or disabled text in the gap is refused as
/// unsupported
/// </summary>
[TestClass]
public class ArgumentListLineStartTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the requirement's first example: the argument list of an invocation of a name and of a parenthesized expression, of an
    /// object creation and an implicit object creation, of a constructor initializer, and of a primary-constructor base type joins the
    /// line in front of it, and a blank line in the gap goes with the join
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void RequirementExampleJoinsTheArgumentListOfEveryOwner()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class B
                             {
                                 public B(int x)
                                 {
                                 }
                             }

                             class D : B
                             {
                                 public D()
                                     : base
                                     (1)
                                 {
                                 }
                             }

                             class E() : B
                                 (1);

                             class C
                             {
                                 void M(Func<int, int> f, Func<int, int> g)
                                 {
                                     Foo
                                     (4);

                                     var l = new List<int>
                                     (4);
                                     List<int> k = new

                                     (4);
                                     var r = (f ?? g)
                                     (1);
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class B
                                {
                                    public B(int x)
                                    {
                                    }
                                }

                                class D : B
                                {
                                    public D()
                                        : base(1)
                                    {
                                    }
                                }

                                class E() : B(1);

                                class C
                                {
                                    void M(Func<int, int> f, Func<int, int> g)
                                    {
                                        Foo(4);

                                        var l = new List<int>(4);
                                        List<int> k = new(4);
                                        var r = (f ?? g)(1);
                                    }

                                    void Foo(int x)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the requirement's second example: the argument list of a <c>with(…)</c> element, of a collection element, and of a
    /// complex element initializer's element joins its name, while the collection expressions and the initializer, written over several
    /// lines only because of the line break, keep one element per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void RequirementExampleJoinsWithElementAndElementsWhileCollectionsKeepOneElementPerLine()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     HashSet<int> h = [with
                                     (4), 1, 2];
                                     int[] a = [Bar
                                     (4)];
                                     var e = new Dictionary<int, int> { { Bar
                                         (1), 2 } };
                                 }

                                 int Bar(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        HashSet<int> h = [
                                                             with(4),
                                                             1,
                                                             2
                                                         ];
                                        int[] a = [
                                                      Bar(4)
                                                  ];
                                        var e = new Dictionary<int, int>
                                                {
                                                    {
                                                        Bar(1),
                                                        2
                                                    }
                                                };
                                    }

                                    int Bar(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the requirement's third example: an argument list and a conditional expression that spanned several lines only because
    /// of the line break in front of an argument list are laid out as the one line they become
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void RequirementExampleJudgesListAndConditionalAsTheLineTheyBecome()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(bool a)
                                 {
                                     Baz(Bar
                                         (1), 2);

                                     var x = a ? Bar
                                     (1) : 2;

                                     return x;
                                 }

                                 void Baz(int x, int y)
                                 {
                                 }

                                 int Bar(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    int M(bool a)
                                    {
                                        Baz(Bar(1), 2);

                                        var x = a ? Bar(1) : 2;

                                        return x;
                                    }

                                    void Baz(int x, int y)
                                    {
                                    }

                                    int Bar(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the requirement's fourth example: a single-line comment, a delimited comment, or a conditional directive with disabled
    /// text in the gap refuses the input as a whole, which is returned unchanged and reported as unsupported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    [Requirement("FMT-UNSUP-014")]
    public void RequirementExampleRefusesTheInputWhenTheGapHoldsCommentDirectiveOrDisabledText()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo // four
                                     (4);
                                     Foo /* four */
                                     (4);
                                     Foo
                             #if X
                                     (4);
                             #else
                                     (5);
                             #endif
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(AttachedPartCommentReason, input, "// four");
    }

    /// <summary>
    /// Verifies that the argument list of an invocation of a generic name joins the type argument list in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void GenericInvocationJoinsItsArgumentList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo<int>
                                     (4);
                                 }

                                 void Foo<T>(T x)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo<int>(4);
                                    }

                                    void Foo<T>(T x)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that each argument list of an invocation of an invocation joins the argument list in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void InvocationOfInvocationJoinsEveryArgumentList()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M(Func<Func<int, Func<int, int>>> make)
                                 {
                                     var m = make()
                                     (2)
                                     (3);
                                 }
                             }
                             """;

        const string expected = """
                                using System;

                                class C
                                {
                                    void M(Func<Func<int, Func<int, int>>> make)
                                    {
                                        var m = make()(2)(3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the argument list of an invocation of a null-forgiving expression joins the <c>!</c> in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void InvocationOfSuppressedExpressionJoinsItsArgumentList()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M(Func<int, int> f)
                                 {
                                     var g = f!
                                     (1);
                                 }
                             }
                             """;

        const string expected = """
                                using System;

                                class C
                                {
                                    void M(Func<int, int> f)
                                    {
                                        var g = f!(1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the argument list of a <c>nameof</c> invocation joins the keyword in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void NameofInvocationJoinsItsArgumentList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 string M(int a)
                                 {
                                     return nameof
                                     (a);
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    string M(int a)
                                    {
                                        return nameof(a);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the argument list of a <c>this</c> constructor initializer joins the keyword in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ThisConstructorInitializerJoinsItsArgumentList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public C(int x)
                                 {
                                 }

                                 public C()
                                     : this
                                     (1)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    public C(int x)
                                    {
                                    }

                                    public C()
                                        : this(1)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the argument list of a record's primary-constructor base type joins the base type in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void RecordPrimaryConstructorBaseTypeJoinsItsArgumentList()
    {
        // Arrange
        const string input = """
                             record B(int X);

                             record R(int X) : B
                                 (X);
                             """;

        const string expected = """
                                record B(int X);

                                record R(int X) : B(X);
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several blank lines between a name and its argument list go with the join
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void SeveralBlankLinesInTheGapGoWithTheJoin()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo


                                     (4);
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(4);
                                    }

                                    void Foo(int x)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list inside a lambda's block, where no list element join reaches, joins the name in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ArgumentListInsideLambdaBlockJoins()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     Run(x =>
                                     {
                                         Foo
                                         (x);
                                     });
                                 }

                                 void Foo(int x)
                                 {
                                 }

                                 void Run(Action<int> a)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Run(x =>
                                            {
                                                Foo(x);
                                            });
                                    }

                                    void Foo(int x)
                                    {
                                    }

                                    void Run(Action<int> a)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list below its name that holds another argument list below its name joins, and so does the inner one
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void NestedArgumentListsJoinBothNames()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo
                                     (Bar
                                     (1));
                                 }

                                 void Foo(int x)
                                 {
                                 }

                                 int Bar(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(Bar(1));
                                    }

                                    void Foo(int x)
                                    {
                                    }

                                    int Bar(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line argument list whose elements start on the lines below its opening parenthesis joins its name, and the
    /// list is laid out as the line its first element then stands on
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void MultiLineArgumentListJoinsItsOpeningParenthesis()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo
                                     (
                                         1,
                                         2);
                                 }

                                 void Foo(int x, int y)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1,
                                            2);
                                    }

                                    void Foo(int x, int y)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list holding a wrapped argument joins its name, and the wrapped argument stays on its own line under
    /// the first one
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ArgumentListWithWrappedArgumentJoinsItsName()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo
                                     (1,
                                         2);
                                 }

                                 void Foo(int x, int y)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1,
                                            2);
                                    }

                                    void Foo(int x, int y)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the argument list of an object creation with a collection initializer joins the type in front of it, and the
    /// initializer keeps its own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ObjectCreationWithInitializerJoinsItsArgumentList()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var l = new List<int>
                                     (4)
                                     {
                                         1
                                     };
                                 }
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var l = new List<int>(4)
                                                {
                                                    1
                                                };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list below its name inside an interpolation hole joins the name in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ArgumentListInInterpolationHoleJoins()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 string M()
                                 {
                                     return $"{Get
                                     (1)} x";
                                 }

                                 int Get(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    string M()
                                    {
                                        return $"{Get(1)} x";
                                    }

                                    int Get(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the requirement's example of an argument list inside an interpolation hole, a query clause, or an array initializer of a
    /// list element: the argument list joins its name, in a chain or not, and the list around it is laid out as the one line it becomes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void RequirementExampleJudgesListAsTheLineItBecomesAroundArgumentListInNestedConstruct()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;
                             using System.Linq;

                             class C
                             {
                                 void M(object x, List<int> list)
                                 {
                                     Baz($"{x.ToString
                                     ()}", 2);
                                     Baz($"{Bar
                                     (1)}", 2);
                                     Baz(from v in list select Bar
                                     (v), 2);
                                     Baz(new[] { x.GetHashCode
                                     () }, 2);
                                 }

                                 void Baz(object a, int b)
                                 {
                                 }

                                 int Bar(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;
                                using System.Linq;

                                class C
                                {
                                    void M(object x, List<int> list)
                                    {
                                        Baz($"{x.ToString()}", 2);
                                        Baz($"{Bar(1)}", 2);
                                        Baz(from v in list select Bar(v), 2);
                                        Baz(new[] { x.GetHashCode() }, 2);
                                    }

                                    void Baz(object a, int b)
                                    {
                                    }

                                    int Bar(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain's argument list below its name inside an interpolation hole of a later argument joins the name, and the
    /// list whose only line break it was becomes one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ChainArgumentListInInterpolationHoleOfLaterArgumentLeavesListOnOneLine()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(object x, List<int> list)
                                 {
                                     Baz(2, $"{x.ToString
                                     ()}");
                                 }

                                 void Baz(object a, object b)
                                 {
                                 }

                                 int Bar(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(object x, List<int> list)
                                    {
                                        Baz(2, $"{x.ToString()}");
                                    }

                                    void Baz(object a, object b)
                                    {
                                    }

                                    int Bar(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain's argument list below its name inside a query clause after the first joins the name, and the list whose
    /// only line break it was becomes one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ChainArgumentListInQueryClauseLeavesListOnOneLine()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;
                             using System.Linq;

                             class C
                             {
                                 void M(object x, List<int> list)
                                 {
                                     Baz(from v in list select v.ToString
                                     (), 2);
                                 }

                                 void Baz(object a, object b)
                                 {
                                 }

                                 int Bar(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;
                                using System.Linq;

                                class C
                                {
                                    void M(object x, List<int> list)
                                    {
                                        Baz(from v in list select v.ToString(), 2);
                                    }

                                    void Baz(object a, object b)
                                    {
                                    }

                                    int Bar(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list below its name, outside a chain, inside an array initializer of a list element joins the name, and
    /// the list whose only line break it was becomes one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ArgumentListInArrayInitializerLeavesListOnOneLine()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(object x, List<int> list)
                                 {
                                     Baz(new[] { Bar
                                     (1) }, 2);
                                 }

                                 void Baz(object a, object b)
                                 {
                                 }

                                 int Bar(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(object x, List<int> list)
                                    {
                                        Baz(new[] { Bar(1) }, 2);
                                    }

                                    void Baz(object a, object b)
                                    {
                                    }

                                    int Bar(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain's argument list below its name inside a stackalloc initializer of a list element joins the name, and the
    /// list whose only line break it was becomes one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ChainArgumentListInStackallocInitializerLeavesListOnOneLine()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(object x, List<int> list)
                                 {
                                     Baz(stackalloc int[] { x.GetHashCode
                                     () }.Length, 2);
                                 }

                                 void Baz(object a, object b)
                                 {
                                 }

                                 int Bar(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(object x, List<int> list)
                                    {
                                        Baz(stackalloc int[] { x.GetHashCode() }.Length, 2);
                                    }

                                    void Baz(object a, object b)
                                    {
                                    }

                                    int Bar(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a list that also holds a line break between its arguments keeps one argument per line when an argument list below
    /// its name inside an interpolation hole of an element joins the name
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ListWithLineBreakBetweenArgumentsKeepsOneArgumentPerLineAroundJoinedInterpolationHole()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(object x, List<int> list)
                                 {
                                     Baz($"{Bar
                                     (1)}",
                                     2);
                                 }

                                 void Baz(object a, object b)
                                 {
                                 }

                                 int Bar(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(object x, List<int> list)
                                    {
                                        Baz($"{Bar(1)}",
                                            2);
                                    }

                                    void Baz(object a, object b)
                                    {
                                    }

                                    int Bar(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a collection expression in a list element that spanned several lines only because of the line break in front of
    /// an argument list keeps one element per line, and the list around it keeps one argument per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void CollectionExpressionInListElementIsJudgedAsWrittenAroundJoinedArgumentList()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(object x, List<int> list)
                                 {
                                     Baz([Bar
                                     (1), 2], 3);
                                 }

                                 void Baz(object a, object b)
                                 {
                                 }

                                 int Bar(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(object x, List<int> list)
                                    {
                                        Baz([
                                                Bar(1),
                                                2
                                            ],
                                            3);
                                    }

                                    void Baz(object a, object b)
                                    {
                                    }

                                    int Bar(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list below its name in a switch expression arm joins the name in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ArgumentListInSwitchExpressionArmJoins()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(object o)
                                 {
                                     return o switch
                                     {
                                         _ => Get
                                         (1)
                                     };
                                 }

                                 int Get(int x)
                                 {
                                     return x;
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
                                                   _ => Get(1)
                                               };
                                    }

                                    int Get(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list below its name in a lambda's expression body joins the name in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ArgumentListInLambdaExpressionBodyJoins()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     Func<int> h = () => Get
                                     (1);
                                 }

                                 int Get(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int> h = () => Get(1);
                                    }

                                    int Get(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list below its name in a binary expression's right operand joins the name, and the expression becomes
    /// one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ArgumentListBehindBinaryOperatorJoins()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(bool a)
                                 {
                                     return a && Has
                                     (1);
                                 }

                                 bool Has(int x)
                                 {
                                     return x > 0;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    bool M(bool a)
                                    {
                                        return a && Has(1);
                                    }

                                    bool Has(int x)
                                    {
                                        return x > 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list below its name in a field initializer and in an expression-bodied property joins the name in
    /// front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ArgumentListInFieldInitializerAndArrowExpressionJoins()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _f = Get
                                     (1);

                                 int P => Get
                                     (2);

                                 static int Get(int x)
                                 {
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    private int _f = Get(1);

                                    int P => Get(2);

                                    static int Get(int x)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the argument list of an object creation followed by a member access joins the type in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ObjectCreationFollowedByMemberAccessJoinsItsArgumentList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int P { get; set; }

                                 int M()
                                 {
                                     return new C
                                     ().P;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    int P { get; set; }

                                    int M()
                                    {
                                        return new C().P;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment behind the opening parenthesis, which is not in the gap in front of it, does not block the join
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void CommentBehindOpeningParenthesisDoesNotBlockTheJoin()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo
                                     ( // c
                                         4);
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo( // c
                                            4);
                                    }

                                    void Foo(int x)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment on the line above the name and a comment behind the argument list, both outside the gap, do not block
    /// the join
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void CommentsOutsideTheGapDoNotBlockTheJoin()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     // above
                                     Foo
                                     (4); // behind
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        // above
                                        Foo(4); // behind
                                    }

                                    void Foo(int x)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list whose opening parenthesis stands on the line below its name and on the line above its first
    /// argument joins its name in whole-document formatting, and the list becomes the one line it then is
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ArgumentListFormattedByFirstArgumentFixJoinsInTheDocument()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo
                                     (
                                         1, 2);
                                 }

                                 void Foo(int a, int b)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Foo(1, 2);
                                    }

                                    void Foo(int a, int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment at the start of the opening parenthesis's line, which is in the gap, refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    public void DelimitedCommentInFrontOfOpeningParenthesisIsUnsupported()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo
                                     /* c */ (4);
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(AttachedPartCommentReason, input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment on a line of its own between the name and the argument list refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    public void CommentLineInTheGapIsUnsupported()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo
                                     // c
                                     (4);
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(AttachedPartCommentReason, input, "// c");
    }

    /// <summary>
    /// Verifies that a <c>#pragma</c> directive between the name and the argument list refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    public void PragmaDirectiveInTheGapIsUnsupported()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo
                             #pragma warning disable CS0168
                                     (4);
                             #pragma warning restore CS0168
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(AttachedPartCommentReason, input, "#pragma warning disable CS0168");
    }

    /// <summary>
    /// Verifies that a <c>#region</c> directive between the name and the argument list refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    public void RegionDirectiveInTheGapIsUnsupported()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo
                                     #region R
                                     (4);
                                     #endregion
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(AttachedPartCommentReason, input, "#region R");
    }

    /// <summary>
    /// Verifies that a fully inactive <c>#if false</c> block with disabled text between the name and the argument list refuses the
    /// input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    public void InactiveConditionalBlockInTheGapIsUnsupported()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo
                             #if false
                                     (5);
                                     Foo
                             #endif
                                     (4);
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(AttachedPartCommentReason, input, "#if false");
    }

    /// <summary>
    /// Verifies that argument lists written on the line of the code in front of them stay unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-039")]
    public void ArgumentListsAlreadyOnTheirNamesLineStayUnchanged()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class B
                             {
                                 public B(int x)
                                 {
                                 }
                             }

                             class D : B
                             {
                                 public D()
                                     : base(1)
                                 {
                                 }
                             }

                             class E() : B(1);

                             class C
                             {
                                 void M()
                                 {
                                     Foo(4);

                                     var l = new List<int>(4);
                                     List<int> k = new(4);
                                     HashSet<int> h = [with(4), 1, 2];
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the parenthesized type of a <c>typeof</c> expression, which is not an argument list, keeps its line break
    /// </summary>
    [TestMethod]
    public void TypeofOperandBelowTheKeywordKeepsItsLineBreak()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var t = typeof
                                     (int);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a statement whose first token is the opening parenthesis of a parenthesized expression, which is not an argument
    /// list's, is not joined onto the statement above it
    /// </summary>
    [TestMethod]
    public void StatementStartingWithParenthesizedExpressionIsNotJoined()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M(object k)
                                 {
                                     Foo(1);
                                     (k as Action)();
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>/** */</c> comment behind the name, in the gap, refuses the input in the documentation modes that parse
    /// documentation
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    public void DelimitedDocumentationCommentBehindTheNameIsUnsupported(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo /** c */
                                     (4);
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(AttachedPartCommentReason, input, "/** c */", parseOptions: CreateParseOptions(documentationMode));
    }

    /// <summary>
    /// Verifies that a <c>/** */</c> comment behind the name, in the gap, refuses the input when documentation is not parsed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    public void DelimitedDocumentationCommentBehindTheNameIsUnsupportedWithoutDocumentationParsing()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo /** c */
                                     (4);
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(AttachedPartCommentReason, input, "/** c */", parseOptions: CreateParseOptions(DocumentationMode.None));
    }

    /// <summary>
    /// Verifies that a <c>/** */</c> comment at the start of the opening parenthesis's line, in the gap, refuses the input in the
    /// documentation modes that parse documentation
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    public void DelimitedDocumentationCommentInFrontOfOpeningParenthesisIsUnsupported(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo
                                     /** c */ (4);
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(AttachedPartCommentReason, input, "/** c */", parseOptions: CreateParseOptions(documentationMode));
    }

    /// <summary>
    /// Verifies that a <c>/** */</c> comment at the start of the opening parenthesis's line, in the gap, refuses the input when
    /// documentation is not parsed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    public void DelimitedDocumentationCommentInFrontOfOpeningParenthesisIsUnsupportedWithoutDocumentationParsing()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo
                                     /** c */ (4);
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(AttachedPartCommentReason, input, "/** c */", parseOptions: CreateParseOptions(DocumentationMode.None));
    }

    /// <summary>
    /// Verifies that a <c>///</c> comment on a line of its own between the name and the argument list refuses the input in every
    /// documentation mode
    /// </summary>
    /// <param name="documentationMode">The documentation mode the input is parsed with</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    [DataRow(DocumentationMode.None, DisplayName = "None")]
    [DataRow(DocumentationMode.Parse, DisplayName = "Parse")]
    [DataRow(DocumentationMode.Diagnose, DisplayName = "Diagnose")]
    public void SingleLineDocumentationCommentInTheGapIsUnsupported(DocumentationMode documentationMode)
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo
                                     /// c
                                     (4);
                                 }

                                 void Foo(int x)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(AttachedPartCommentReason, input, "/// c", parseOptions: CreateParseOptions(documentationMode));
    }

    /// <summary>
    /// Verifies that a comment in the gap in front of the argument list of a constructor initializer, a primary-constructor base
    /// type, an object creation, and a <c>with(…)</c> element refuses the input, which is reported at the first comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    public void CommentInTheGapIsUnsupportedForEveryOwner()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class B
                             {
                                 public B(int x)
                                 {
                                 }
                             }

                             class D : B
                             {
                                 public D()
                                     : base // c
                                     (1)
                                 {
                                 }
                             }

                             class E() : B /* c */
                                 (2);

                             class C
                             {
                                 void M()
                                 {
                                     var l = new List<int> // c
                                     (3);
                                     HashSet<int> h = [with // c
                                     (4)];
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(AttachedPartCommentReason, input, "// c");
    }

    /// <summary>
    /// Creates the parse options of a fixture with the given documentation mode
    /// </summary>
    /// <param name="documentationMode">The documentation mode</param>
    /// <returns>The parse options</returns>
    private static CSharpParseOptions CreateParseOptions(DocumentationMode documentationMode)
    {
        return CSharpParseOptions.Default.WithDocumentationMode(documentationMode);
    }

    #endregion // Methods
}