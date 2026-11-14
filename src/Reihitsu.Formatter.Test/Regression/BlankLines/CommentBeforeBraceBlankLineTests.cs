using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for the blank lines between a comment on its own line and the brace, bracket, or parenthesis behind
/// it
/// </summary>
[TestClass]
public class CommentBeforeBraceBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the blank line between an own-line comment and a closing bracket, an initializer's opening brace, or a
    /// closing brace is removed, while the blank line between two comments stays
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    [Requirement("FMT-INDENT-006")]
    public void RemovesBlankLineBetweenCommentAndBraceOrBracket()
    {
        const string input = """
                             class C
                             {
                                 int[] F = [
                                     1
                                     // end of list

                                 ];

                                 void M(int a)
                                 {
                                     var p = new P
                                     // initializer

                                     {
                                         X = 1
                                     };
                                     a++;
                                     // c

                                     // d

                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] F = [
                                                  1

                                                  // end of list
                                              ];

                                    void M(int a)
                                    {
                                        var p = new P

                                                // initializer
                                                {
                                                    X = 1
                                                };

                                        a++;

                                        // c

                                        // d
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and a block statement that is the first statement of its
    /// block is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndFirstBlockStatementOfBlock()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     // c

                                     {
                                         B();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        // c
                                        {
                                            B();
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line behind a comment that trails the previous token is removed as a blank line directly
    /// before the closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBehindTrailingCommentBeforeClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++; // c

                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++; // c
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the author's blank lines on both sides of an own-line comment above a block statement after another
    /// statement are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    [Requirement("FMT-BLANK-020")]
    public void KeepsBlankLineBetweenCommentAndBlockStatementThatFollowsStatement()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     A();

                                     // c

                                     {
                                         B();
                                     }
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the brace or bracket behind it is removed for an
    /// accessor list, a method body, an <c>if</c> body, a block's closing brace, an object initializer's braces, the
    /// closing brackets of a collection expression and a list pattern, a switch expression's closing brace, a lambda body,
    /// and a type's closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    [Requirement("FMT-INDENT-006")]
    public void RemovesBlankLineBetweenCommentAndEveryNamedBraceOrBracket()
    {
        const string input = """
                             namespace N
                             {
                                 class C
                                 {
                                     int P
                                     // accessor

                                     {
                                         get;
                                     }

                                     void M(int a, object o)
                                     // body

                                     {
                                         if (a > 0)
                                         // if body

                                         {
                                             a++;
                                             // end block

                                         }

                                         var p = new P
                                         // initializer

                                         {
                                             X = 1
                                             // end initializer

                                         };
                                         int[] f = [
                                             1
                                             // end list

                                         ];
                                         var b = o is [
                                                 1
                                                 // end pattern

                                             ];
                                         var s = a switch
                                         {
                                             _ => 0
                                             // end switch

                                         };
                                         System.Action l = () =>
                                         // lambda

                                         {
                                         };
                                     }
                                     // end type

                                 }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    class C
                                    {
                                        int P

                                        // accessor
                                        {
                                            get;
                                        }

                                        void M(int a, object o)

                                        // body
                                        {
                                            if (a > 0)

                                            // if body
                                            {
                                                a++;

                                                // end block
                                            }

                                            var p = new P

                                                    // initializer
                                                    {
                                                        X = 1

                                                        // end initializer
                                                    };
                                            int[] f = [
                                                          1

                                                          // end list
                                                      ];
                                            var b = o is [
                                                             1

                                                         // end pattern
                                                         ];
                                            var s = a switch
                                                    {
                                                        _ => 0

                                                        // end switch
                                                    };
                                            System.Action l = () =>

                                                              // lambda
                                                              {
                                                              };
                                        }

                                        // end type
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment behind an <c>#endif</c> and a closing brace or a closing
    /// bracket is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    [Requirement("FMT-INDENT-006")]
    public void RemovesBlankLineBetweenCommentBehindDirectiveAndBraceOrBracket()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     a++;
                             #if NEVER
                                     a--;
                             #endif
                                     // c

                                 }

                                 int[] F = [
                                     1
                             #if NEVER
                                     , 2
                             #endif
                                     // c

                                 ];
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                #if NEVER
                                        a--;
                                #endif
                                        // c
                                    }

                                    int[] F = [
                                                  1
                                #if NEVER
                                        , 2
                                #endif
                                                  // c
                                              ];
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the opening brace of a property's array initializer is
    /// removed, while FMT-BLANK-011 keeps the one above the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndOpeningBraceOfPropertyArrayInitializer()
    {
        const string input = """
                             class C
                             {
                                 int[] G { get; } =
                                 // c

                                 {
                                     1
                                 };
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] G { get; } =

                                    // c
                                    {
                                        1
                                    };
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: the blank line is removed behind two comments on a line of their own, behind a comment
    /// above the opening brace of a collection initializer assigned to a member, and behind a comment above the opening
    /// brace of a complex element initializer that stays on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBehindCommentLineBeforeAssignedCollectionInitializerAndOneLineComplexElement()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     var p = new P
                                             {
                                                 L =
                                                 // c

                                                 {
                                                     1
                                                 }
                                             };
                                     var d = new Dictionary<int, int>
                                             {
                                                 { 1, 2 },

                                                 // e

                                                 { 3, 4 }
                                             };
                                     a++;

                                     /* a */ /* b */

                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        var p = new P
                                                {
                                                    L =

                                                    // c
                                                    {
                                                        1
                                                    }
                                                };
                                        var d = new Dictionary<int, int>
                                                {
                                                    { 1, 2 },

                                                    // e
                                                    { 3, 4 }
                                                };

                                        a++;

                                        /* a */ /* b */
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the opening brace of a collection initializer assigned
    /// to a member is removed, as before the opening brace of an assigned object initializer, while FMT-BLANK-011 puts a
    /// blank line above the comment that follows the <c>=</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndOpeningBraceOfAssignedCollectionInitializer()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 L =
                                                 // c

                                                 {
                                                     1
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
                                        var p = new P
                                                {
                                                    L =

                                                    // c
                                                    {
                                                        1
                                                    }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted above an own-line comment that follows the <c>=</c> of a collection initializer
    /// assigned to a member, and that the line-break phase keeps it, which gives the same layout as when the author wrote a
    /// blank line between the comment and the opening brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BREAK-030")]
    public void InsertsBlankLineAboveCommentBetweenAssignmentAndOpeningBraceOfAssignedCollectionInitializer()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 L =
                                                 // c
                                                 {
                                                     1
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
                                        var p = new P
                                                {
                                                    L =

                                                    // c
                                                    {
                                                        1
                                                    }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the opening brace of a complex element initializer on
    /// one line is removed, while the blank line above the comment stays
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndOneLineComplexElementInitializer()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 Dictionary<int, int> D = new()
                                                          {
                                                              { 1, 2 },

                                                              // c

                                                              { 3, 4 }
                                                          };
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    Dictionary<int, int> D = new()
                                                             {
                                                                 { 1, 2 },

                                                                 // c
                                                                 { 3, 4 }
                                                             };
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the opening brace of a complex element initializer that
    /// spans several lines is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndMultiLineComplexElementInitializer()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 Dictionary<int, int> D = new()
                                                          {
                                                              { 1, 2 },

                                                              // c

                                                              {
                                                                  5,
                                                                  6
                                                              }
                                                          };
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    Dictionary<int, int> D = new()
                                                             {
                                                                 { 1, 2 },

                                                                 // c
                                                                 {
                                                                     5,
                                                                     6
                                                                 }
                                                             };
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the opening brace of a complex element initializer whose
    /// object initializer the line-break phase expands is removed in the first pass and stays removed in the second
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndComplexElementThatLineBreakPhaseExpands()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 object M()
                                 {
                                     return new Dictionary<int, P>
                                            {
                                                { 1, null },

                                                // c

                                                { 2, new P { A = 2 } }
                                            };
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    object M()
                                    {
                                        return new Dictionary<int, P>
                                               {
                                                   { 1, null },

                                                   // c
                                                   {
                                                       2,
                                                       new P
                                                       {
                                                           A = 2
                                                       }
                                                   }
                                               };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that behind an own-line comment, the author's blank line before the closing brace of a collection initializer
    /// assigned to a member, written in its formatted layout, is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndClosingBraceOfAssignedCollectionInitializerInItsFormattedLayout()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 L = {
                                                         1

                                                         // c

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
                                        var p = new P
                                                {
                                                    L = {
                                                            1

                                                            // c
                                                        }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the opening brace, and between an own-line comment and the
    /// closing brace, of an array initializer in an assignment is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndBothBracesOfAssignedArrayInitializer()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[] g;
                                     g = new int[]
                                         // c

                                         {
                                             1,
                                             2
                                             // d

                                         };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int[] g;
                                        g = new int[]

                                            // c
                                            {
                                                1,
                                                2

                                                // d
                                            };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the closing brace of a field's array initializer, and between
    /// an own-line comment and the opening brace of a nested element of a multi-dimensional one, is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndClosingBraceOfFieldArrayInitializerAndNestedElementBrace()
    {
        const string input = """
                             class C
                             {
                                 int[] G =
                                 {
                                     1
                                     // d

                                 };

                                 int[,] H =
                                 {
                                     { 1 },
                                     // e

                                     { 2 }
                                 };
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] G = {
                                                  1

                                                  // d
                                              };

                                    int[,] H = {
                                                   { 1 },

                                                   // e
                                                   { 2 }
                                               };
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the closing brace of the array initializer of an implicitly
    /// typed array and of a <c>stackalloc</c> expression is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndClosingBraceOfImplicitArrayAndStackAllocInitializer()
    {
        const string input = """
                             class C
                             {
                                 unsafe void M()
                                 {
                                     var g = new[]
                                             {
                                                 1
                                                 // d

                                             };
                                     System.Span<int> s = stackalloc int[]
                                                          {
                                                              1
                                                              // d

                                                          };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    unsafe void M()
                                    {
                                        var g = new[]
                                                {
                                                    1

                                                    // d
                                                };
                                        System.Span<int> s = stackalloc int[]
                                                             {
                                                                 1

                                                                 // d
                                                             };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the opening brace, and between an own-line comment and the
    /// closing brace, of an extension block is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndBothBracesOfExtensionBlock()
    {
        const string input = """
                             static class E
                             {
                                 extension(int i)
                                 // g

                                 {
                                     public int D => i;
                                     // h

                                 }
                             }
                             """;
        const string expected = """
                                static class E
                                {
                                    extension(int i)

                                    // g
                                    {
                                        public int D => i;

                                        // h
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the opening brace of a property pattern is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndOpeningBraceOfPropertyPattern()
    {
        const string input = """
                             class C
                             {
                                 void M(object o)
                                 {
                                     if (o is C
                                         // c

                                         { })
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o)
                                    {
                                        if (o is C

                                        // c
                                        { })
                                        {
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the closing brace of a collection initializer assigned to a
    /// member is removed, as before the closing brace of any other collection initializer
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndClosingBraceOfAssignedCollectionInitializer()
    {
        const string input = """
                             class P
                             {
                                 public System.Collections.Generic.List<int> L { get; } = new();
                             }

                             class C
                             {
                                 void M()
                                 {
                                     var p = new P
                                             {
                                                 L =
                                                 {
                                                     1
                                                     // d

                                                 }
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                class P
                                {
                                    public System.Collections.Generic.List<int> L { get; } = new();
                                }

                                class C
                                {
                                    void M()
                                    {
                                        var p = new P
                                                {
                                                    L = {
                                                            1

                                                            // d
                                                        }
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the closing brace of an interpolation of a raw interpolated
    /// string is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndClosingBraceOfRawStringInterpolation()
    {
        const string input = """"
                             class C
                             {
                                 void M(int a)
                                 {
                                     var s = $$"""
                                         {{
                                             a
                                             // f

                                         }}
                                         """;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M(int a)
                                    {
                                        var s = $$"""
                                                  {{
                                                      a

                                                      // f
                                                  }}
                                                  """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and <c>else</c>, <c>catch</c>, <c>finally</c>, and a do
    /// statement's <c>while</c> is removed, while FMT-BLANK-011 keeps the one above the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndElseCatchFinallyAndDoWhile()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     if (a > 0)
                                     {
                                         a++;
                                     }
                                     // c

                                     else
                                     {
                                         a--;
                                     }

                                     try
                                     {
                                         a++;
                                     }
                                     // t

                                     catch
                                     {
                                     }
                                     // u

                                     finally
                                     {
                                     }

                                     do
                                     {
                                         a++;
                                     }
                                     // f

                                     while (a > 0);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        if (a > 0)
                                        {
                                            a++;
                                        }

                                        // c
                                        else
                                        {
                                            a--;
                                        }

                                        try
                                        {
                                            a++;
                                        }

                                        // t
                                        catch
                                        {
                                        }

                                        // u
                                        finally
                                        {
                                        }

                                        do
                                        {
                                            a++;
                                        }

                                        // f
                                        while (a > 0);
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line behind a delimited comment on its own line is removed in front of an array initializer's
    /// opening brace and in front of <c>else</c>, and that of two comments in front of an array initializer's closing brace only
    /// the blank line behind the last one is removed, while the one between the two comments stays
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBehindDelimitedCommentAndBehindLastOfTwoCommentsBeforeArrayInitializerBraceAndElse()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     int[] g;
                                     g = new int[]
                                         /* c */

                                         {
                                             1
                                             // d

                                             // e

                                         };

                                     if (a > 0)
                                     {
                                         a++;
                                     }
                                     /* c */

                                     else
                                     {
                                         a--;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        int[] g;
                                        g = new int[]

                                            /* c */
                                            {
                                                1

                                                // d

                                                // e
                                            };

                                        if (a > 0)
                                        {
                                            a++;
                                        }

                                        /* c */
                                        else
                                        {
                                            a--;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment behind an <c>#endif</c> and the closing brace of an array
    /// initializer, or <c>else</c>, is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentBehindDirectiveAndArrayInitializerBraceOrElse()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     int[] g;
                                     g = new int[]
                                         {
                                             1
                             #if NEVER
                                             , 2
                             #endif
                                             // d

                                         };

                                     if (a > 0)
                                     {
                                         a++;
                                     }
                             #if NEVER
                                     a--;
                             #endif
                                     // c

                                     else
                                     {
                                         a--;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        int[] g;
                                        g = new int[]
                                            {
                                                1
                                #if NEVER
                                                , 2
                                #endif
                                                // d
                                            };

                                        if (a > 0)
                                        {
                                            a++;
                                        }
                                #if NEVER
                                        a--;
                                #endif
                                        // c
                                        else
                                        {
                                            a--;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: the blank line behind an own-line comment is removed in front of the braces of an array
    /// initializer, a nested element of one, an extension block, a property pattern, and a collection initializer assigned to a
    /// member, in front of the closing brace of an interpolation, and in front of <c>else</c>, <c>catch</c>, <c>finally</c>, and a
    /// do statement's <c>while</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndEveryBraceAndChainedKeywordOfSpecificationExample()
    {
        const string input = """"
                             using System.Collections.Generic;

                             static class E
                             {
                                 extension(int i)
                                 // g

                                 {
                                     public int D => i;
                                     // h

                                 }
                             }

                             class P
                             {
                                 public List<int> L { get; } = new();
                             }

                             class C
                             {
                                 int[] G =
                                 {
                                     1
                                     // d

                                 };

                                 int[,] H =
                                 {
                                     { 1 },
                                     // e

                                     { 2 }
                                 };

                                 void M(object o, int a)
                                 {
                                     int[] g;
                                     g = new int[]
                                         // c

                                         {
                                             1,
                                             2
                                             // d

                                         };

                                     if (o is C
                                         // e

                                         { })
                                     {
                                     }

                                     var p = new P
                                             {
                                                 L =
                                                 {
                                                     1
                                                     // d

                                                 }
                                             };

                                     var s = $$"""
                                         {{
                                             a
                                             // f

                                         }}
                                         """;

                                     if (a > 0)
                                     {
                                         a++;
                                     }
                                     // c

                                     else
                                     {
                                         a--;
                                     }

                                     try
                                     {
                                         a++;
                                     }
                                     // t

                                     catch
                                     {
                                     }
                                     // u

                                     finally
                                     {
                                     }

                                     do
                                     {
                                         a++;
                                     }
                                     // f

                                     while (a > 0);
                                 }
                             }
                             """";
        const string expected = """"
                                using System.Collections.Generic;

                                static class E
                                {
                                    extension(int i)

                                    // g
                                    {
                                        public int D => i;

                                        // h
                                    }
                                }

                                class P
                                {
                                    public List<int> L { get; } = new();
                                }

                                class C
                                {
                                    int[] G = {
                                                  1

                                                  // d
                                              };

                                    int[,] H = {
                                                   { 1 },

                                                   // e
                                                   { 2 }
                                               };

                                    void M(object o, int a)
                                    {
                                        int[] g;
                                        g = new int[]

                                            // c
                                            {
                                                1,
                                                2

                                                // d
                                            };

                                        if (o is C

                                        // e
                                        { })
                                        {
                                        }

                                        var p = new P
                                                {
                                                    L = {
                                                            1

                                                            // d
                                                        }
                                                };

                                        var s = $$"""
                                                  {{
                                                      a

                                                      // f
                                                  }}
                                                  """;

                                        if (a > 0)
                                        {
                                            a++;
                                        }

                                        // c
                                        else
                                        {
                                            a--;
                                        }

                                        try
                                        {
                                            a++;
                                        }

                                        // t
                                        catch
                                        {
                                        }

                                        // u
                                        finally
                                        {
                                        }

                                        do
                                        {
                                            a++;
                                        }

                                        // f
                                        while (a > 0);
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the opening brace of a block statement that follows another
    /// statement is kept, while the one in front of a block behind a case label, which follows no statement, is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    [Requirement("FMT-BLANK-020")]
    public void KeepsBlankLineBetweenCommentAndBlockStatementAfterStatementButRemovesItBehindCaseLabel()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     switch (a)
                                     {
                                         case 1:
                                         // c

                                         {
                                             a++;
                                             break;
                                         }
                                     }

                                     a++;
                                     // c

                                     {
                                         a--;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                // c
                                                {
                                                    a++;

                                                    break;
                                                }
                                        }

                                        a++;

                                        // c

                                        {
                                            a--;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line between an own-line comment and the closing brace of an interpolation of a regular
    /// interpolated string is removed; the column of that comment is not decided by this requirement and is not asserted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBetweenCommentAndClosingBraceOfRegularStringInterpolation()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     var s = $"{
                                         a
                                         // f

                                         }";
                                 }
                             }
                             """;

        AssertBlankLinesAbove(input, ("}\";", false));
    }

    /// <summary>
    /// Verifies that the blank line behind a line that holds two delimited comments is removed before every named brace
    /// and bracket, like the blank line behind a line that holds one comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    [Requirement("FMT-INDENT-006")]
    public void RemovesBlankLineBehindLineOfTwoDelimitedComments()
    {
        AssertRemovesBlankLineBehindCommentLine("/* a */ /* b */");
    }

    /// <summary>
    /// Verifies that the blank line behind a line that holds a delimited comment followed by a line comment is removed
    /// before every named brace and bracket
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    [Requirement("FMT-INDENT-006")]
    public void RemovesBlankLineBehindLineOfDelimitedCommentAndLineComment()
    {
        AssertRemovesBlankLineBehindCommentLine("/* a */ // b");
    }

    /// <summary>
    /// Verifies that the blank line behind a line comment that follows the end of a multi-line comment on its line is
    /// removed before every named brace and bracket
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    [Requirement("FMT-INDENT-006")]
    public void RemovesBlankLineBehindLineCommentThatFollowsEndOfMultiLineComment()
    {
        AssertRemovesBlankLineBehindCommentLine("/* x", "   y */ // b");
    }

    /// <summary>
    /// Verifies that the blank line behind a line comment that follows a delimited documentation comment on its line is
    /// removed before every named brace and bracket
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    [Requirement("FMT-INDENT-006")]
    public void RemovesBlankLineBehindLineCommentThatFollowsDelimitedDocumentationComment()
    {
        AssertRemovesBlankLineBehindCommentLine("/** d */ // c");
    }

    /// <summary>
    /// Verifies that the blank line behind a line that holds two delimited comments is removed before the closing
    /// parenthesis of a parenthesized pattern
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBehindLineOfTwoDelimitedCommentsBeforeParenthesizedPatternParenthesis()
    {
        AssertRemovesBlankLineBehindCommentLineBeforeParenthesizedPatternParenthesis("/* a */ /* b */");
    }

    /// <summary>
    /// Verifies that the blank line behind a line that holds a delimited comment followed by a line comment is removed
    /// before the closing parenthesis of a parenthesized pattern
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBehindLineOfDelimitedCommentAndLineCommentBeforeParenthesizedPatternParenthesis()
    {
        AssertRemovesBlankLineBehindCommentLineBeforeParenthesizedPatternParenthesis("/* a */ // b");
    }

    /// <summary>
    /// Verifies that the blank line behind a line comment that follows the end of a multi-line comment is removed before
    /// the closing parenthesis of a parenthesized pattern
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBehindLineCommentThatFollowsEndOfMultiLineCommentBeforeParenthesizedPatternParenthesis()
    {
        AssertRemovesBlankLineBehindCommentLineBeforeParenthesizedPatternParenthesis("/* x", "   y */ // b");
    }

    /// <summary>
    /// Verifies that the blank line behind a line comment that follows a delimited documentation comment is removed
    /// before the closing parenthesis of a parenthesized pattern
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void RemovesBlankLineBehindLineCommentThatFollowsDelimitedDocumentationCommentBeforeParenthesizedPatternParenthesis()
    {
        AssertRemovesBlankLineBehindCommentLineBeforeParenthesizedPatternParenthesis("/** d */ // c");
    }

    /// <summary>
    /// Formats a fixture that holds the given comment line, followed by a blank line, before a collection expression's
    /// closing bracket, a method body's opening brace, an <c>if</c> body's opening brace, an object initializer's closing
    /// brace, a method body's closing brace, and a type's closing brace, and verifies that each of these blank lines is
    /// removed while FMT-BLANK-011 puts one above the comment; the comment in front of the collection expression's closing
    /// bracket moves its first line into the column of the expression's elements (FMT-INDENT-006), and every further line
    /// of it stays where it is (FMT-INDENT-007)
    /// </summary>
    /// <param name="commentLines">
    /// The lines of the comment line, the first one starting at the comment's column and every
    /// further one indented relative to it
    /// </param>
    private static void AssertRemovesBlankLineBehindCommentLine(params string[] commentLines)
    {
        const string input = """
                             class C
                             {
                                 int[] F = [
                                               1
                                           @@

                                           ];

                                 void M(int a)
                                 @@

                                 {
                                     if (a > 0)
                                     @@

                                     {
                                         a++;
                                     }

                                     var p = new P
                                             {
                                                 X = 1
                                                 @@

                                             };
                                     @@

                                 }
                                 @@

                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] F = [
                                                  1

                                              @@+4
                                              ];

                                    void M(int a)

                                    @@
                                    {
                                        if (a > 0)

                                        @@
                                        {
                                            a++;
                                        }

                                        var p = new P
                                                {
                                                    X = 1

                                                    @@
                                                };

                                        @@
                                    }

                                    @@
                                }
                                """;

        AssertRuleResult(PlaceCommentLines(input, commentLines), PlaceCommentLines(expected, commentLines));
    }

    /// <summary>
    /// Formats a fixture that holds the given comment line, followed by a blank line, before the closing parenthesis of a
    /// parenthesized pattern, and verifies that the blank line is removed
    /// </summary>
    /// <param name="commentLines">
    /// The lines of the comment line, the first one starting at the comment's column and every
    /// further one indented relative to it
    /// </param>
    private static void AssertRemovesBlankLineBehindCommentLineBeforeParenthesizedPatternParenthesis(params string[] commentLines)
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is (
                                         1 or 2
                                         @@

                                     );
                                 }
                             }
                             """;

        AssertBlankLinesAbove(PlaceCommentLines(input, commentLines), (")", false));
    }

    /// <summary>
    /// Replaces every line of the fixture that holds only the placeholder <c>@@</c> with the given comment lines, each
    /// indented by the placeholder's indentation; a placeholder <c>@@+n</c> indents the first comment line by <c>n</c>
    /// further columns and leaves every further comment line at the placeholder's indentation
    /// </summary>
    /// <param name="fixture">The fixture with placeholder lines</param>
    /// <param name="commentLines">The comment lines to place</param>
    /// <returns>The fixture with the comment lines in place of the placeholders</returns>
    private static string PlaceCommentLines(string fixture, string[] commentLines)
    {
        var lines = fixture.Split('\n');
        var result = new List<string>();

        foreach (var line in lines)
        {
            var placeholder = line.Trim();

            if (placeholder.StartsWith("@@", StringComparison.Ordinal))
            {
                var indentation = line[..line.IndexOf('@')];
                var firstLineShift = placeholder.Length > 2
                                         ? int.Parse(placeholder[3..], CultureInfo.InvariantCulture)
                                         : 0;

                result.Add($"{indentation}{new string(' ', firstLineShift)}{commentLines[0]}");
                result.AddRange(commentLines.Skip(1).Select(commentLine => indentation + commentLine));
            }
            else
            {
                result.Add(line);
            }
        }

        return string.Join("\n", result);
    }

    #endregion // Methods
}