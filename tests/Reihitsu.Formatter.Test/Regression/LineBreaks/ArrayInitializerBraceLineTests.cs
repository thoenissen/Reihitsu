using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the braces of an array initializer (FMT-BREAK-046) and the layout of the rows of a multi-dimensional
/// array (FMT-INDENT-023): an initializer on one line stays as it is, one that spans several lines puts its braces on lines of
/// their own, judged for each initializer on the lines the other line-break rules leave, for every kind of array initializer, with
/// comments, directives, string literals, and tokens behind its closing brace
/// </summary>
[TestClass]
public class ArrayInitializerBraceLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the first specification example: a field's initializer, declaration initializers with multi-line and single-line
    /// rows, <c>new[]</c> and <c>new int[,]</c> spanning several lines, a row whose element list wraps, and single-line
    /// initializers that stay as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void SpecExampleMultiLineArrayInitializersPutTheirBracesOnLinesOfTheirOwn()
    {
        const string input = """
                             class C
                             {
                                 int[] F = { 1,
                                 2 };

                                 void M()
                                 {
                                     int[,] b = { {
                                     1, 2
                                     }, { 3, 4 } };
                                     var e = new[] {
                                     1, 2
                                     };
                                     var f = new int[,] { { 1, 2 }, {
                                     3, 4
                                     } };
                                     int[,] g = { { 1,
                                     2 }, { 3, 4 } };
                                     int[,] a = { { 1, 2 }, { 3, 4 } };
                                     var h = new[] { 1, 2 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] F = {
                                                  1,
                                                  2
                                              };

                                    void M()
                                    {
                                        int[,] b = {
                                                       {
                                                           1, 2
                                                       },
                                                       { 3, 4 }
                                                   };
                                        var e = new[]
                                                {
                                                    1, 2
                                                };
                                        var f = new int[,]
                                                {
                                                    { 1, 2 },
                                                    {
                                                        3, 4
                                                    }
                                                };
                                        int[,] g = {
                                                       {
                                                           1,
                                                           2
                                                       },
                                                       { 3, 4 }
                                                   };
                                        int[,] a = { { 1, 2 }, { 3, 4 } };
                                        var h = new[] { 1, 2 };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the second specification example: the braces are decided on the lines the other line-break rules leave, for
    /// <c>stackalloc</c> as for <c>new</c>, and a comment in front of a row's opening brace keeps its line while the brace moves
    /// below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    [Requirement("FMT-INDENT-023")]
    public void SpecExampleBracesAreDecidedOnTheLinesTheOtherRulesLeave()
    {
        const string input = """
                             class X
                             {
                                 public int A { get; set; }
                             }

                             class C
                             {
                                 int[,] F = {
                                                /* c */ { 3,
                                                4 }
                                            };

                                 void M(int y)
                                 {
                                     var x = new[] { new X { A = 1 } };
                                     var c = new[] { (int)
                                         y };
                                     System.Span<int> s = stackalloc int[] { 1,
                                     2 };
                                     System.Span<int> t = stackalloc[] { 1,
                                     2 };
                                 }
                             }
                             """;
        const string expected = """
                                class X
                                {
                                    public int A { get; set; }
                                }

                                class C
                                {
                                    int[,] F = {
                                                   /* c */
                                                   {
                                                       3,
                                                       4
                                                   }
                                               };

                                    void M(int y)
                                    {
                                        var x = new[]
                                                {
                                                    new X
                                                    {
                                                        A = 1
                                                    }
                                                };
                                        var c = new[] { (int)y };
                                        System.Span<int> s = stackalloc int[]
                                                             {
                                                                 1,
                                                                 2
                                                             };
                                        System.Span<int> t = stackalloc []
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
    /// Verifies the visible output change of a declaration's initializer with a multi-line and a single-line row: the opening
    /// brace stays behind the <c>=</c>, every row starts its own line, and the multi-line row puts its braces on lines of their own
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void DeclarationInitializerWithAMultiLineAndASingleLineRow()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[,] b = { {
                                     1, 2
                                     }, { 3, 4 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[,] b = {
                                                       {
                                                           1, 2
                                                       },
                                                       { 3, 4 }
                                                   };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the visible output change of <c>new[]</c> spanning several lines: its opening brace starts its own line under
    /// <c>new</c>, and the elements' line stays as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void ImplicitArrayCreationSpanningSeveralLines()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var e = new[] {
                                     1, 2
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var e = new[]
                                                {
                                                    1, 2
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the visible output change of <c>new int[,]</c> with a single-line and a multi-line row
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void ArrayCreationWithASingleLineAndAMultiLineRow()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var f = new int[,] { { 1, 2 }, {
                                     3, 4
                                     } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var f = new int[,]
                                                {
                                                    { 1, 2 },
                                                    {
                                                        3, 4
                                                    }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the visible output change of a row whose element list wraps: the row is multi-line, and the line break between
    /// its elements stays as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void RowWhoseElementListWrapsKeepsTheElementLineBreak()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[,] g = { { 1,
                                     2 }, { 3, 4 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[,] g = {
                                                       {
                                                           1,
                                                           2
                                                       },
                                                       { 3, 4 }
                                                   };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the visible output change of a field's array initializer: the opening brace stays behind the <c>=</c>, and the
    /// first element and the closing brace start their own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void FieldInitializerSpanningSeveralLines()
    {
        const string input = """
                             class C
                             {
                                 int[] F = { 1,
                                 2 };
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] F = {
                                                  1,
                                                  2
                                              };
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a property's array initializer is laid out like a field's
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void PropertyInitializerSpanningSeveralLines()
    {
        const string input = """
                             class C
                             {
                                 int[] P { get; } = { 1,
                                 2 };
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] P { get; } = {
                                                           1,
                                                           2
                                                       };
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the visible output change of single-line array initializers: they stay as written, also as rows of a single-line
    /// initializer
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void SingleLineArrayInitializersStayAsWritten()
    {
        const string input = """
                             class C
                             {
                                 int[] F = { 1, 2 };

                                 void M()
                                 {
                                     int[,] a = { { 1, 2 }, { 3, 4 } };
                                     var h = new[] { 1, 2 };
                                     var d = new int[2] { 1, 2 };
                                     int[][] j = { new[] { 5 } };
                                     System.Span<int> s = stackalloc int[] { 1, 2 };
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an array creation with a size spanning several lines puts its opening brace under <c>new</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void ArrayCreationWithASizeSpanningSeveralLines()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var d = new int[2] { 1,
                                     2 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var d = new int[2]
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
    /// Verifies the visible output change of <c>stackalloc int[]</c> and <c>stackalloc[]</c> spanning several lines: the opening
    /// brace starts its own line under <c>stackalloc</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void StackallocSpanningSeveralLines()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.Span<int> s = stackalloc int[] { 1,
                                     2 };
                                     System.Span<int> t = stackalloc[] { 1,
                                     2 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        System.Span<int> s = stackalloc int[]
                                                             {
                                                                 1,
                                                                 2
                                                             };
                                        System.Span<int> t = stackalloc []
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
    /// Verifies the visible output change of an initializer the other line-break rules make multi-line or single-line: an object
    /// initializer laid out over several lines makes the array initializer multi-line, and a joined cast leaves it on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void InitializerIsJudgedOnTheLinesTheOtherRulesLeave()
    {
        const string input = """
                             class X
                             {
                                 public int A { get; set; }
                             }

                             class C
                             {
                                 void M(int y)
                                 {
                                     var x = new[] { new X { A = 1 } };
                                     var c = new[] { (int)
                                         y };
                                 }
                             }
                             """;
        const string expected = """
                                class X
                                {
                                    public int A { get; set; }
                                }

                                class C
                                {
                                    void M(int y)
                                    {
                                        var x = new[]
                                                {
                                                    new X
                                                    {
                                                        A = 1
                                                    }
                                                };
                                        var c = new[] { (int)y };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous object, a block lambda, and a switch expression that the formatter lays out over several lines make
    /// the array initializer around them multi-line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void ElementsTheFormatterLaysOutOverSeveralLinesMakeTheInitializerMultiLine()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     var z = new[] { new { A = 1 } };
                                     System.Action[] w = { () => { M(x); } };
                                     var a = new[] { x switch { 1 => 2, _ => 3 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        var z = new[]
                                                {
                                                    new
                                                    {
                                                        A = 1
                                                    }
                                                };
                                        System.Action[] w = {
                                                                () =>
                                                                {
                                                                    M(x);
                                                                }
                                                            };
                                        var a = new[]
                                                {
                                                    x switch
                                                    {
                                                        1 => 2, _ => 3
                                                    }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the opening brace of a declaration's initializer written below the <c>=</c> is joined onto the <c>=</c> line
    /// and ends it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void DeclarationBraceWrittenBelowTheEqualsStaysOnTheEqualsLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[] b =
                                     { 1,
                                     2 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[] b = {
                                                      1,
                                                      2
                                                  };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty initializer whose braces stand on two lines keeps them there, its closing brace under its opening
    /// brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void EmptyInitializerOnTwoLinesKeepsItsBraces()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[] e = {
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[] e = {
                                                  };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the visible output change of an initializer whose only line break is inside a verbatim string: it is multi-line,
    /// and the string's text stays as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void LineBreakInsideAVerbatimStringMakesTheInitializerMultiLine()
    {
        const string input = """
                             class C
                             {
                                 int[] F = { @"a
                             b".Length, 2 };
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] F = {
                                                  @"a
                                b".Length, 2
                                              };
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line raw string as an element makes the initializer multi-line, and that its content lines and its
    /// closing quotes move with its opening quotes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void MultiLineRawStringElementMovesWithItsOpeningQuotes()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     string[] r = { """
                                         a
                                         """, "b" };
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        string[] r = {
                                                         """
                                                         a
                                                         """, "b"
                                                     };
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>//</c> comment behind an element makes the initializer multi-line and keeps its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void LineCommentBehindAnElementMakesTheInitializerMultiLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     string[] s = { "x", // c
                                     "y" };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        string[] s = {
                                                         "x", // c
                                                         "y"
                                                     };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the comment positions next to the braces of a multi-line array initializer: a comment behind the token in front
    /// of an opening brace that starts its line, a comment behind an opening brace, and a comment behind the last element keep
    /// their lines, and the brace or the element on their other side moves
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void CommentsNextToTheBracesKeepTheirLines()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var e = new[] /* c */ { 1,
                                     2 };
                                     int[] a = { /* first */ 1,
                                     2 /* last */ };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var e = new[] /* c */
                                                {
                                                    1,
                                                    2
                                                };
                                        int[] a = { /* first */
                                                      1,
                                                      2 /* last */
                                                  };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the visible output change of a comment in front of a multi-line row's opening brace: the comment keeps its line,
    /// the brace moves below it, and the row is laid out from the brace's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    [Requirement("FMT-INDENT-023")]
    public void CommentInFrontOfAMultiLineRowKeepsItsLineAndTheBraceMovesBelowIt()
    {
        const string input = """
                             class C
                             {
                                 int[,] F = {
                                                /* c */ { 3,
                                                4 }
                                            };
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[,] F = {
                                                   /* c */
                                                   {
                                                       3,
                                                       4
                                                   }
                                               };
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment behind the comma that follows a multi-line row keeps its line, and the next row starts the line
    /// below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void CommentBehindTheCommaBehindARowKeepsItsLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[,] b = { { 1,
                                     2 }, /* c */ { 3, 4 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[,] b = {
                                                       {
                                                           1,
                                                           2
                                                       }, /* c */
                                                       { 3, 4 }
                                                   };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment between a multi-line row's closing brace and the comma behind it stays on the brace's line with
    /// the comma
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void CommentBetweenARowsClosingBraceAndItsCommaStaysOnTheBracesLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[,] b = { { 1,
                                     2 } /* c */, { 3, 4 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[,] b = {
                                                       {
                                                           1,
                                                           2
                                                       } /* c */,
                                                       { 3, 4 }
                                                   };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional group between rows keeps its directives at the start of their lines and its disabled text as
    /// written, while the first row and the closing brace start their own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void ConditionalGroupBetweenRowsIsKept()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[,] b = { { 1, 2 },
                             #if X
                                                    { 3, 4 },
                             #endif
                                                    { 5, 6 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[,] b = {
                                                       { 1, 2 },
                                #if X
                                                       { 3, 4 },
                                #endif
                                                       { 5, 6 }
                                                   };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a member access and a closing parenthesis behind a multi-line initializer's closing brace stay on the brace's
    /// line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void TokensBehindTheClosingBraceStayOnItsLine()
    {
        const string input = """
                             class C
                             {
                                 void M(int[] a)
                                 {
                                     var n = new[] { 1,
                                     2 }.Length;

                                     M(new[] { 1,
                                     2 });
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[] a)
                                    {
                                        var n = new[]
                                                {
                                                    1,
                                                    2
                                                }.Length;

                                        M(new[]
                                          {
                                              1,
                                              2
                                          });
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the first element of a multi-line array initializer in an argument list starts the line behind the opening
    /// brace also when it is an interpolated string, whether the author wrote it behind the brace or below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void InterpolatedStringAsTheFirstElementInAnArgumentStartsTheLineBehindTheBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(string[] a, string[] b)
                                 {
                                     M(new[] { $"a",
                                     $"b" }, null);
                                     M(null, new[]
                                             {
                                                 $"a",
                                                 $"b"
                                             });
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(string[] a, string[] b)
                                    {
                                        M(new[]
                                          {
                                              $"a",
                                              $"b"
                                          },
                                          null);
                                        M(null,
                                          new[]
                                          {
                                              $"a",
                                              $"b"
                                          });
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line array creation in an attribute argument puts its braces on lines of their own under <c>new</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void ArrayCreationInAnAttributeArgument()
    {
        const string input = """
                             class AAttribute : System.Attribute
                             {
                                 public AAttribute(int[] values)
                                 {
                                 }
                             }

                             class C
                             {
                                 [A(new[] { 1,
                                 2 })]
                                 void M()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class AAttribute : System.Attribute
                                {
                                    public AAttribute(int[] values)
                                    {
                                    }
                                }

                                class C
                                {
                                    [A(new[]
                                       {
                                           1,
                                           2
                                       })]
                                    void M()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a three-dimensional array is judged for each initializer: a multi-line middle row puts its braces on lines of
    /// their own, while the single-line row beside it and the single-line rows inside it stay as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    [Requirement("FMT-INDENT-023")]
    public void ThreeDimensionalArrayIsJudgedForEachInitializer()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[,,] c = { { { 1, 2 } }, { { 3, 4 },
                                     { 5, 6 } } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[,,] c = {
                                                        { { 1, 2 } },
                                                        {
                                                            { 3, 4 },
                                                            { 5, 6 }
                                                        }
                                                    };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that in a jagged array the element behind a multi-line <c>new[]</c> element stays on the line of its closing brace:
    /// only a row of a multi-dimensional array moves the element behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void ElementBehindAMultiLineImplicitArrayCreationInAJaggedArrayStaysOnTheBracesLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[][] j = { new[] {
                                     1
                                     }, new[] { 2 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[][] j = {
                                                        new[]
                                                        {
                                                            1
                                                        }, new[] { 2 }
                                                    };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that in a jagged array the element behind a multi-line <c>new int[]</c> element stays on the line of its closing
    /// brace, as behind a <c>new[]</c> element
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void ElementBehindAMultiLineArrayCreationInAJaggedArrayStaysOnTheBracesLine()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[][] j = { new int[] {
                                     1
                                     }, new int[] { 2 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[][] j = {
                                                        new int[]
                                                        {
                                                            1
                                                        }, new int[] { 2 }
                                                    };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that in a jagged array a line break the author wrote in front of the element behind a multi-line array creation
    /// stays as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void LineBreakInFrontOfTheElementBehindAMultiLineArrayCreationInAJaggedArrayStaysAsWritten()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[][] j = { new[] {
                                     1
                                     },
                                     new[] { 2 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[][] j = {
                                                        new[]
                                                        {
                                                            1
                                                        },
                                                        new[] { 2 }
                                                    };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the row side of the jagged-array sentence: the element behind a multi-line row of a multi-dimensional array starts
    /// the line below the row's closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void ElementBehindAMultiLineRowStartsTheLineBelowItsClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[,] r = { {
                                     1
                                     }, { 2 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[,] r = {
                                                       {
                                                           1
                                                       },
                                                       { 2 }
                                                   };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example of FMT-INDENT-023: the rows of a multi-dimensional array are laid out like complex element
    /// initializers, their closing brace under their opening brace, while the braces of an array creation inside an array
    /// initializer align to its <c>new</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-023")]
    public void SpecExampleRowsAreLaidOutLikeComplexElementInitializers()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[,] a = {
                                                    {
                                                    1, 2
                                                    },
                                                    {
                                                    3, 4
                                                    }
                                                };
                                     int[][] j = {
                                                     new[]
                                                     {
                                                     5
                                                     }
                                                 };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[,] a = {
                                                       {
                                                           1, 2
                                                       },
                                                       {
                                                           3, 4
                                                       }
                                                   };
                                        int[][] j = {
                                                        new[]
                                                        {
                                                            5
                                                        }
                                                    };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an initializer whose only line break is one the chain layout removes later in the line-break phase is on one
    /// line: the first link joined onto its root, also behind a conditional access's <c>?</c>, leaves the declaration's
    /// initializer, the array creation's, and the <c>stackalloc</c> creation's on one line, and their braces stay where they are
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void InitializerWhoseOnlyLineBreakTheChainLayoutJoinsStaysOnOneLine()
    {
        const string input = """
                             class C
                             {
                                 void M(string z)
                                 {
                                     int[] a = { z
                                         .Length };
                                     var b = new object[] { z?
                                         .Length };
                                     System.Span<int> c = stackalloc int[] { z
                                         .Length };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(string z)
                                    {
                                        int[] a = { z.Length };
                                        var b = new object[] { z?.Length };
                                        System.Span<int> c = stackalloc int[] { z.Length };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an initializer whose only line break is one the list layout removes later in the line-break phase is on one
    /// line: an argument list, a bracketed argument list, a type argument list, and a lambda's parameter list joined onto one line
    /// leave the array initializer around them on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void InitializerWhoseOnlyLineBreakTheListLayoutJoinsStaysOnOneLine()
    {
        const string input = """
                             class C
                             {
                                 void M(int[] w)
                                 {
                                     var c = new[] { N(
                                         1, 2) };
                                     var d = new[] { w[
                                         0] };
                                     var e = new[] { typeof(System.Collections.Generic.Dictionary<
                                         int, int>) };
                                     var f = new System.Func<int, int>[] { (
                                         int q) => q };
                                 }

                                 int N(int x, int y)
                                 {
                                     return x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[] w)
                                    {
                                        var c = new[] { N(1, 2) };
                                        var d = new[] { w[0] };
                                        var e = new[] { typeof(System.Collections.Generic.Dictionary<int, int>) };
                                        var f = new System.Func<int, int>[] { (int q) => q };
                                    }

                                    int N(int x, int y)
                                    {
                                        return x;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a row whose only line break is one the chain layout removes later in the line-break phase is on one line,
    /// and so is the declaration's initializer around it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void RowWhoseOnlyLineBreakTheChainLayoutJoinsStaysOnOneLine()
    {
        const string input = """
                             class C
                             {
                                 void M(string z)
                                 {
                                     int[,] h = { { z
                                         .Length, 1 }, { 2, 3 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(string z)
                                    {
                                        int[,] h = { { z.Length, 1 }, { 2, 3 } };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the judgment on the lines the phase's other rules leave: a binary operator wrap, a conditional
    /// expression's wrap, and a chain with two wrapped links stay on several lines, so the initializer around them puts its
    /// braces on lines of their own
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void InitializerWithAWrapTheOtherRulesKeepPutsItsBracesOnLinesOfTheirOwn()
    {
        const string input = """
                             class C
                             {
                                 void M(string z, int[] w)
                                 {
                                     var i = new[] { 1 +
                                         2 };
                                     var j = new[] { w.Length > 0
                                         ? 1
                                         : 2 };
                                     var k = new[] { z
                                         .Trim()
                                         .Length };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(string z, int[] w)
                                    {
                                        var i = new[]
                                                {
                                                    1
                                                    + 2
                                                };
                                        var j = new[]
                                                {
                                                    w.Length > 0
                                                        ? 1
                                                        : 2
                                                };
                                        var k = new[]
                                                {
                                                    z.Trim()
                                                     .Length
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing brace of a multi-line array initializer starts the line below a comment that starts its line in
    /// front of it, for a delimited comment as for a line comment, behind <c>new[]</c> and behind a declaration's <c>=</c>; the
    /// comment keeps its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void ClosingBraceBehindACommentThatStartsItsLineStartsTheLineBelowIt()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var a = new[] { 1,
                                     2
                                     /* c */ };
                                     int[] e = { 1,
                                     2
                                     /* c */ };
                                     var d = new[] { 1,
                                     2
                                     // c
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = new[]
                                                {
                                                    1,
                                                    2

                                                    /* c */
                                                };
                                        int[] e = {
                                                      1,
                                                      2

                                                      /* c */
                                                  };
                                        var d = new[]
                                                {
                                                    1,
                                                    2

                                                    // c
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing brace of a multi-line row starts the line below a comment that starts its line in front of it,
    /// and the row behind it starts the line below the brace's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void ClosingBraceOfARowBehindACommentThatStartsItsLineStartsTheLineBelowIt()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[,] b = { { 1,
                                     2
                                     /* c */ }, { 3, 4 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[,] b = {
                                                       {
                                                           1,
                                                           2

                                                           /* c */
                                                       },
                                                       { 3, 4 }
                                                   };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment that spans several lines behind the last element stays on that element's line, its text
    /// unchanged, and the closing brace starts the line below the comment's last line, for an array creation and for a row
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-046")]
    public void ClosingBraceBehindAMultiLineCommentBehindTheLastElementStartsTheLineBelowIt()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var c = new[] { 1,
                                     2 /* x
                                     y */ };
                                     int[,] r = { { 1,
                                     2 /* x
                                     y */ }, { 3, 4 } };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var c = new[]
                                                {
                                                    1,
                                                    2 /* x
                                        y */
                                                };
                                        int[,] r = {
                                                       {
                                                           1,
                                                           2 /* x
                                        y */
                                                       },
                                                       { 3, 4 }
                                                   };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}