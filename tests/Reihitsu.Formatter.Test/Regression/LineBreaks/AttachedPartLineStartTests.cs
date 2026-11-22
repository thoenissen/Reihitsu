using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for an element access, a conditional element access, or a null-forgiving operator written on the line below
/// the code in front of it: the line-break phase joins the part's first token onto that line wherever the part stands, removes a
/// line break between the <c>?</c> and the <c>[</c> of a conditional element access, and the removed line break counts for the
/// constructs around the part as the one in front of an argument list does
/// </summary>
[TestClass]
public class AttachedPartLineStartTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the requirement's first example: an element access, a conditional element access, a null-forgiving operator, one
    /// behind a blank line, and an element access that is an assignment's target each join the line in front of them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void RequirementExampleJoinsEveryAttachedPart()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             class C
                             {
                                 void M(int[] arr, int[]? n, string? s)
                                 {
                                     var a = arr
                                     [0];
                                     var b = n
                                     ?[0];
                                     var c = Get()
                                     !;
                                     var d = s

                                     ! + "";
                                     arr
                                     [0] = 1;
                                 }

                                 object? Get()
                                 {
                                     return null;
                                 }
                             }
                             """;

        const string expected = """
                                #nullable enable

                                class C
                                {
                                    void M(int[] arr, int[]? n, string? s)
                                    {
                                        var a = arr[0];
                                        var b = n?[0];
                                        var c = Get()!;
                                        var d = s! + "";
                                        arr[0] = 1;
                                    }

                                    object? Get()
                                    {
                                        return null;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the requirement's second example: a list and a conditional expression that spanned several lines only because of the
    /// join are judged as the line they become, also around an interpolation hole, and a collection expression written over several
    /// lines keeps one element per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void RequirementExampleJudgesListAndConditionalAsTheLineTheyBecomeAndKeepsCollectionPerElement()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             class C
                             {
                                 int M(bool a, int[] arr, int[]? n, string? s)
                                 {
                                     Baz(s
                                         !, 2);
                                     Baz(n
                                         ?[0], 2);
                                     Baz($"{arr
                                     [0]}", 2);

                                     var x = a ? arr
                                     [0] : 2;
                                     int[] e = [arr
                                     [0], 2];

                                     return x;
                                 }

                                 void Baz(object? x, int y)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                #nullable enable

                                class C
                                {
                                    int M(bool a, int[] arr, int[]? n, string? s)
                                    {
                                        Baz(s!, 2);
                                        Baz(n?[0], 2);
                                        Baz($"{arr[0]}", 2);

                                        var x = a ? arr[0] : 2;
                                        int[] e = [
                                                      arr[0],
                                                      2
                                                  ];

                                        return x;
                                    }

                                    void Baz(object? x, int y)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the requirement's third example: the implicit element accesses of a dictionary initializer and the brackets of
    /// <c>new[]</c>, <c>new int[3]</c>, and <c>stackalloc []</c>, which are no attached parts, are kept as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void RequirementExampleKeepsBracketsThatStartNoAttachedPart()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(int[] arr)
                                 {
                                     var d = new Dictionary<string, int>
                                             {
                                                 ["a"] = 1,
                                                 ["b"] = arr[0]
                                             };
                                     var e = new[] { 1, 2 };
                                     var f = new int[3];
                                     Span<int> g = stackalloc [] { 1, 2 };
                                     Span<int> h = stackalloc int[3];
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the change's first example: an element access behind a name, a nested element access, an indexer access on
    /// <c>this</c>, an element access behind a parenthesized expression, a null-forgiving operator behind an invocation and behind a
    /// blank line, and an element access that is an assignment's target each join the line in front of them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void ChangeExampleJoinsElementAccessesAndNullForgivingOperatorsOutsideChains()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             using System.Collections.Generic;

                             class C
                             {
                                 int this[int i] => i;

                                 void M(int[] arr, int[]? n, string? s, Dictionary<string, int[]> d)
                                 {
                                     var a = arr
                                     [0];
                                     var b = Get()
                                     !;
                                     var c = s

                                     ! + "";
                                     var e = d
                                     ["k"]
                                     [0];
                                     var f = this
                                     [0];
                                     arr
                                     [0] = 1;
                                     var g = (arr)
                                     [0];
                                 }

                                 object? Get()
                                 {
                                     return null;
                                 }
                             }
                             """;

        const string expected = """
                                #nullable enable

                                using System.Collections.Generic;

                                class C
                                {
                                    int this[int i] => i;

                                    void M(int[] arr, int[]? n, string? s, Dictionary<string, int[]> d)
                                    {
                                        var a = arr[0];
                                        var b = Get()!;
                                        var c = s! + "";
                                        var e = d["k"][0];
                                        var f = this[0];
                                        arr[0] = 1;

                                        var g = (arr)[0];
                                    }

                                    object? Get()
                                    {
                                        return null;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an element access inside a lambda's expression body and inside an interpolation hole that is not in a list
    /// joins the line in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void ElementAccessInLambdaAndInterpolationHoleJoins()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M(int[] arr)
                                 {
                                     Func<int> l = () => arr
                                     [0];
                                     var t = $"{arr
                                     [0]}";
                                 }
                             }
                             """;

        const string expected = """
                                using System;

                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        Func<int> l = () => arr[0];
                                        var t = $"{arr[0]}";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break between the <c>?</c> and the <c>[</c> of a conditional element access is removed, alone and
    /// together with a line break in front of the <c>?</c>, and behind a null-forgiving operator
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void LineBreakBetweenQuestionMarkAndBracketIsRemoved()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             class C
                             {
                                 void M(int[]? n, int[]? x)
                                 {
                                     var a = n?
                                     [0];
                                     var b = n
                                     ?
                                     [0];
                                     var c = x
                                     !
                                     ?
                                     [0];
                                 }
                             }
                             """;

        const string expected = """
                                #nullable enable

                                class C
                                {
                                    void M(int[]? n, int[]? x)
                                    {
                                        var a = n?[0];
                                        var b = n?[0];
                                        var c = x!?[0];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change's second example: a conditional expression whose branch spanned several lines only because of the line
    /// break in front of an element access or a null-forgiving operator is judged as the one line it becomes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void ConditionalExpressionIsJudgedAsTheLineItBecomes()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             class C
                             {
                                 void M(bool c, int[] arr, string? s)
                                 {
                                     var t1 = c ? arr
                                         [0] : 1;
                                     var t2 = c ? s
                                         ! : "";
                                 }
                             }
                             """;

        const string expected = """
                                #nullable enable

                                class C
                                {
                                    void M(bool c, int[] arr, string? s)
                                    {
                                        var t1 = c ? arr[0] : 1;
                                        var t2 = c ? s! : "";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change's third example: a list whose element spanned several lines only because of the line break in front of an
    /// attached part is judged as the one line it becomes — directly in the element, behind a chain, inside an interpolation hole, in
    /// a query clause after the first, and in an array initializer
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void ListIsJudgedAsTheLineItBecomes()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             using System.Collections.Generic;
                             using System.Linq;

                             class C
                             {
                                 void M(int[] arr, int[]? n, string? s, List<int> list)
                                 {
                                     Baz(s
                                         !, 2);
                                     Baz(n
                                         ?[0], 2);
                                     Baz(list.Count.ToString()
                                         !, 2);
                                     Baz($"{arr
                                     [0]}", 2);
                                     Baz($"{list.Count.ToString()
                                     [0]}", 2);
                                     Baz(from v in list select arr
                                     [v], 2);
                                     Baz(new[] { s
                                     ! }, 2);
                                 }

                                 void Baz(object? x, int y)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                #nullable enable

                                using System.Collections.Generic;
                                using System.Linq;

                                class C
                                {
                                    void M(int[] arr, int[]? n, string? s, List<int> list)
                                    {
                                        Baz(s!, 2);
                                        Baz(n?[0], 2);
                                        Baz(list.Count.ToString()!, 2);
                                        Baz($"{arr[0]}", 2);
                                        Baz($"{list.Count.ToString()[0]}", 2);
                                        Baz(from v in list select arr[v], 2);
                                        Baz(new[] { s! }, 2);
                                    }

                                    void Baz(object? x, int y)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a list whose arguments are separated by a line break of their own keeps one argument per line around a joined
    /// attached part
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void ListWithLineBreakBetweenArgumentsKeepsOneArgumentPerLineAroundJoinedPart()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[] arr)
                                 {
                                     Baz(arr
                                         [0],
                                         2);
                                 }

                                 void Baz(int x, int y)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        Baz(arr[0],
                                            2);
                                    }

                                    void Baz(int x, int y)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change's fourth example: a collection expression written over several lines only because of the line break in
    /// front of an element access or a null-forgiving operator keeps one element per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void CollectionExpressionKeepsOneElementPerLine()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             class C
                             {
                                 void M(int[] arr, string? s)
                                 {
                                     int[] e = [arr
                                     [0], 2];
                                     string[] f = [s
                                     !];
                                 }
                             }
                             """;

        const string expected = """
                                #nullable enable

                                class C
                                {
                                    void M(int[] arr, string? s)
                                    {
                                        int[] e = [
                                                      arr[0],
                                                      2
                                                  ];
                                        string[] f = [
                                                         s!
                                                     ];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a collection expression in a list element that spanned several lines only because of the line break in front of
    /// an element access keeps one element per line, and the list around it is judged as written and keeps one argument per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void CollectionExpressionInListElementIsJudgedAsWrittenAroundJoinedElementAccess()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[] arr)
                                 {
                                     Baz([arr
                                     [0], 2], 3);
                                 }

                                 void Baz(object a, object b)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        Baz([
                                                arr[0],
                                                2
                                            ],
                                            3);
                                    }

                                    void Baz(object a, object b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a null-forgiving operator joins its element whatever follows it — an element access, a conditional element
    /// access, an argument list, a closing parenthesis, and another null-forgiving operator, which stays a token of its own
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void NullForgivingOperatorJoinsWhateverFollowsIt()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             using System;

                             class C
                             {
                                 void M(string? s, int[]? a, Func<int, int>? f)
                                 {
                                     var b = a
                                     ![0];
                                     var c = a
                                     !?[0];
                                     var d = f
                                     !(1);
                                     var e = (s
                                     !).Length;
                                     var g = s
                                     !
                                     !;
                                 }
                             }
                             """;

        const string expected = """
                                #nullable enable

                                using System;

                                class C
                                {
                                    void M(string? s, int[]? a, Func<int, int>? f)
                                    {
                                        var b = a![0];
                                        var c = a!?[0];
                                        var d = f!(1);
                                        var e = (s!).Length;
                                        var g = s!!;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the null-forgiving condition: a <c>!</c> followed by <c>.</c> or <c>?.</c> starts a link
    /// operator, whose gap the chain layout lays out, and joins the line in front of it as the first link of a chain does
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void NullForgivingOperatorThatStartsALinkOperatorIsLaidOutAsALink()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             class C
                             {
                                 void M(string? s)
                                 {
                                     var a = s
                                     !.Length;
                                     var b = s
                                     !?.Length;
                                 }
                             }
                             """;

        const string expected = """
                                #nullable enable

                                class C
                                {
                                    void M(string? s)
                                    {
                                        var a = s!.Length;
                                        var b = s!?.Length;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an element access written over several lines joins its element, and its bracketed argument list becomes one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void MultiLineElementAccessJoinsItsElement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[] a)
                                 {
                                     var f = a
                                         [
                                             0
                                         ];
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(int[] a)
                                    {
                                        var f = a[0];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an element access joins the last line of a multi-line element in front of it: a raw string literal, an object
    /// creation with an initializer, and a parenthesized switch expression
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void ElementAccessJoinsTheLastLineOfAMultiLineElement()
    {
        // Arrange
        const string input = """"
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(int[] arr, object o)
                                 {
                                     var r = """
                                         raw
                                         """
                                         [0];
                                     var w = new Dictionary<string, int[]>
                                             {
                                                 ["a"] = new[] { 1 }
                                             }
                                             ["a"];
                                     var v = (o switch
                                              {
                                                  int[] x => x,
                                                  _ => arr
                                              })
                                              [0];
                                 }
                             }
                             """";

        const string expected = """"
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(int[] arr, object o)
                                    {
                                        var r = """
                                                raw
                                                """[0];
                                        var w = new Dictionary<string, int[]>
                                                {
                                                    ["a"] = new[] { 1 }
                                                }["a"];
                                        var v = (o switch
                                                 {
                                                     int[] x => x,
                                                     _ => arr
                                                 })[0];
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an element access joins its element in an expression-bodied member, a switch-expression arm, the value of a
    /// nested object initializer, a pointer element access, and with an index or a range argument
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void ElementAccessJoinsInEveryPosition()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 private int[] _arr = [];

                                 int P => _arr
                                     [0];

                                 unsafe void M(int[] arr, int* p, object o)
                                 {
                                     var i = p
                                         [0];
                                     var g = arr
                                         [^1];
                                     var h = arr
                                         [1..];
                                     var sw = o switch
                                              {
                                                  int[] y => y
                                                      [0],
                                                  _ => 0
                                              };
                                     var tt = new T
                                              {
                                                  A =
                                                  {
                                                      [0] = arr
                                                          [0]
                                                  }
                                              };
                                 }

                                 class T
                                 {
                                     public Dictionary<int, int> A { get; } = new();
                                 }
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    private int[] _arr = [];

                                    int P => _arr[0];

                                    unsafe void M(int[] arr, int* p, object o)
                                    {
                                        var i = p[0];
                                        var g = arr[^1];
                                        var h = arr[1..];
                                        var sw = o switch
                                                 {
                                                     int[] y => y[0],
                                                     _ => 0
                                                 };
                                        var tt = new T
                                                 {
                                                     A =
                                                     {
                                                         [0] = arr[0]
                                                     }
                                                 };
                                    }

                                    class T
                                    {
                                        public Dictionary<int, int> A { get; } = new();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment behind the element access on the part's own line stays behind it when the part joins its element, and
    /// the operator below keeps its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void CommentBehindThePartStaysBehindItAfterTheJoin()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[] arr)
                                 {
                                     var a = arr
                                         [0] // c
                                         + 1;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var a = arr[0] // c
                                                + 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that in a conditional expression with a collection-expression branch only the element access joins its element; the
    /// collection expression's <c>[</c>, which starts no attached part, keeps its place behind the <c>?</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void ConditionalWithCollectionExpressionBranchJoinsOnlyTheElementAccess()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool c, int[] arr)
                                 {
                                     var b = c ? [1] : arr
                                         [0];
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(bool c, int[] arr)
                                    {
                                        var b = c ? [1] : arr[0];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an element access in a chain with links joins the link in front of it inside an interpolation hole, a lambda's
    /// expression body, and a switch-expression arm, so no part of the chain keeps a line of its own
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void ElementAccessInAChainJoinsInEveryPosition()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Linq;

                             class C
                             {
                                 void M(List<int> list, object o)
                                 {
                                     var ch1 = $"{list.Where(z => z > 0)
                                         .ToArray()
                                         [0]}";
                                     Func<int> ch2 = () => list.Where(z => z > 0)
                                                               .ToArray()
                                                               [0];
                                     var ch3 = o switch
                                               {
                                                   List<int> z => z.Where(u => u > 0)
                                                                   .ToArray()
                                                                   [0],
                                                   _ => 0
                                               };
                                 }
                             }
                             """;

        const string expected = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;

                                class C
                                {
                                    void M(List<int> list, object o)
                                    {
                                        var ch1 = $"{list.Where(z => z > 0)
                                                         .ToArray()[0]}";
                                        Func<int> ch2 = () => list.Where(z => z > 0)
                                                                  .ToArray()[0];
                                        var ch3 = o switch
                                                  {
                                                      List<int> z => z.Where(u => u > 0)
                                                                      .ToArray()[0],
                                                      _ => 0
                                                  };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that attached parts already written on the line of the code in front of them stay unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-040")]
    public void AttachedPartsAlreadyOnTheirElementsLineStayUnchanged()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             class C
                             {
                                 void M(int[] arr, int[]? n, string? s)
                                 {
                                     var a = arr[0];
                                     var b = n?[0];
                                     var c = s!;
                                     var d = s!.Length;
                                     var e = n!?[0];
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}