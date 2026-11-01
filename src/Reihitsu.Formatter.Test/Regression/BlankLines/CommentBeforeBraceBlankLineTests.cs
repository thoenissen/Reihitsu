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
    /// Verifies that the blank lines around an own-line comment in front of an array initializer's brace, which is not one
    /// of the named tokens, are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-020")]
    public void KeepsBlankLineBetweenCommentAndArrayInitializerBrace()
    {
        const string input = """
                             class C
                             {
                                 int[] G =
                                 // c

                                 {
                                     1
                                 };
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] G =

                                    // c

                                    {
                                        1
                                    };
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}