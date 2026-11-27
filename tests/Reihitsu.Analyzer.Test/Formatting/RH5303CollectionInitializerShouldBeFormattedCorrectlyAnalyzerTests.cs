using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH5303CollectionInitializerShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer, RH5303CollectionInitializerShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that an explicit collection initializer reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-001")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyDiagnosticAndCodeFixForExplicitCollectionInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = {|#0:new List<int>
                                        {
                                        1,
                                            2
                                        }|};
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;

                                  internal class Example
                                  {
                                      private static void Method()
                                      {
                                          var values = new List<int>
                                                       {
                                                           1,
                                                           2
                                                       };
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a target-typed collection initializer reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-001")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyDiagnosticAndCodeFixForTargetTypedCollectionInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        List<int> values = {|#0:new()
                                        {
                                        1,
                                            2
                                        }|};
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;

                                  internal class Example
                                  {
                                      private static void Method()
                                      {
                                          List<int> values = new()
                                                             {
                                                                 1,
                                                                 2
                                                             };
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned expression inside a multi-line complex element reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyDiagnosticAndCodeFixForComplexElementInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data
                                {
                                    public int A { get; set; }
                                }

                                internal class Example
                                {
                                    private static void Method(Data existingValue)
                                    {
                                        var values = new Dictionary<Data, Data>()
                                                     {
                                                         {
                                                             new Data
                                                             {
                                                                 A = 1
                                                             },
                                                         {|#0:existingValue|}
                                                         }
                                                     };
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;

                                  internal class Data
                                  {
                                      public int A { get; set; }
                                  }

                                  internal class Example
                                  {
                                      private static void Method(Data existingValue)
                                      {
                                          var values = new Dictionary<Data, Data>()
                                                       {
                                                           {
                                                               new Data
                                                               {
                                                                   A = 1
                                                               },
                                                               existingValue
                                                           }
                                                       };
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned closing brace on a multi-line complex element reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyDiagnosticAndCodeFixForComplexElementClosingBrace()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var values = new Dictionary<Data, Data>()
                                                     {
                                                         {|#0:{
                                                             existingKey,
                                                             existingValue
                                                             }|}
                                                     };
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;

                                  internal class Data;

                                  internal class Example
                                  {
                                      private static void Method(Data existingKey, Data existingValue)
                                      {
                                          var values = new Dictionary<Data, Data>()
                                                       {
                                                           {
                                                               existingKey,
                                                               existingValue
                                                           }
                                                       };
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a correctly formatted explicit collection initializer does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-001")]
    [Requirement("RH5303-002")]
    public async Task VerifyNoDiagnosticForCorrectExplicitCollectionInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new List<int>
                                                     {
                                                         1,
                                                         2
                                                     };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly formatted target-typed collection initializer does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-001")]
    [Requirement("RH5303-002")]
    public async Task VerifyNoDiagnosticForCorrectTargetTypedCollectionInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        List<int> values = new()
                                                           {
                                                               1,
                                                               2
                                                           };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly aligned multi-line complex element does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    public async Task VerifyNoDiagnosticForCorrectComplexElementInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data
                                {
                                    public int A { get; set; }
                                }

                                internal class Example
                                {
                                    private static void Method(Data existingValue)
                                    {
                                        var values = new Dictionary<Data, Data>()
                                                     {
                                                         {
                                                             new Data
                                                             {
                                                                 A = 1
                                                             },
                                                             existingValue
                                                         }
                                                     };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that same-line comments can prefix every token validated inside a complex element when the expressions and the
    /// closing brace are measured from the opening brace behind its leading comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyNoDiagnosticForCommentPrefixedComplexElementTokens()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var values = new Dictionary<Data, Data>()
                                                     {
                                                         /* Keep element. */ {
                                                                                 /* Keep key. */ existingKey,

                                                                                 /* Keep value. */ existingValue
                                                                             /* Keep close. */ }
                                                     };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment-prefixed complex element whose expressions and closing brace are measured from the comment instead
    /// of the brace behind it reports on the whole complex element and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyCommentPrefixedComplexElementMeasuredFromTheCommentIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var values = new Dictionary<Data, Data>()
                                                     {
                                                         /* Keep element. */ {|#0:{
                                                             /* Keep key. */ existingKey,

                                                             /* Keep value. */ existingValue
                                                         /* Keep close. */ }|}
                                                     };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var values = new Dictionary<Data, Data>()
                                                      {
                                                          /* Keep element. */ {
                                                                                  /* Keep key. */ existingKey,

                                                                                  /* Keep value. */ existingValue
                                                                              /* Keep close. */ }
                                                      };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the spec example: a complex element whose expressions and closing brace follow its opening brace behind the
    /// comment that leads the brace's line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    public async Task VerifyNoDiagnosticForComplexElementAlignedToItsBraceBehindLeadingComment()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* e */ {
                                                                /* k */ 1,
                                                                2
                                                            /* c */ }
                                                };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the spec example: a complex element whose expressions and closing brace start one level deeper than the comment
    /// that leads the brace's line, and under it, reports on the whole complex element and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyComplexElementAlignedToTheLeadingCommentIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* e */ {|#0:{
                                                        /* k */ 1,
                                                        2
                                                    /* c */ }|}
                                                };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var d = new Dictionary<int, int>
                                                 {
                                                     /* e */ {
                                                                 /* k */ 1,
                                                                 2
                                                             /* c */ }
                                                 };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the spec example: when only an expression of a complex element behind a leading comment is out of place, the
    /// diagnostic is on that expression, and the fix moves it under the brace's level
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyExpressionOutOfPlaceInComplexElementBehindLeadingCommentIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* e */ {
                                                        {|#0:1|},
                                                                2
                                                            }
                                                };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var d = new Dictionary<int, int>
                                                 {
                                                     /* e */ {
                                                                 1,
                                                                 2
                                                             }
                                                 };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the shared spec example: a complex element whose brace a comment leads is measured from the brace, and a
    /// complex element whose comment stands on the line above is measured from the line's column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    public async Task VerifyNoDiagnosticForComplexElementsWithCommentInFrontOfTheBraceAndOnTheLineAbove()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* e */ {
                                                                1,
                                                                2
                                                            },

                                                    // f
                                                    {
                                                        3,
                                                        4
                                                    }
                                                };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the shared spec example: a complex element measured from the comment in front of its brace reports on the
    /// whole complex element, because its closing brace does not start at the brace's column, and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyComplexElementMeasuredFromTheCommentInFrontOfItsBraceIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* e */ {|#0:{
                                                        1,
                                                        2
                                                    }|}
                                                };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var d = new Dictionary<int, int>
                                                 {
                                                     /* e */ {
                                                                 1,
                                                                 2
                                                             }
                                                 };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a complex element whose brace stands behind the last line of a multi-line comment is measured from the
    /// brace and is not reported when its expressions and closing brace follow it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    public async Task VerifyNoDiagnosticForComplexElementAlignedToItsBraceBehindTheLastLineOfAMultiLineComment()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* a
                                                       b */ {
                                                                1,
                                                                2
                                                            }
                                                };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a complex element whose opening brace stands behind the last line of a multi-line comment is reported, and
    /// that the fix lays out the code as the formatter lays out the whole document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    [Requirement("RH5303-004")]
    public async Task VerifyComplexElementBehindTheLastLineOfAMultiLineCommentIsDetectedAndFixedAsTheFormatterLaysItOut()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* a
                                                       b */ {|RH5303:{
                                                        1,
                                                        2
                                                    }|}
                                                };
                                    }
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
    }

    /// <summary>
    /// Verifies the other side of the spec example: with the multi-line comment ending on a line above the complex element's
    /// opening brace, the fix is offered and moves the misplaced expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-003")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyComplexElementBelowAMultiLineCommentThatEndsAboveItsBraceIsFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* a
                                                       b */
                                                    {
                                                        1,
                                                      {|#0:2|}
                                                    }
                                                };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var d = new Dictionary<int, int>
                                                 {
                                                     /* a
                                                        b */
                                                     {
                                                         1,
                                                         2
                                                     }
                                                 };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that misplaced initializer braces around a complex element behind the last line of a multi-line comment are
    /// reported on the object creation, and that the fix lays out the code as the formatter lays out the whole document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-001")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyMisplacedBracesAroundComplexElementBehindTheLastLineOfAMultiLineCommentAreDetectedAndFixedAsTheFormatterLaysThemOut()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = {|RH5303:new Dictionary<int, int>
                                              {
                                                    /* a
                                                       b */ {
                                                                1,
                                                                2
                                                            }
                                                }|};
                                    }
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
    }

    /// <summary>
    /// Verifies the one-line side of the guard's scope: misplaced initializer braces around a complex element behind a one-line
    /// comment that leads its line are reported on the object creation and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-001")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyMisplacedBracesAroundComplexElementBehindLeadingCommentAreDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = {|#0:new Dictionary<int, int>
                                              {
                                                    /* e */ {
                                                                1,
                                                                2
                                                            }
                                                }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var d = new Dictionary<int, int>
                                                 {
                                                     /* e */ {
                                                                 1,
                                                                 2
                                                             }
                                                 };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix lays out an assignment statement that starts its line and contains a complex element behind the last
    /// line of a multi-line comment as the formatter lays out the whole document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    public async Task VerifyComplexElementBehindTheLastLineOfAMultiLineCommentInAssignmentStatementIsDetectedAndFixedAsTheFormatterLaysItOut()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(Dictionary<int, int> d)
                                    {
                                        d = new Dictionary<int, int>
                                            {
                                                /* a
                                                   b */ {|RH5303:{
                                                    1,
                                                    2
                                                }|}
                                            };
                                    }
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
    }

    /// <summary>
    /// Verifies that the fix lays out an expression body that contains a complex element behind the last line of a multi-line
    /// comment as the formatter lays out the whole document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    public async Task VerifyComplexElementBehindTheLastLineOfAMultiLineCommentInExpressionBodyIsDetectedAndFixedAsTheFormatterLaysItOut()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    Dictionary<int, int> P => new Dictionary<int, int>
                                                              {
                                                                  /* a
                                                                     b */ {|RH5303:{
                                                                      1,
                                                                      2
                                                                  }|}
                                                              };
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
    }

    /// <summary>
    /// Verifies Fix All across two object creations that stand behind the last line of a multi-line comment: both are laid out
    /// from <c>new</c> where it stands
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyFixAllLaysOutTwoCreationsBehindTheLastLineOfAMultiLineCommentFromNew()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = /* a
                                                   b */ {|#0:new List<int>
                                          {
                                            1
                                          }|};
                                        var e = /* c
                                                   d */ {|#1:new List<int>
                                          {
                                            2
                                          }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var d = /* a
                                                    b */ new List<int>
                                                         {
                                                             1
                                                         };
                                         var e = /* c
                                                    d */ new List<int>
                                                         {
                                                             2
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the spec example: an object creation behind the last line of a multi-line comment is laid out from <c>new</c>
    /// where it stands
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyCreationBehindTheLastLineOfAMultiLineCommentIsFixedFromNewWhereItStands()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = /* a
                                                   b */ {|#0:new List<int>
                                          {
                                            1
                                          }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var d = /* a
                                                    b */ new List<int>
                                                         {
                                                             1
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that two tabs in front of the declaration's value count as one column each, so the fix lays out the braces from
    /// <c>new</c> in its character column, and its result is not reported again
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyCreationBehindTabsIsFixedFromNewInItsCharacterColumn()
    {
        const string testData = "using System.Collections.Generic;\n\nclass C\n{\n\tvoid M()\n\t{\n\t\tvar d = {|#0:new List<int>\n\t\t  {\n\t\t\t1\n\t\t  }|};\n\t}\n}";
        const string fixedData = "using System.Collections.Generic;\n\nclass C\n{\n\tvoid M()\n\t{\n\t\tvar d = new List<int>\n          {\n              1\n          };\n\t}\n}";

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix lays out an object creation element behind the last line of a multi-line comment as the formatter
    /// lays out the whole document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyObjectCreationElementBehindTheLastLineOfAMultiLineCommentIsFixedAsTheFormatterLaysItOut()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class P
                                {
                                    public int X { get; set; }
                                }

                                class C
                                {
                                    void M()
                                    {
                                        var d = {|RH5303:new List<P>
                                              {
                                                    /* a
                                                       b */ new P
                                                            {
                                                                X = 1
                                                            }
                                                }|};
                                    }
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
    }

    /// <summary>
    /// Verifies that the fix lays out a collection creation element behind the last line of a multi-line comment as the
    /// formatter lays out the whole document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyCollectionCreationElementBehindTheLastLineOfAMultiLineCommentIsFixedAsTheFormatterLaysItOut()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class P
                                {
                                    public int X { get; set; }
                                }

                                class C
                                {
                                    void M()
                                    {
                                        var d = {|RH5303:new List<List<int>>
                                              {
                                                    /* a
                                                       b */ new List<int>
                                                            {
                                                                1
                                                            }
                                                }|};
                                    }
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
    }

    /// <summary>
    /// Verifies that the fix lays out a call element with a wrapped argument behind the last line of a multi-line comment as the
    /// formatter lays out the whole document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyCallElementBehindTheLastLineOfAMultiLineCommentIsFixedAsTheFormatterLaysItOut()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class P
                                {
                                    public int X { get; set; }
                                }

                                class C
                                {
                                    static int Call(int a, int b)
                                    {
                                        return a + b;
                                    }

                                    void M()
                                    {
                                        var d = {|RH5303:new List<int>
                                              {
                                                    /* a
                                                       b */ Call(1,
                                                                 2)
                                                }|};
                                    }
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
    }

    /// <summary>
    /// Verifies that the fix lays out a logical element with a wrapped operand behind the last line of a multi-line comment as
    /// the formatter lays out the whole document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyLogicalElementBehindTheLastLineOfAMultiLineCommentIsFixedAsTheFormatterLaysItOut()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class P
                                {
                                    public int X { get; set; }
                                }

                                class C
                                {
                                    void M(bool x, bool y)
                                    {
                                        var d = {|RH5303:new List<bool>
                                              {
                                                    /* a
                                                       b */ x
                                                            && y
                                                }|};
                                    }
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
    }

    /// <summary>
    /// Verifies that the fix lays out an expression body whose object creation element stands behind the last line of a
    /// multi-line comment as the formatter lays out the whole document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyObjectCreationElementBehindTheLastLineOfAMultiLineCommentInExpressionBodyIsFixedAsTheFormatterLaysItOut()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class P
                                {
                                    public int X { get; set; }
                                }

                                class C
                                {
                                    List<P> Q => {|RH5303:new List<P>
                                                {
                                                  /* a
                                                     b */ new P
                                                          {
                                                              X = 1
                                                          }
                                                 }|};
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
    }

    /// <summary>
    /// Verifies that two complex elements of one initializer whose leading comments decide their columns both report and are
    /// fixed together
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyTwoComplexElementsMeasuredFromTheirLeadingCommentsAreDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    /* e */ {|#0:{
                                                        1,
                                                        2
                                                    }|},
                                                    /* f */ {|#1:{
                                                        3,
                                                        4
                                                    }|}
                                                };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var d = new Dictionary<int, int>
                                                 {
                                                     /* e */ {
                                                                 1,
                                                                 2
                                                             },
                                                     /* f */ {
                                                                 3,
                                                                 4
                                                             }
                                                 };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a shifted standalone comment before a complex-element opening brace reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5303-002")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyStandaloneCommentBeforeComplexOpenBraceIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var values = new Dictionary<Data, Data>()
                                                     {
                                                     /* Keep element. */
                                                         {|#0:{
                                                             existingKey,
                                                             existingValue
                                                         }|}
                                                     };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var values = new Dictionary<Data, Data>()
                                                      {
                                                          /* Keep element. */
                                                          {
                                                              existingKey,
                                                              existingValue
                                                          }
                                                      };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a shifted multi-line comment before a complex-element expression reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyMultilineCommentBeforeComplexExpressionIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var values = new Dictionary<Data, Data>()
                                                     {
                                                         {
                                                         /*
                                                             Keep key.
                                                             */ {|#0:existingKey|},
                                                             existingValue
                                                         }
                                                     };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var values = new Dictionary<Data, Data>()
                                                      {
                                                          {
                                                              /*
                                                              Keep key.
                                                              */ existingKey,
                                                              existingValue
                                                          }
                                                      };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a shifted documentation-comment continuation before a complex opening brace reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5303-002")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyDocumentationContinuationBeforeComplexOpenBraceIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var values = new Dictionary<Data, Data>()
                                                     {
                                                         /// Keep element.
                                /// Keep continuation.
                                                         {|#0:{
                                                             existingKey,
                                                             existingValue
                                                         }|}
                                                     };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var values = new Dictionary<Data, Data>()
                                                      {
                                                          /// Keep element.
                                                          /// Keep continuation.
                                                          {
                                                              existingKey,
                                                              existingValue
                                                          }
                                                      };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a shifted documentation-comment continuation before a complex expression reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyDocumentationContinuationBeforeComplexExpressionIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var values = new Dictionary<Data, Data>()
                                                     {
                                                         {
                                                             /// Keep key.
                                /// Keep continuation.
                                                             {|#0:existingKey|},
                                                             existingValue
                                                         }
                                                     };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var values = new Dictionary<Data, Data>()
                                                      {
                                                          {
                                                              /// Keep key.
                                                              /// Keep continuation.
                                                              existingKey,
                                                              existingValue
                                                          }
                                                      };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a multi-line complex element cannot share the collection opening-brace line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-002")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyComplexElementOnCollectionOpenBraceLineIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var values = new Dictionary<Data, Data>()
                                                     {   {|#0:{
                                                             existingKey,
                                                             existingValue
                                                         }|}
                                                     };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var values = new Dictionary<Data, Data>()
                                                      {
                                                          {
                                                              existingKey,
                                                              existingValue
                                                          }
                                                      };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line complex element remains valid without interior column checks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    public async Task VerifyNoDiagnosticForSingleLineComplexElementInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var values = new Dictionary<Data, Data>()
                                                     {
                                                         { existingKey, existingValue }
                                                     };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that object initializers remain owned by RH5301
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForObjectInitializer()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        Name = "Test"
                                                    };
                                    }

                                    private string Name { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that nested collection initializer assignments remain owned by RH5304
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForNestedCollectionInitializerOwnedByRH5304()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        Values = {
                                                                     1,
                                                                     2
                                                                 }
                                                    };
                                    }

                                    public List<int> Values { get; set; } = [];
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a formatter-stable same-line comment can prefix the collection opening brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5303-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyNoDiagnosticForCommentPrefixedCollectionOpeningBrace()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new List<int>()
                                                     /* Keep open. */ {
                                                         1
                                                     };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a formatter-stable same-line comment can prefix the collection closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5303-001")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyNoDiagnosticForCommentPrefixedCollectionClosingBrace()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new List<int>()
                                                     {
                                                         1
                                                     /* Keep close. */ };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a shifted same-line comment before the collection opening brace reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5303-001")]
    [Requirement("RH5303-004")]
    [Requirement("RH5303-005")]
    public async Task VerifyShiftedCommentBeforeCollectionOpeningBraceIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = {|#0:new List<int>()
                                        /* Keep open. */ {
                                            1
                                        }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var values = new List<int>()
                                                      /* Keep open. */ {
                                                          1
                                                      };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that scoped formatting reanchors tokens after directive trivia in a field initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveTriviaConvergesInFieldInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private Dictionary<int, int> Values = new()
                                                                          {
                                                                              {
                                                                                  1,
                                #if false
                                                                                  disabledValue
                                #endif
                                {|#0:2|}
                                                                              }
                                                                          };
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private Dictionary<int, int> Values = new()
                                                                           {
                                                                               {
                                                                                   1,
                                 #if false
                                                                                   disabledValue
                                 #endif
                                                                                   2
                                                                               }
                                                                           };
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that scoped formatting reanchors tokens after directive trivia in an arrow expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveTriviaConvergesInArrowExpression()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private Dictionary<int, int> Create()
                                        => new()
                                           {
                                               {
                                                   1,
                                #if false
                                                   disabledValue
                                #endif
                                {|#0:2|}
                                               }
                                           };
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private Dictionary<int, int> Create()
                                         => new()
                                            {
                                                {
                                                    1,
                                 #if false
                                                    disabledValue
                                 #endif
                                                    2
                                                }
                                            };
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a collection initializer on the line of new is reported on the whole object creation, while a
    /// target-typed creation with its braces in the column of new is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-001")]
    [Requirement("RH5303-004")]
    public async Task VerifyBracesOutsideColumnOfNewAreReportedOnCreation()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var a = {|RH5303:new List<int> { 1, 2 }|};
                                        List<int> b = new()
                                                      {
                                                          1
                                                      };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an element on the line of an earlier element and an element outside its column are each reported
    /// on the element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-002")]
    [Requirement("RH5303-004")]
    public async Task VerifyElementsOutsideTheirLineOrColumnAreReportedOnTheElement()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var a = new List<int>
                                                {
                                                    1, {|RH5303:2|},
                                                  {|RH5303:3|}
                                                };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an element on the line of the opening brace is reported on the element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-002")]
    [Requirement("RH5303-004")]
    public async Task VerifyElementOnLineOfOpeningBraceIsReported()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var a = new List<int>
                                                {   {|RH5303:1|},
                                                    2
                                                };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a complex element on one line and a wrapped complex element laid out as a block are not reported,
    /// while a misplaced expression is reported on that expression and a misplaced brace on the whole complex element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-002")]
    [Requirement("RH5303-003")]
    [Requirement("RH5303-004")]
    public async Task VerifyWrappedComplexElementsAreLaidOutAsBlocks()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var d = new Dictionary<int, int>
                                                {
                                                    { 1, 2 },
                                                    {
                                                        3,
                                                        4
                                                    },
                                                    {
                                                        5,
                                                      {|RH5303:6|}
                                                    },
                                                    {
                                                        7, {|RH5303:8|}
                                                    },
                                                    {|RH5303:{
                                                        9,
                                                        10
                                                  }|}
                                                };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix lays out the collection initializer with every element on its own line one level deeper
    /// and every expression of a wrapped complex element on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    public async Task VerifyFixLaysOutCollectionInitializer()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var a = new Dictionary<int, int>
                                                {
                                                    {
                                                        1, {|RH5303:2|}
                                                    },
                                                  {|RH5303:{ 3, 4 }|}
                                                };
                                    }
                                }
                                """;
        const string fixedCode = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var a = new Dictionary<int, int>
                                                 {
                                                     {
                                                         1,
                                                         2
                                                     },
                                                     { 3, 4 }
                                                 };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix lays out the collection initializer with every element on its own line one level deeper
    /// and every expression of a wrapped complex element on its own line, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    public async Task VerifyFixLaysOutCollectionInitializerWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var a = new Dictionary<int, int>
                                                {
                                                    {
                                                        1, {|RH5303:2|}
                                                    },
                                                  {|RH5303:{ 3, 4 }|}
                                                };
                                    }
                                }
                                """;
        const string fixedCode = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var a = new Dictionary<int, int>
                                                 {
                                                     {
                                                         1,
                                                         2
                                                     },
                                                     { 3, 4 }
                                                 };
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that a comment that starts the line of a collection element in the element's column does not get the
    /// element reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-002")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyNoDiagnosticForCommentPrefixedCollectionElement()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new List<int>()
                                                     {
                                                         /* Keep element. */ 1,
                                                         2
                                                     };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a later element on a line that starts with a comment in the element column is still reported,
    /// because only the first element behind the comment is exempt
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-002")]
    [Requirement("RH5303-005")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyLaterElementOnLineStartingWithCommentIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new List<int>()
                                                     {
                                                         /* a */ 1, {|#0:2|}
                                                     };
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;

                                  internal class Example
                                  {
                                      private static void Method()
                                      {
                                          var values = new List<int>()
                                                       {
                                                           /* a */ 1,
                                                           2
                                                       };
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a later element on a line that starts with a comment in the element column is still reported and
    /// fixed under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-002")]
    [Requirement("RH5303-005")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyLaterElementOnLineStartingWithCommentIsDetectedAndFixedWithCrLf()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new List<int>()
                                                     {
                                                         /* a */ 1, {|#0:2|}
                                                     };
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;

                                  internal class Example
                                  {
                                      private static void Method()
                                      {
                                          var values = new List<int>()
                                                       {
                                                           /* a */ 1,
                                                           2
                                                       };
                                      }
                                  }
                                  """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(resultData), Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a collection element behind a block comment which starts its line in column zero is reported,
    /// although the element stands in its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-002")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyElementInItsColumnBehindMisplacedBlockCommentIsDetected()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new List<int>()
                                                     {
                                /* c */                  {|#0:1|}
                                                     };
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a collection element behind a block comment which starts its line in column zero is reported,
    /// although the element stands in its column, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-002")]
    [Requirement("SHARED-COMMENT-011")]
    public async Task VerifyElementInItsColumnBehindMisplacedBlockCommentIsDetectedWithCrLf()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new List<int>()
                                                     {
                                /* c */                  {|#0:1|}
                                                     };
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix, which lays out its result through the formatter, is not offered in a file with a syntax error,
    /// while the diagnostic is still reported, and that it is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              using System.Collections.Generic;

                              internal class Data;

                              internal class Example
                              {
                                  private static void Method(Data firstValue, Data secondValue)
                                  {
                                      var values = new Dictionary<string, Data>()
                                                   {
                                                       {
                                                           "first",
                                                       firstValue
                                                       },
                                                       {
                                                           "second",
                                                       secondValue
                                                       }
                                                   };
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies that the fix of a collection initializer whose comma starts a line moves the comma behind the element in front of
    /// it, as whole-document formatting does, and places the element behind it at the elements' column
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5303-005")]
    [Requirement("FMT-BREAK-038")]
    public async Task VerifyFixMovesACommaThatStartsALineBehindTheElement(string endOfLine)
    {
        const string source = """
                              using System.Collections.Generic;

                              internal class C
                              {
                                  internal void M()
                                  {
                                      var l = new List<int> { 1
                                          , 2 };
                                  }
                              }

                              internal class D
                              {
                                  public int X { get; set; }

                                  public int Y { get; set; }

                                  public List<int> Items { get; } = new List<int>();
                              }
                              """;
        const string expected = """
                                using System.Collections.Generic;

                                internal class C
                                {
                                    internal void M()
                                    {
                                        var l = new List<int>
                                                {
                                                    1,
                                                    2
                                                };
                                    }
                                }

                                internal class D
                                {
                                    public int X { get; set; }

                                    public int Y { get; set; }

                                    public List<int> Items { get; } = new List<int>();
                                }
                                """;

        var input = ConvertEndOfLine(source, endOfLine);
        var fixedSource = await ApplyCodeFixAsync(input);

        Assert.AreEqual(ConvertEndOfLine(expected, endOfLine), fixedSource);
        Assert.AreEqual(Format(input), fixedSource, "The code fix must lay out the code as the formatter lays it out.");
    }

    /// <summary>
    /// Verifies that the fix is withheld for a collection initializer whose comma a comment keeps off the line of the element in
    /// front of it, which the formatter refuses as unsupported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("FMT-UNSUP-009")]
    public async Task VerifyNoCodeFixWhenACommentHoldsACommaAtTheStartOfItsLine()
    {
        const string source = """
                              using System.Collections.Generic;

                              internal class C
                              {
                                  internal void M()
                                  {
                                      var l = new List<int> { 1 // k
                                          , 2 };
                                  }
                              }

                              internal class D
                              {
                                  public int X { get; set; }

                                  public int Y { get; set; }

                                  public List<int> Items { get; } = new List<int>();
                              }
                              """;

        Assert.IsNull(await TryApplyCodeFixAsync(source), "No code fix may be offered for an initializer the formatter refuses.");
    }

    /// <summary>
    /// Verifies that an element behind two comments, the first of which starts its line outside the element column, is reported on the element, also where the element reaches the element column, and that the code fix is withheld or lays the line out in one pass
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5303-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyElementBehindTwoMisplacedLeadingCommentsIsReported(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var values = new List<int>
                                                     {
                                                 /* a */ /* b */ {|RH5303:1|},
                                                         2
                                                     };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var values = new List<int>
                                                      {
                                                          /* a */ /* b */ 1,
                                                          2
                                                      };
                                     }
                                 }
                                 """;

        await VerifyCodeFixIsWithheldOrProducesOneOf(ConvertEndOfLine(testData, endOfLine), true, ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies that a element behind a line of its own that holds two comments, the first of which starts the line in the expected column, is not reported: the further comment is not checked
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5303-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyElementBelowLineOfTwoCommentsInExpectedColumnIsNotReported(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var values = new List<int>
                                                     {
                                                         /* a */ /* b */
                                                         1,
                                                         2
                                                     };
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a element behind a further comment that starts on an earlier line behind a comment in the expected column and ends on the element's line is not reported: neither the further comment nor the element is checked
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5303-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyElementBehindFurtherCommentEndingOnItsLineIsNotReported(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var values = new List<int>
                                                     {
                                                         1,
                                                         /* a */ /* b
                                             */ 2
                                                     };
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a element below a line of its own that holds two comments, the first of which starts the line outside the expected column, is reported, and that the code fix is withheld or lays the line out in one pass
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5303-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyElementBelowLineOfTwoCommentsOutsideExpectedColumnIsReported(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var values = new List<int>
                                                     {
                                             /* a */ /* b */
                                                         {|RH5303:1|},
                                                         2
                                                     };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var values = new List<int>
                                                      {
                                                          /* a */ /* b */
                                                          1,
                                                          2
                                                      };
                                     }
                                 }
                                 """;

        await VerifyCodeFixIsWithheldOrProducesOneOf(ConvertEndOfLine(testData, endOfLine), true, ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies that a element behind a further comment that starts on an earlier line behind a comment outside the expected column and ends on the element's line is reported, and that the code fix is withheld or lays the line out in one pass
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-001")]
    [Requirement("RH5303-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyElementBehindFurtherCommentWhoseLineStartsOutsideExpectedColumnIsReported(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var values = new List<int>
                                                     {
                                                         1,
                                             /* a */ /* b
                                             */ {|RH5303:2|}
                                                     };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var values = new List<int>
                                                      {
                                                          1,
                                                          /* a */ /* b
                                              */ 2
                                                      };
                                     }
                                 }
                                 """;

        await VerifyCodeFixIsWithheldOrProducesOneOf(ConvertEndOfLine(testData, endOfLine), true, ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies the spec example: the fix lays out the object creation from the column of new, which keeps its line and
    /// column, while the misspaced declaration in front of it stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5303-005")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixLeavesTheCodeInFrontOfTheObjectCreationAsWritten(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var  x={|RH5303:new List<int> { 1+2,
                                            3 }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var  x=new List<int>
                                                {
                                                    1 + 2,
                                                    3
                                                };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a comment inside the variable declaration around the object creation, which the formatter refuses,
    /// does not withhold the fix: the fix formats only the object creation, which holds no refused shape, and the
    /// declaration stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5303-005")]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyFixIsOfferedWhenTheDeclarationAroundTheObjectCreationHoldsACommentTheFormatterRefuses(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var /* c */ x = {|RH5303:new List<int> { 1,
                                            2 }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var /* c */ x = new List<int>
                                                         {
                                                             1,
                                                             2
                                                         };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a reported object creation inside a reported object creation is laid out with the outer one, and
    /// that Fix All corrects both in one application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5303-005")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllCorrectsAReportedObjectCreationInsideAReportedObjectCreationInOneApplication()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var x = {|RH5303:new List<List<int>> { {|RH5303:new List<int> { 1,
                                            2 }|},
                                            new List<int>() }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         var x = new List<List<int>>
                                                 {
                                                     new List<int>
                                                     {
                                                         1,
                                                         2
                                                     },
                                                     new List<int>()
                                                 };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, static config => config.NumberOfFixAllIterations = 1);
    }

    /// <summary>
    /// Verifies that a comment inside a variable declaration elsewhere in the statement that holds the object creation,
    /// which the formatter refuses, does not withhold the fix: the fix formats only the object creation, which holds no
    /// refused shape, and the rest of the statement stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5303-005")]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyFixIsOfferedWhenTheStatementAroundTheObjectCreationHoldsACommentTheFormatterRefuses(string endOfLine)
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(Action a, List<int> l)
                                    {
                                        M(() => { int /* c */ y = 1; }, {|RH5303:new List<int> { 1,
                                            2 }|});
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M(Action a, List<int> l)
                                     {
                                         M(() => { int /* c */ y = 1; }, new List<int>
                                                                         {
                                                                             1,
                                                                             2
                                                                         });
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a collection initializer whose element's argument list stands on the line below the invoked name
    /// joins the argument list onto the name
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-BREAK-039")]
    public async Task VerifyFixJoinsElementArgumentListBelowItsName(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var l = {|RH5303:new List<int> { Get
                                        (4), 2 }|};
                                    }

                                    private static int Get(int value)
                                    {
                                        return value;
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var l = new List<int>
                                                 {
                                                     Get(4),
                                                     2
                                                 };
                                     }

                                     private static int Get(int value)
                                     {
                                         return value;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a collection initializer in the value of a field declaration with a directive between its
    /// declarators is offered: the fix formats the initializer, which holds only part of the declaration, and the formatter does
    /// not refuse it; the directive and the declarators stay as they are
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-010")]
    public async Task FixIsOfferedForAnInitializerInsideACombinedFieldWithADirectiveBetweenItsDeclarators()
    {
        const string source = """
                              using System.Collections.Generic;

                              internal class C
                              {
                                  private List<int> _a = new List<int>
                                  {
                                  1,
                                      2
                                  },
                              #pragma warning disable CS0169
                                      _b;
                              #pragma warning restore CS0169
                              }
                              """;

        var fixedSource = await TryApplyCodeFixAsync(source);

        Assert.IsNotNull(fixedSource, "The fix of the initializer must be offered.");
        Assert.AreNotEqual(source, fixedSource, "The fix must change the initializer.");
        Assert.Contains("},\n#pragma warning disable CS0169\n        _b;\n#pragma warning restore CS0169\n}", fixedSource.Replace("\r\n", "\n"));
    }

    /// <summary>
    /// Verifies that the fix stays offered for a collection creation behind a comment behind a property initializer's <c>=</c>:
    /// the object creation the fix formats holds neither the <c>=</c> nor the comment, although the document is refused as a whole
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("FMT-UNSUP-003")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyFixIsOfferedForACollectionCreationBehindACommentedPropertyInitializerEquals(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    List<int> L { get; } = /* a */ {|RH5303:new List<int> { 1,
                                        2 }|};
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     List<int> L { get; } = /* a */ new List<int>
                                                                    {
                                                                        1,
                                                                        2
                                                                    };
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix stays offered for a collection creation behind a comment behind an assignment operator: the object
    /// creation the fix formats holds neither the operator nor the comment, although the document is refused as a whole
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("FMT-UNSUP-003")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyFixIsOfferedForACollectionCreationBehindACommentedAssignmentOperator(string endOfLine)
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        List<int> l;
                                        l = /* a */ {|RH5303:new List<int> { 1,
                                            2 }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         List<int> l;
                                         l = /* a */ new List<int>
                                                     {
                                                         1,
                                                         2
                                                     };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the change's example of a code fix that formats a collection initializer through the formatter: the comma in
    /// front of the <c>#endif</c> behind an active <c>#else</c> branch stays, since the formatter keeps a trailing comma in
    /// front of every conditional directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-020")]
    [Requirement("SHARED-TRAIL-004")]
    public async Task VerifyCodeFixKeepsTrailingCommaInFrontOfEndIfBehindActiveElseBranch()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var list = {|#0:new List<int> { 1,
                                #if X
                                            2,
                                #else
                                            3,
                                #endif
                                        }|};
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;

                                  class C
                                  {
                                      void M()
                                      {
                                          var list = new List<int>
                                                     {
                                                         1,
                                  #if X
                                              2,
                                  #else
                                                         3,
                                  #endif
                                                     };
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies a collection initializer inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is not
    /// reported when the code is compiled with C# 10, while a collection initializer that only contains an interpolated string is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-003")]
    public async Task CollectionInitializerInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    string M() => $"a {new List<int> { 1, 2 }.Count} b";

                                    string N() => $@"x {$"a {new List<int> { 1, 2 }.Count} b"} y";

                                    void O()
                                    {
                                        var a = {|RH5303:new List<int> { $"{1}".Length, 2 }|};
                                    }
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp10)));
    }

    /// <summary>
    /// Verifies a collection initializer inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is
    /// reported when the code is compiled with C# 11
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-003")]
    public async Task CollectionInitializerInsideANonVerbatimInterpolationIsReportedFromCSharp11()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    string M() => $"a {{|RH5303:new List<int> { 1, 2 }|}.Count} b";

                                    string N() => $@"x {$"a {{|RH5303:new List<int> { 1, 2 }|}.Count} b"} y";
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp11)));
    }

    /// <summary>
    /// Verifies a collection initializer inside an interpolation of a verbatim interpolated string, written <c>$@</c> or <c>@$</c>, is
    /// reported when the code is compiled with C# 10
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-003")]
    public async Task CollectionInitializerInsideAVerbatimInterpolationIsReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    string M() => $@"a {{|RH5303:new List<int> { 1, 2 }|}.Count} b";

                                    string Q() => @$"a {{|RH5303:new List<int> { 1, 2 }|}.Count} b";
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp10)));
    }

    /// <summary>
    /// Verifies a collection initializer whose elements are written in braces, inside an interpolation of a non-verbatim
    /// interpolated string, is not reported when the code is compiled with C# 10
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-INIT-003")]
    public async Task DictionaryCollectionInitializerInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    string M() => $"a {new Dictionary<int, int> { { 1, 2 } }.Count} b";
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp10)));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    private static void Method(Data firstValue, Data secondValue)
                                    {
                                        var values = new Dictionary<string, Data>()
                                                     {
                                                         {
                                                             "first",
                                                         {|#0:firstValue|}
                                                         },
                                                         {
                                                             "second",
                                                         {|#1:secondValue|}
                                                         }
                                                     };
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Collections.Generic;

                                  internal class Data;

                                  internal class Example
                                  {
                                      private static void Method(Data firstValue, Data secondValue)
                                      {
                                          var values = new Dictionary<string, Data>()
                                                       {
                                                           {
                                                               "first",
                                                               firstValue
                                                           },
                                                           {
                                                               "second",
                                                               secondValue
                                                           }
                                                       };
                                      }
                                  }
                                  """;

        // Verifies that Fix All converges when multiple complex elements contain misaligned expressions
        return new FixAllScenario(testData,
                                  resultData,
                                  Diagnostics(RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}