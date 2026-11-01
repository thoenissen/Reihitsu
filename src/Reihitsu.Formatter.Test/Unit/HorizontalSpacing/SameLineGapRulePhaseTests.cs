using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.HorizontalSpacing;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.HorizontalSpacing;

/// <summary>
/// Tests the spacing rules for arrows, accessor lists, and the gaps in front of a delimited documentation comment by running
/// <see cref="HorizontalSpacingPhase"/> alone, for inputs whose final layout other phases change: expression bodies the structural
/// transforms convert to blocks, accessor lists and <c>with</c> initializers the line-break phase expands, and documentation
/// comments the documentation phase moves onto a line of their own
/// </summary>
[TestClass]
public class SameLineGapRulePhaseTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the arrows of an operator, a conversion operator, a constructor, a finalizer, a method, and a local function
    /// get one space on each side
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public void ArrowsOfEveryExpressionBodiedMemberKindGetOneSpaceOnEachSide()
    {
        const string input = """
                             class C
                             {
                                 C()=>M();

                                 ~C()=>M();

                                 public static C operator +(C a, C b)=>a;

                                 public static implicit operator int(C c) =>0;

                                 int M()=> 1;

                                 void N()
                                 {
                                     int L()=>1;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    C() => M();

                                    ~C() => M();

                                    public static C operator +(C a, C b) => a;

                                    public static implicit operator int(C c) => 0;

                                    int M() => 1;

                                    void N()
                                    {
                                        int L() => 1;
                                    }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that a one-line event accessor list gets one space behind its <c>{</c> and between its accessors
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-022")]
    public void OneLineEventAccessorListIsSpaced()
    {
        const string input = """
                             class C
                             {
                                 event System.Action E {add=>F();remove=>F();}

                                 void F()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    event System.Action E { add => F(); remove => F(); }

                                    void F()
                                    {
                                    }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that a one-line indexer accessor list gets one space behind its <c>{</c> and between its accessors
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-022")]
    public void OneLineIndexerAccessorListIsSpaced()
    {
        const string input = """
                             class C
                             {
                                 int this[int i] {get=>i;set=>F();}

                                 void F()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int this[int i] { get => i; set => F(); }

                                    void F()
                                    {
                                    }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the rule: an accessor list laid out over several lines keeps the line starts of its accessors,
    /// because the next token is not on the same line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-022")]
    public void AccessorListOverSeveralLinesIsNotSpaced()
    {
        const string input = """
                             class C
                             {
                                 int P
                                 {
                                     get;
                                     set;
                                 }
                             }
                             """;

        AssertHorizontalSpacing(input, input);
    }

    /// <summary>
    /// Verifies that a comment the <c>{</c> or an accessor's <c>;</c> carries keeps its place, and the next accessor follows it
    /// after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-022")]
    [Requirement("FMT-SPACE-013")]
    public void CommentsBehindTheBraceAndBetweenAccessorsKeepTheirPlace()
    {
        const string input = """
                             class C
                             {
                                 int P {/* a */get;/* b */set; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P {/* a */ get;/* b */ set; }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment an accessor carries keeps the whitespace in front of it, and the accessor
    /// follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-022")]
    [Requirement("FMT-SPACE-014")]
    public void DocumentationCommentsInFrontOfAccessorsKeepTheirPlace()
    {
        const string input = """
                             class C
                             {
                                 int P { /** a */get; /** b */set; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P { /** a */ get; /** b */ set; }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment the expression behind an arrow carries keeps the whitespace in front of
    /// it, and the expression follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    [Requirement("FMT-SPACE-014")]
    public void DocumentationCommentBehindTheArrowKeepsItsPlace()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.Func<int, int> f = y =>/** a */y;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        System.Func<int, int> f = y =>/** a */ y;
                                    }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment the <c>?</c> or the first branch of a conditional carries keeps the
    /// whitespace in front of it, and the token follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-018")]
    [Requirement("FMT-SPACE-014")]
    public void DocumentationCommentsAroundTheQuestionMarkKeepTheirPlace()
    {
        const string input = """
                             class C
                             {
                                 int M(bool b)
                                 {
                                     var r = b /** x */?1 : 2;

                                     return b ?/** y */1 : 2;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(bool b)
                                    {
                                        var r = b /** x */ ? 1 : 2;

                                        return b ?/** y */ 1 : 2;
                                    }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment <c>when</c> carries keeps the whitespace in front of it, and <c>when</c>
    /// follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-020")]
    [Requirement("FMT-SPACE-014")]
    public void DocumentationCommentInFrontOfWhenKeepsItsPlace()
    {
        const string input = """
                             class C
                             {
                                 void M(object o, bool b)
                                 {
                                     switch (o)
                                     {
                                         case (1, 2) /** a */when b:
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o, bool b)
                                    {
                                        switch (o)
                                        {
                                            case (1, 2) /** a */ when b:
                                                break;
                                        }
                                    }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment the operand of <c>await</c> carries keeps the whitespace in front of it,
    /// and the operand follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-024")]
    [Requirement("FMT-SPACE-014")]
    public void DocumentationCommentInFrontOfTheOperandOfAwaitKeepsItsPlace()
    {
        const string input = """
                             class C
                             {
                                 async System.Threading.Tasks.Task M(System.Threading.Tasks.Task t)
                                 {
                                     await /** a */(t);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    async System.Threading.Tasks.Task M(System.Threading.Tasks.Task t)
                                    {
                                        await /** a */ (t);
                                    }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment the parameter list of a lambda with a return type carries keeps the
    /// whitespace in front of it, and the parameter list follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-026")]
    [Requirement("FMT-SPACE-014")]
    public void DocumentationCommentInFrontOfTheLambdaParameterListKeepsItsPlace()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N(int /** a */(x) => x);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N(int /** a */ (x) => x);
                                    }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment the calling convention of a function pointer carries keeps the whitespace
    /// in front of it, and the calling convention follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-027")]
    [Requirement("FMT-SPACE-014")]
    public void DocumentationCommentInFrontOfTheCallingConventionKeepsItsPlace()
    {
        const string input = """
                             unsafe class C
                             {
                                 void M(delegate* /** a */managed<int> f)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                unsafe class C
                                {
                                    void M(delegate* /** a */ managed<int> f)
                                    {
                                    }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies the example of the requirement: <c>with</c> gets one space on each side. The spacing phase runs alone, because the
    /// line-break phase lays out the initializer of a <c>with</c> expression over several lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-025")]
    public void WithRequirementExampleGetsOneSpaceOnEachSide()
    {
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
                                        var a = p.Q() with { X = 1 };
                                        var b = ps[0] with { X = 2 };
                                        var c = p! with { X = 3 };
                                    }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that <c>with</c> behind the closing brace of an object initializer gets one space in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-025")]
    public void WithBehindAnObjectInitializerGetsOneSpaceInFrontOfIt()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var a = new P { }with { };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = new P { } with { };
                                    }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that the arrow of a lambda with an attribute gets one space on each side
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public void ArrowOfAnAttributedLambdaGetsOneSpaceOnEachSide()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var f = [System.Obsolete] (int x)=>x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var f = [System.Obsolete] (int x) => x;
                                    }
                                }
                                """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Applies horizontal spacing to the given input and asserts that the result matches the expected output
    /// </summary>
    /// <param name="input">The input C# code</param>
    /// <param name="expected">The expected output</param>
    private void AssertHorizontalSpacing(string input, string expected)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var result = HorizontalSpacingPhase.Execute(tree.GetRoot(TestContext.CancellationToken), TestContext.CancellationToken);

        Assert.AreEqual(expected, result.ToFullString());
    }

    #endregion // Methods
}