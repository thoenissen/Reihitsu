using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the arrow <c>=&gt;</c>: one space on each side of it, whatever construct it belongs to and
/// whether the author wrote it on one line or the line-break phase joined it
/// </summary>
[TestClass]
public class ArrowSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that the arrows of simple and parenthesized lambdas get one space on each side, however the author spaced them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public void LambdaArrowsGetOneSpaceOnEachSide()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     Func<int, int> a = y=>y;
                                     Func<int, int> b = y =>y;
                                     Func<int, int> c = y=> y;
                                     Func<int, int> d = y  =>  y;
                                     Func<int, int, int> e = (x, y)=>x;
                                     Func<int> f = ()=>1;
                                     Func<int, int> g = (int y)=>y;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> a = y => y;
                                        Func<int, int> b = y => y;
                                        Func<int, int> c = y => y;
                                        Func<int, int> d = y => y;
                                        Func<int, int, int> e = (x, y) => x;
                                        Func<int> f = () => 1;
                                        Func<int, int> g = (int y) => y;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the arrows of lambdas with a modifier, an attribute, or an explicit return type get one space on each side
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public void ArrowsOfLambdasWithModifiersAttributesAndReturnTypesGetOneSpaceOnEachSide()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Threading.Tasks;

                             class C
                             {
                                 void M(Task t)
                                 {
                                     N(static y=>y);
                                     N(async ()=>await t);
                                     N(static (int y)=>y);
                                     N(int (int y)=>y);
                                     N(static int (int y)=>y);
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M(Task t)
                                    {
                                        N(static y => y);
                                        N(async () => await t);
                                        N(static (int y) => y);
                                        N(int (int y) => y);
                                        N(static int (int y) => y);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the arrows of switch-expression arms get one space on each side, also behind a <c>when</c> guard
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public void SwitchExpressionArmArrowsGetOneSpaceOnEachSide()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x, bool b)
                                 {
                                     return x switch
                                            {
                                                1=>2,
                                                2 =>3,
                                                3=> 4,
                                                int i when b=>i,
                                                _  =>  0
                                            };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x, bool b)
                                    {
                                        return x switch
                                               {
                                                   1 => 2,
                                                   2 => 3,
                                                   3 => 4,
                                                   int i when b => i,
                                                   _ => 0
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the arrows of expression-bodied properties and indexers get one space on each side
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public void PropertyAndIndexerArrowsGetOneSpaceOnEachSide()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int P =>1;

                                 int Q=>1;

                                 int R=> 1;

                                 int this[int i]=>i;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P => 1;

                                    int Q => 1;

                                    int R => 1;

                                    int this[int i] => i;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the hand-written arrows of <c>get</c>, <c>set</c>, <c>init</c>, <c>add</c>, and <c>remove</c> accessors get
    /// one space on each side
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public void HandWrittenAccessorArrowsGetOneSpaceOnEachSide()
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

                                 int B
                                 {
                                     get=> _p;
                                     init  =>  _p = value;
                                 }

                                 event Action E
                                 {
                                     add=>F();
                                     remove =>F();
                                 }

                                 void F()
                                 {
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

                                    int B
                                    {
                                        get => _p;
                                        init => _p = value;
                                    }

                                    event Action E
                                    {
                                        add => F();
                                        remove => F();
                                    }

                                    void F()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the arrows the accessor and get-only-member transforms create get one space on each side, also where the
    /// author wrote no whitespace around the removed tokens
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    [Requirement("FMT-STRUCT-026")]
    public void ArrowsCreatedByStructuralTransformsGetOneSpaceOnEachSide()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 string _s;

                                 string P
                                 {
                                     get{return _s;}
                                     set{_s = value;}
                                 }

                                 string Q{get{return _s;}}

                                 string this[int i]{get{return _s;}}
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    string _s;

                                    string P
                                    {
                                        get => _s;
                                        set => _s = value;
                                    }

                                    string Q => _s;

                                    string this[int i] => _s;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the arrow of a method and of a local function whose expression body is kept, because their return type is
    /// a custom task type, gets one space on each side
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public void ArrowsOfKeptMethodAndLocalFunctionExpressionBodiesGetOneSpaceOnEachSide()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 async Awaitable M()=>await N();

                                 void O()
                                 {
                                     async Awaitable L() =>await N();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    async Awaitable M() => await N();

                                    void O()
                                    {
                                        async Awaitable L() => await N();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lambda wrapped behind its tight arrow is joined and spaced like the same lambda written on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    [Requirement("FMT-BREAK-031")]
    public void JoinedLambdaArrowIsSpacedLikeTheSameArrowOnOneLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N(z=>
                                       z);
                                     N(z=>z);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N(z => z);
                                        N(z => z);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a property arrow wrapped in front of or behind a tight arrow is joined and spaced like the same arrow on
    /// one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    [Requirement("FMT-BREAK-026")]
    public void JoinedPropertyArrowIsSpacedLikeTheSameArrowOnOneLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int P
                                     =>1;

                                 int Q=>
                                     1;

                                 int R=>1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P => 1;

                                    int Q => 1;

                                    int R => 1;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that arrows inside an argument, a nested argument list, an interpolation hole, and an array initializer get one
    /// space on each side
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public void NestedArrowsGetOneSpaceOnEachSide()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     N(O(y=>y), x=>x);

                                     var s = $"{N(y=>y)}";
                                     var a = new Func<int, int>[] { y=>y };
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        N(O(y => y), x => x);

                                        var s = $"{N(y => y)}";
                                        var a = new Func<int, int>[] { y => y };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that arrows already spaced as the rule requires are left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public void ArrowsAlreadySpacedStayUnchanged()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 int P => 1;

                                 void M(int x)
                                 {
                                     Func<int, int> f = y => y;
                                     var s = x switch
                                             {
                                                 1 => 2,
                                                 _ => 3
                                             };
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment the token in front of the arrow carries keeps its place, and the arrow follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    [Requirement("FMT-SPACE-013")]
    public void CommentInFrontOfTheArrowKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     Func<int, int> f = y/* a */=>y;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> f = y/* a */ => y;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment the arrow carries behind it keeps its place, and the expression follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    [Requirement("FMT-SPACE-013")]
    public void CommentBehindTheArrowKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     Func<int, int> f = y =>/* a */y;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> f = y =>/* a */ y;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}