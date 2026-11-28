using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for the alignment of an anonymous method's block under the anonymous method's anchor wherever the
/// anonymous method stands, outside interpolation holes of multi-line raw string literals, as a block lambda in the same
/// position is aligned
/// </summary>
[TestClass]
public class AnonymousMethodAlignmentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the spec example: as an argument, followed by another argument, behind <c>static</c> (under <c>delegate</c>), behind <c>async</c> (under <c>async</c>), with a parameter list, and as the value of a declaration, an assignment, and a return statement, the block of an anonymous method aligns under its anchor
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void SpecExampleArgumentModifiersParameterListAndValuesAlignUnderTheAnchor()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Threading.Tasks;

                             class C
                             {
                                 object M(Func<int> f, int y = 0)
                                 {
                                     var x = M(delegate { return 1; }, 2);
                                     Func<int> s = static delegate { return 1; };
                                     Func<Task<int>> t = async delegate { return 1; };
                                     Func<int, int> d = delegate (int a) { return a; };
                                     Func<int> g;
                                     g = delegate { return 1; };

                                     return M(delegate { return 1; });
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    object M(Func<int> f, int y = 0)
                                    {
                                        var x = M(delegate
                                                  {
                                                      return 1;
                                                  },
                                                  2);
                                        Func<int> s = static delegate
                                                             {
                                                                 return 1;
                                                             };
                                        Func<Task<int>> t = async delegate
                                                            {
                                                                return 1;
                                                            };
                                        Func<int, int> d = delegate (int a)
                                                           {
                                                               return a;
                                                           };
                                        Func<int> g;
                                        g = delegate
                                            {
                                                return 1;
                                            };

                                        return M(delegate
                                                 {
                                                     return 1;
                                                 });
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: as a field's value, in an object initializer, a conditional branch, a switch-expression arm, a collection expression, behind <c>+=</c>, in a chain of calls, and nested in another anonymous method, the block aligns under its own <c>delegate</c>, as a block lambda in the same position aligns
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void SpecExampleFieldInitializerBranchArmElementCompoundAssignmentChainAndNestedAlignUnderDelegate()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Linq;

                             class D
                             {
                                 public Func<int> F { get; set; }
                             }

                             class C
                             {
                                 private Func<int> _f = delegate { return 1; };

                                 event EventHandler E;

                                 int M(Func<int> f, int k = 0)
                                 {
                                     var d1 = new D { F = delegate { return 1; } };
                                     Func<int> c1 = k > 0 ? delegate { return 1; } : null;
                                     var a = k switch
                                     {
                                         0 => M(delegate { return 1; }),
                                         _ => 2,
                                     };
                                     Func<int>[] arr = [delegate { return 1; }, null];
                                     E += delegate { Console.WriteLine(); };
                                     var q = new List<int>().Select(delegate (int b) { return b; }).Where(delegate (int b) { return b > 0; });

                                     return M(delegate { return M(delegate { return 2; }); });
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;

                                class D
                                {
                                    public Func<int> F { get; set; }
                                }

                                class C
                                {
                                    private Func<int> _f = delegate
                                                           {
                                                               return 1;
                                                           };

                                    event EventHandler E;

                                    int M(Func<int> f, int k = 0)
                                    {
                                        var d1 = new D
                                                 {
                                                     F = delegate
                                                         {
                                                             return 1;
                                                         }
                                                 };
                                        Func<int> c1 = k > 0
                                                           ? delegate
                                                             {
                                                                 return 1;
                                                             }
                                                           : null;
                                        var a = k switch
                                                {
                                                    0 => M(delegate
                                                           {
                                                               return 1;
                                                           }),
                                                    _ => 2,
                                                };
                                        Func<int>[] arr = [
                                                              delegate
                                                              {
                                                                  return 1;
                                                              },
                                                              null
                                                          ];
                                        E += delegate
                                             {
                                                 Console.WriteLine();
                                             };

                                        var q = new List<int>().Select(delegate (int b)
                                                                       {
                                                                           return b;
                                                                       }).Where(delegate (int b)
                                                                                {
                                                                                    return b > 0;
                                                                                });

                                        return M(delegate
                                                 {
                                                     return M(delegate
                                                              {
                                                                  return 2;
                                                              });
                                                 });
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: with C# 10, an anonymous method in an interpolation of a regular interpolated string stays on one line, so its opening brace does not start a line and nothing is shifted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void SpecExampleAnonymousMethodInRegularStringKeepsItsLineInCSharp10()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 string M(System.Func<int> f)
                                 {
                                     return $"{M(delegate { return 1; })}";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input, null, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10));
    }

    /// <summary>
    /// Verifies that the block of an anonymous method argument followed by another argument aligns under <c>delegate</c>, in the column of the argument behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AnonymousMethodArgumentFollowedByAnotherArgumentAlignsUnderDelegate()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M(System.Func<int> f, int y)
                                 {
                                     var x = M(delegate { return 1; }, 2);

                                     return x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M(System.Func<int> f, int y)
                                    {
                                        var x = M(delegate
                                                  {
                                                      return 1;
                                                  },
                                                  2);

                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of a <c>static</c> anonymous method aligns under <c>delegate</c>, not under <c>static</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void StaticAnonymousMethodAlignsUnderDelegate()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.Func<int> s = static delegate { return 1; };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        System.Func<int> s = static delegate
                                                                    {
                                                                        return 1;
                                                                    };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of an <c>async</c> anonymous method aligns under <c>async</c>, not under <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AsyncAnonymousMethodAlignsUnderAsync()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Threading.Tasks;

                             class C
                             {
                                 void M()
                                 {
                                     Func<Task<int>> t = async delegate { return 1; };
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M()
                                    {
                                        Func<Task<int>> t = async delegate
                                                            {
                                                                return 1;
                                                            };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of an anonymous method assigned in an object initializer aligns under <c>delegate</c>, right of the initializer's braces, not in the statement's column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AnonymousMethodInObjectInitializerAlignsUnderDelegate()
    {
        // Arrange
        const string input = """
                             using System;

                             class D
                             {
                                 public Func<int> F { get; set; }
                             }

                             class C
                             {
                                 void M()
                                 {
                                     var d = new D { F = delegate { return 1; } };
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class D
                                {
                                    public Func<int> F { get; set; }
                                }

                                class C
                                {
                                    void M()
                                    {
                                        var d = new D
                                                {
                                                    F = delegate
                                                        {
                                                            return 1;
                                                        }
                                                };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the braces of an empty anonymous method argument align under <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void EmptyAnonymousMethodAlignsUnderDelegate()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M(System.Action f)
                                 {
                                     return M(delegate { });
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M(System.Action f)
                                    {
                                        return M(delegate
                                                 {
                                                 });
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of an anonymous method whose parameter list is wrapped aligns under <c>delegate</c>, below the aligned parameters
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AnonymousMethodWithWrappedParameterListAlignsUnderDelegate()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     Func<int, int, int> e = delegate (int a,
                                         int b) { return a; };
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int, int> e = delegate (int a,
                                                                          int b)
                                                                {
                                                                    return a;
                                                                };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of an anonymous method that is the expression body of a property aligns under <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AnonymousMethodAsExpressionBodyOfPropertyAlignsUnderDelegate()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 Func<int> P => delegate { return 1; };
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    Func<int> P => delegate
                                                   {
                                                       return 1;
                                                   };
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the statements, comments, and a raw string literal of an anonymous method's block move with the block under <c>delegate</c>, while the continuation line of a verbatim string, the directives, and the disabled text keep their columns, as in the block of a block lambda
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void BlockContentMovesWithTheBlockWhileLiteralLinesAndDisabledTextKeepTheirColumns()
    {
        // Arrange
        const string input = """"
                             using System;

                             class C
                             {
                                 object M(Func<string> f, int y = 0)
                                 {
                                     return M(delegate {
                                         // leading
                                         var v = @"line1
                             line2";
                                         var r = """
                                             raw
                                             """;
                             #if NEVER
                                         disabled text
                             #endif
                                         return v + r;
                                         // trailing
                                     });
                                 }
                             }
                             """";
        const string expected = """"
                                using System;

                                class C
                                {
                                    object M(Func<string> f, int y = 0)
                                    {
                                        return M(delegate
                                                 {
                                                     // leading
                                                     var v = @"line1
                                line2";
                                                     var r = """
                                                             raw
                                                             """;
                                #if NEVER
                                            disabled text
                                #endif
                                                     return v + r;

                                                     // trailing
                                                 });
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that anonymous methods whose blocks already stand under their anchor are left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void AnonymousMethodAlreadyAlignedUnderItsAnchorIsLeftUnchanged()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Threading.Tasks;

                             class C
                             {
                                 object M(Func<int> f, int y = 0)
                                 {
                                     var x = M(delegate
                                               {
                                                   return 1;
                                               },
                                               2);
                                     Func<int> s = static delegate
                                                          {
                                                              return 1;
                                                          };
                                     Func<Task<int>> t = async delegate
                                                         {
                                                             return 1;
                                                         };
                                     Func<int, int> d = delegate (int a)
                                                        {
                                                            return a;
                                                        };
                                     Func<int> g;
                                     g = delegate
                                         {
                                             return 1;
                                         };

                                     return M(delegate
                                              {
                                                  return 1;
                                              });
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the block of an anonymous method whose <c>delegate</c> line ends in a comment aligns under <c>delegate</c>, the comment staying on its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void BlockBehindCommentOnTheDelegateLineAlignsUnderDelegate()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M(System.Func<int> f)
                                 {
                                     return M(delegate // c
                                     {
                                         return 1;
                                     });
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M(System.Func<int> f)
                                    {
                                        return M(delegate // c
                                                 {
                                                     return 1;
                                                 });
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment on its own line between <c>delegate</c> and the opening brace takes the column of the brace, which aligns under <c>delegate</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    [Requirement("FMT-INDENT-006")]
    public void CommentLineInFrontOfTheBlockTakesTheColumnOfTheAlignedBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M(System.Func<int> f)
                                 {
                                     return M(delegate
                                         // d
                                         {
                                             return 1;
                                         });
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M(System.Func<int> f)
                                    {
                                        return M(delegate

                                                 // d
                                                 {
                                                     return 1;
                                                 });
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block of an anonymous method behind <c>#if</c> branches aligns under the <c>delegate</c> of the active branch, the directives and the disabled text keeping their columns
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-031")]
    public void BlockBehindConditionalDirectivesAlignsUnderTheActiveDelegate()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M(System.Func<int> f)
                                 {
                             #if A
                                     return M(delegate
                             #else
                                     return M(delegate
                             #endif
                                     {
                                         return 1;
                                     });
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M(System.Func<int> f)
                                    {
                                #if A
                                        return M(delegate
                                #else
                                        return M(delegate
                                #endif
                                                 {
                                                     return 1;
                                                 });
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}