using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for a comma that starts a line in an initializer, a collection expression, or a pattern
/// (FMT-BREAK-038): the comma ends the line of the element in front of it, a comment behind it goes with it, and the element
/// behind it starts its own line, in every construct of the family, nested, and next to commas that already end their line
/// </summary>
[TestClass]
public class InitializerLeadingCommaTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the reported example: a trailing comma that starts the line behind the last member of an object initializer
    /// moves behind that member, and no line holding only the comma is left
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void ReportedTrailingCommaOnItsOwnLineMovesBehindTheLastMember()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var d = new D(1, 2) { X = 3
                                     , };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var d = new D(1, 2)
                                                {
                                                    X = 3,
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a comma that starts a line ends the line of the element in front of it, a comment
    /// behind it goes with it, and a comma alone on its line leaves no line behind, in an object initializer, a collection
    /// expression, an anonymous object, a <c>with</c> initializer, and a property pattern
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void SpecificationExampleCommaThatStartsALineEndsTheElementsLine()
    {
        const string input = """
                             class C
                             {
                                 void M(object o, P p)
                                 {
                                     var a = new D(1, 2) { X = 3
                                         , };
                                     var b = new D { X = 3
                                         , /* c */ Y = 4
                                         , // d
                                         Z = 5 };
                                     int[] d = [1
                                         , 2];
                                     var h = new { A = 1
                                         , B = 2 };
                                     var q = p with { X = 1
                                         , Y = 2 };
                                     var r = o is P { X: 1
                                         , Y: 2 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o, P p)
                                    {
                                        var a = new D(1, 2)
                                                {
                                                    X = 3,
                                                };
                                        var b = new D
                                                {
                                                    X = 3, /* c */
                                                    Y = 4, // d
                                                    Z = 5
                                                };
                                        int[] d = [
                                                      1,
                                                      2
                                                  ];
                                        var h = new
                                                {
                                                    A = 1,
                                                    B = 2
                                                };
                                        var q = p with
                                                  {
                                                      X = 1,
                                                      Y = 2
                                                  };
                                        var r = o is P
                                                  {
                                                      X: 1,
                                                      Y: 2
                                                  };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example of array initializers: the comma moves, the element behind it starts the next line,
    /// and the array's other line breaks stay as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void SpecificationExampleArrayInitializerCommaMovesAndKeepsTheOtherLineBreaks()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var e = new int[]
                                     {
                                         1
                                         , 2
                                     };
                                     var f = new int[]
                                     {
                                         1, 2
                                         , 3, 4
                                     };
                                     int[] y = { 1
                                         , 2 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var e = new int[]
                                                {
                                                    1,
                                                    2
                                                };
                                        var f = new int[]
                                                {
                                                    1, 2,
                                                    3, 4
                                                };
                                        int[] y = { 1,
                                                      2 };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comma alone on its line between two members, with the member behind it on the next line, moves behind
    /// the member in front of it and leaves no line behind
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void CommaAloneOnItsLineBetweenTwoMembersLeavesNoLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var c = new D
                                             {
                                                 X = 3
                                                 ,
                                                 Y = 4
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var c = new D
                                                {
                                                    X = 3,
                                                    Y = 4
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line block comment behind a comma that starts a line goes with the comma, and the member behind
    /// the comment starts its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void MultiLineBlockCommentBehindTheCommaGoesWithIt()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var b = new D { X = 3
                                         , /* a
                                         b */ Y = 4 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var b = new D
                                                {
                                                    X = 3, /* a
                                            b */
                                                    Y = 4
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that commas that start a line in a collection initializer assigned to a member and in a complex element
    /// initializer move behind the element in front of them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void CommaInNestedCollectionAndComplexElementInitializersEndsTheElementsLine()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var n = new D { Items = { 1
                                         , 2 } };
                                     var g = new Dictionary<int, int> { { 1
                                         , 2 } };
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var n = new D
                                                {
                                                    Items = {
                                                                1,
                                                                2
                                                            }
                                                };
                                        var g = new Dictionary<int, int>
                                                {
                                                    {
                                                        1,
                                                        2
                                                    }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comma that starts a line in a collection initializer moves behind the element in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void CommaInCollectionInitializerEndsTheElementsLine()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var k = new List<int> { 1
                                         , 2 };
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var k = new List<int>
                                                {
                                                    1,
                                                    2
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that commas that start a line in an implicitly typed array and in a <c>stackalloc</c> array initializer move
    /// behind the element in front of them, and the arrays' other line breaks stay as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void CommaInImplicitArrayAndStackallocInitializersEndsTheElementsLine()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     var w = new[] { 1
                                         , 2 };
                                     Span<int> s = stackalloc int[] { 1
                                         , 2 };
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        var w = new[] { 1,
                                                    2 };
                                        Span<int> s = stackalloc int[] { 1,
                                                          2 };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comma that starts a line in a list pattern and in a property pattern introduced directly by a
    /// <c>case</c> label moves behind the subpattern in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void CommaInListPatternAndCasePropertyPatternEndsTheSubpatternsLine()
    {
        const string input = """
                             class C
                             {
                                 void M(object o)
                                 {
                                     var p = o is [1
                                         , 2];

                                     switch (o)
                                     {
                                         case D { X: 1
                                             , Y: 2 }:
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o)
                                    {
                                        var p = o is [
                                                         1,
                                                         2
                                                     ];

                                        switch (o)
                                        {
                                            case D {
                                                     X: 1,
                                                     Y: 2
                                                 }:
                                                break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that commas that start a line in an initializer inside an argument, inside a lambda body, and inside another
    /// initializer move behind the element in front of them, at every level
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void CommaInNestedInitializersEndsTheElementsLineAtEveryLevel()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     F(new D { X = 1
                                         , Y = 2 });

                                     Func<D> f = () => new D { X = 1
                                         , Y = 2 };
                                     var z = new D { X = 1
                                         , Y = new D { X = 2
                                             , Y = 3 } };
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        F(new D
                                          {
                                              X = 1,
                                              Y = 2
                                          });

                                        Func<D> f = () => new D
                                                          {
                                                              X = 1,
                                                              Y = 2
                                                          };
                                        var z = new D
                                                {
                                                    X = 1,
                                                    Y = new D
                                                        {
                                                            X = 2,
                                                            Y = 3
                                                        }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side: an initializer whose commas already end their element's line, and an array initializer
    /// without a comma that starts a line, keep their layout
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-038")]
    public void CommasThatDoNotStartALineAreNotChanged()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var c = new D
                                             {
                                                 X = 3,
                                                 Y = 4
                                             };
                                     var f = new int[]
                                             {
                                                 1, 2,
                                                 3, 4
                                             };
                                     int[] y = { 1, 2 };
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    #endregion // Tests
}