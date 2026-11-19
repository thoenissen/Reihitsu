using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3004UseLambdaSyntaxAnalyzer"/> and <see cref="RH3004UseLambdaSyntaxCodeFixProvider"/>
/// </summary>
public partial class RH3004UseLambdaSyntaxAnalyzerTests
{
    #region Tests

    /// <summary>
    /// Verifies the specification example of a block that only returns a verbatim string over several lines without a
    /// <c>#</c> line: the fix writes an expression body, and the later line keeps its columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-008")]
    public async Task FixRewritesASingleReturnOfAMultiLineVerbatimStringWithoutAHashLineAsAnExpressionBody()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<string> b = {|RH3004:delegate|} ()
                                        {
                                            return @"a
                                b";
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<string> b = () => @"a
                                 b";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a block that only returns a verbatim string over several lines without a <c>#</c> line, in a
    /// document whose line breaks are CRLF: the fix writes an expression body, and the later line keeps its columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-008")]
    public async Task FixRewritesASingleReturnOfAMultiLineVerbatimStringWithoutAHashLineAsAnExpressionBodyWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Func<string> b = {|RH3004:delegate|} ()
                                                                 {
                                                                     return @"a
                                                         b";
                                                                 };
                                                             }
                                                         }
                                                         """);

        var fixedCode = NormalizeToCarriageReturnLineFeed("""
                                                          using System;

                                                          class C
                                                          {
                                                              void M()
                                                              {
                                                                  Func<string> b = () => @"a
                                                          b";
                                                              }
                                                          }
                                                          """);

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a block that only returns a verbatim string over several lines whose later line holds <c>#</c> only
    /// behind other text: that line is no <c>#</c> line, so the fix writes an expression body, and the later line
    /// keeps its columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-003")]
    [Requirement("RH3004-008")]
    public async Task FixRewritesASingleReturnOfAStringWhoseLineHoldsHashBehindOtherTextAsAnExpressionBody()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<string> b = {|RH3004:delegate|} ()
                                        {
                                            return @"a
                                b #c";
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<string> b = () => @"a
                                 b #c";
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a block holding the <c>#else</c> and <c>#endif</c> of a chain whose
    /// <c>#if</c> stands before <c>delegate</c>, with <c>A</c> defined: the diagnostic is reported and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainOpenedBeforeTheAnonymousMethodThatEndsInsideItsBlock()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a =
                                #if A
                                            {|RH3004:delegate|} ()
                                            {
                                                Console.WriteLine(1);
                                #else
                                            () =>
                                            {
                                                Console.WriteLine(2);
                                #endif
                                                Console.WriteLine(3);
                                            };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode, DocumentationMode.Parse, "A");
    }

    /// <summary>
    /// Verifies the specification example of a block holding only the <c>#endif</c> of a chain whose <c>#if</c> and
    /// <c>#else</c> stand before <c>delegate</c>, with no symbol defined: the diagnostic is reported and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForABlockThatHoldsOnlyTheEndifOfAChainOpenedBeforeTheAnonymousMethod()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a =
                                #if A
                                            () =>
                                            {
                                                Console.WriteLine(1);
                                #else
                                            {|RH3004:delegate|} ()
                                            {
                                                Console.WriteLine(2);
                                #endif
                                                Console.WriteLine(3);
                                            };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a block holding a verbatim string with a line that starts with
    /// <c>#</c> and the end of a chain opened before <c>delegate</c>, with <c>A</c> defined: the diagnostic is reported
    /// and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainOpenedBeforeTheAnonymousMethodWhoseElseLiesInAStringOfTheBlock()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a =
                                #if A
                                            {|RH3004:delegate|} ()
                                            {
                                                var s = @"a
                                #else
                                            () =>
                                            {
                                                var s = @"
                                            ; // ";
                                #endif
                                                Console.WriteLine(s);
                                            };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding the <c>#if</c> of a chain whose <c>#endif</c> stands after the block's closing brace,
    /// without any symbol defined: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainOpenedInsideTheBlockThatEndsAfterItsClosingBrace()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = {|RH3004:delegate|} ()
                                        {
                                            Console.WriteLine(1);
                                #if A
                                        };
                                #else
                                        };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding the <c>#if</c> of a chain whose <c>#else</c> and <c>#endif</c> stand after the block's
    /// closing brace, with <c>A</c> defined: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainOpenedInsideTheBlockThatEndsAfterItsClosingBraceWithTheFirstBranchActive()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = {|RH3004:delegate|} ()
                                        {
                                            Console.WriteLine(1);
                                #if A
                                        };
                                #else
                                        };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode, DocumentationMode.Parse, "A");
    }

    /// <summary>
    /// Verifies the specification example of a conditional chain wholly outside the anonymous method, with <c>A</c>
    /// defined: the block holds no directive and no string, the fix is offered, and the lambda is laid out as the
    /// formatter lays it out
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixRewritesAnAnonymousMethodInsideAChainThatLiesWhollyOutsideIt()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #if A
                                        Action a = {|RH3004:delegate|} () { Console.WriteLine(1); };
                                #else
                                        Action a = () => Console.WriteLine(2);
                                #endif
                                    }
                                }
                                """;

        const string fixedCode = """
                                 #define A
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                 #if A
                                         Action a = () =>
                                                    {
                                                        Console.WriteLine(1);
                                                    };
                                 #else
                                         Action a = () => Console.WriteLine(2);
                                 #endif
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a first statement on the line of the opening brace and later lines that
    /// start left of it: the later lines move together until the leftmost of them stands in the statement column, and
    /// keep their indentation relative to each other
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesTheLinesAfterASharedOpeningBraceLineSoTheLeftmostStandsInTheStatementColumn()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action<int> a)
                                    {
                                    }

                                    void Foo(int x)
                                    {
                                    }

                                    void M()
                                    {
                                        Run({|RH3004:delegate|} (int x) { Foo(x);
                                            if (x > 0)
                                                Foo(x + 1);
                                        });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action<int> a)
                                     {
                                     }

                                     void Foo(int x)
                                     {
                                     }

                                     void M()
                                     {
                                         Run((int x) =>
                                             {
                                                 Foo(x);
                                                 if (x > 0)
                                                     Foo(x + 1);
                                             });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a first statement on the line of the opening brace and later lines that
    /// start left of it in a document whose line breaks are CRLF, and keeps those line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesTheLinesAfterASharedOpeningBraceLineSoTheLeftmostStandsInTheStatementColumnWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void Run(Action<int> a)
                                                             {
                                                             }

                                                             void Foo(int x)
                                                             {
                                                             }

                                                             void M()
                                                             {
                                                                 Run(delegate (int x) { Foo(x);
                                                                     if (x > 0)
                                                                         Foo(x + 1);
                                                                 });
                                                             }
                                                         }
                                                         """);

        var expectedCode = NormalizeToCarriageReturnLineFeed("""
                                                             using System;

                                                             class C
                                                             {
                                                                 void Run(Action<int> a)
                                                                 {
                                                                 }

                                                                 void Foo(int x)
                                                                 {
                                                                 }

                                                                 void M()
                                                                 {
                                                                     Run((int x) =>
                                                                         {
                                                                             Foo(x);
                                                                             if (x > 0)
                                                                                 Foo(x + 1);
                                                                         });
                                                                 }
                                                             }
                                                             """);

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Verifies the specification example of a first statement on the line of the opening brace and a later line that
    /// does not start left of it: the later line keeps its indentation relative to the first statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixKeepsTheLinesAfterASharedOpeningBraceLineRelativeToTheFirstStatement()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Foo(int x, int y)
                                    {
                                    }

                                    void M()
                                    {
                                        Action<int> c = {|RH3004:delegate|} (int x) { Foo(x,
                                                                               x + 1);
                                                                         };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Foo(int x, int y)
                                     {
                                     }

                                     void M()
                                     {
                                         Action<int> c = (int x) =>
                                                         {
                                                             Foo(x,
                                                                 x + 1);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a first statement on the line of the opening brace and a later statement that starts left of it, with
    /// the closing brace behind it: the later statement stands in the statement column, not left of the braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesALaterStatementInFrontOfAClosingBraceOnItsLineIntoTheStatementColumn()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Foo(int x)
                                    {
                                    }

                                    void M()
                                    {
                                        Action<int> c = {|RH3004:delegate|} (int x) { Foo(x);
                                            Foo(x + 1); };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Foo(int x)
                                     {
                                     }

                                     void M()
                                     {
                                         Action<int> c = (int x) =>
                                                         {
                                                             Foo(x);
                                                             Foo(x + 1);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a comment line in column 0 after an opening brace that shares its line with the first statement stays
    /// in column 0 and does not count for the leftmost line: the later statement stands in the statement column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixKeepsAColumnZeroCommentLineAfterASharedOpeningBraceLineInColumnZero()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action<int> a)
                                    {
                                    }

                                    void Foo(int x)
                                    {
                                    }

                                    void M()
                                    {
                                        Run({|RH3004:delegate|} (int x) { Foo(x);
                                // Foo(x + 2);
                                            Foo(x + 1);
                                        });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action<int> a)
                                     {
                                     }

                                     void Foo(int x)
                                     {
                                     }

                                     void M()
                                     {
                                         Run((int x) =>
                                             {
                                                 Foo(x);
                                 // Foo(x + 2);
                                                 Foo(x + 1);
                                             });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a comment line in column 0 in a block whose shift moves its lines to the
    /// left: the comment line stays in column 0, and the statement moves into the statement column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixKeepsAColumnZeroCommentLineInColumnZero()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void Foo()
                                    {
                                    }

                                    void M()
                                    {
                                        Run(
                                            {|RH3004:delegate|} ()
                                                {
                                                    Foo();
                                // note
                                                });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action a)
                                     {
                                     }

                                     void Foo()
                                     {
                                     }

                                     void M()
                                     {
                                         Run(
                                             () =>
                                             {
                                                 Foo();
                                 // note
                                             });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a comment line in column 8 in a block whose shift moves its lines to the
    /// left: no line would stand left of column 0, so every line moves by the measured shift, also the comment left of
    /// the braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesACommentLineInAnotherColumnByTheMeasuredShift()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void Foo()
                                    {
                                    }

                                    void M()
                                    {
                                        Run(
                                            {|RH3004:delegate|} ()
                                                {
                                                    Foo();
                                        // note
                                                });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action a)
                                     {
                                     }

                                     void Foo()
                                     {
                                     }

                                     void M()
                                     {
                                         Run(
                                             () =>
                                             {
                                                 Foo();
                                     // note
                                             });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a comment line in column 2 in a block whose shift moves its lines to the
    /// left: the shift would put the comment left of column 0, so the lines move together until the comment stands in
    /// the statement column, and the statement keeps its offset to it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesTheLinesOfABlockSoTheLeftmostStandsInTheStatementColumnWhenTheShiftWouldCrossColumnZero()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void Foo()
                                    {
                                    }

                                    void M()
                                    {
                                        Run(
                                            {|RH3004:delegate|} ()
                                                {
                                                    Foo();
                                  // note
                                                });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action a)
                                     {
                                     }

                                     void Foo()
                                     {
                                     }

                                     void M()
                                     {
                                         Run(
                                             () =>
                                             {
                                                                   Foo();
                                                 // note
                                             });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a conditional chain in column 0 inside a block whose lines the fix would move left, with <c>DEBUG</c>
    /// defined: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAColumnZeroConditionalChainInABlockWhoseLinesWouldMoveLeft()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void Foo()
                                    {
                                    }

                                    void M()
                                    {
                                        Run(
                                            {|RH3004:delegate|} ()
                                                {
                                #if DEBUG
                                                    Foo();
                                #endif
                                                });
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies <c>#region</c> and <c>#endregion</c> lines inside a block whose shift would cross column 0: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForRegionDirectivesInABlockWhoseShiftWouldCrossColumnZero()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void Foo()
                                    {
                                    }

                                    void M()
                                    {
                                        Run(
                                            {|RH3004:delegate|} ()
                                                {
                                  #region Calls
                                                    Foo();
                                  #endregion
                                                });
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block whose first statement stands in a conditional branch, active with <c>DEBUG</c> defined, in
    /// front of a statement in another column: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAConditionalStatementInFrontOfAStatementInAnotherColumnWithDebug()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                #if DEBUG
                                            Console.WriteLine(x);
                                #endif
                                                Console.WriteLine(-x);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies a block whose first statement is disabled text in front of a statement in another column: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForADisabledStatementInFrontOfAStatementInAnotherColumn()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                #if DEBUG
                                            Console.WriteLine(x);
                                #endif
                                                Console.WriteLine(-x);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a first statement line that starts with a comment in column 0: the line
    /// holds code, so it moves, the shift is measured from its first character, the comment, and the later statement
    /// keeps its offset to it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesAFirstStatementLineThatStartsWithAColumnZeroCommentAndMeasuresTheShiftFromTheComment()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                /* note */ Console.WriteLine(x);
                                            Console.WriteLine(x + 1);
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                                             /* note */ Console.WriteLine(x);
                                                                         Console.WriteLine(x + 1);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a later statement line that starts with a comment in column 0 holds code, so it is no comment line
    /// and moves by the shift of the first statement line, like the other statement lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    [Requirement("SHARED-FIX-007")]
    public async Task FixMovesALaterStatementLineThatStartsWithAColumnZeroCommentByTheShift()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            Console.WriteLine(x);
                                /* note */ Console.WriteLine(x + 1);
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                                             Console.WriteLine(x);
                                                 /* note */ Console.WriteLine(x + 1);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a first statement line that starts with a comment in column 0 in a
    /// document whose line breaks are CRLF, and keeps those line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMovesAFirstStatementLineThatStartsWithAColumnZeroCommentWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<int> b = delegate (int x)
                                                                 {
                                                         /* note */ Console.WriteLine(x);
                                                                     Console.WriteLine(x + 1);
                                                                 };
                                                             }
                                                         }
                                                         """);

        var expectedCode = NormalizeToCarriageReturnLineFeed("""
                                                             using System;

                                                             class C
                                                             {
                                                                 void M()
                                                                 {
                                                                     Action<int> b = (int x) =>
                                                                                     {
                                                                                         /* note */ Console.WriteLine(x);
                                                                                                     Console.WriteLine(x + 1);
                                                                                     };
                                                                 }
                                                             }
                                                             """);

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Verifies a block holding a delimited comment over several lines that starts in column 0: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForADelimitedCommentThatStartsInColumnZeroAndSpansLines()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            Console.WriteLine(x);
                                /* Console.WriteLine(x + 1);
                                   Console.WriteLine(x + 2); */
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding a delimited comment over several lines that starts in another column: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForADelimitedCommentThatStartsInAnotherColumnAndSpansLines()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            Console.WriteLine(x);
                                            /* Console.WriteLine(x + 3);
                                               Console.WriteLine(x + 4); */
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the former specification example of two delimited comments over several lines, one starting in column 0
    /// and one in another column: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForTwoDelimitedCommentsThatSpanLines()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            Console.WriteLine(x);
                                /* Console.WriteLine(x + 1);
                                   Console.WriteLine(x + 2); */
                                            /* Console.WriteLine(x + 3);
                                               Console.WriteLine(x + 4); */
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a delimited comment over several lines that, with <c>A</c> defined, holds
    /// the <c>#else</c> of a chain standing wholly outside the anonymous method: the diagnostic is reported and no code
    /// fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAMultiLineCommentThatHidesTheElseOfAChainAroundTheAnonymousMethod()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #if A
                                        Action a = {|RH3004:delegate|} ()
                                        {
                                            /* x
                                #else
                                        Action a = () => { var s = @"
                                            */ ";//".ToString();
                                            Console.WriteLine(1); };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a delimited documentation comment over several lines that, with <c>A</c> defined, holds the
    /// <c>#else</c> of a chain standing wholly outside the anonymous method: the diagnostic is reported and no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAMultiLineDocumentationCommentThatHidesTheElseOfAChainAroundTheAnonymousMethod()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #if A
                                        Action a = {|RH3004:delegate|} ()
                                        {
                                            /** x
                                #else
                                        Action a = () => { var s = @"
                                            */ ";//".ToString();
                                            Console.WriteLine(1); };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a delimited comment over several lines that, with <c>A</c> defined, holds the <c>#else</c> of a chain
    /// inside the block: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAMultiLineCommentThatHidesTheElseOfAChainInsideTheBlock()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = {|RH3004:delegate|} ()
                                        {
                                #if A
                                            /* x
                                #else
                                            var s = @"
                                            */ ";//".ToString();
                                #endif
                                            Console.WriteLine(1);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a verbatim string over several lines whose <c>#else</c> line starts with a
    /// form feed, which is a directive line without <c>A</c>: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAStringWhoseDirectiveLineStartsWithAFormFeed()
    {
        var template = """
                       #define A
                       using System;

                       class C
                       {
                           void M()
                           {
                       #if A
                               Action a = {|RH3004:delegate|} ()
                               {
                                   var s = @"x
                       <form-feed>#else
                               Action a = () => { var s = @"
                                   ; // ";
                                   Console.WriteLine(s); };
                       #endif
                           }
                       }
                       """;

        var testCode = template.Replace("<form-feed>", "\f", System.StringComparison.Ordinal);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of blocks holding a verbatim string, a delimited comment, an interpolated
    /// verbatim string, and a raw string literal over several lines, with no directive in the document: each diagnostic
    /// is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForStringsAndCommentsThatSpanLinesWithoutAnyDirective()
    {
        const string testCode = """"
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action b = {|RH3004:delegate|} ()
                                        {
                                            Console.WriteLine(
                                                @"a
                                b #c");
                                            /* first
                                               second */
                                        };
                                        Action<string> c = {|RH3004:delegate|} (string name)
                                        {
                                            var s = $@"Hello
                                {name}";
                                            Console.WriteLine(s);
                                        };
                                        Action<int> d = {|RH3004:delegate|} (int x)
                                        {
                                            Console.WriteLine("""
                                                a
                                                """);
                                        };
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a block that returns a verbatim string whose <c>#else</c> line ends the
    /// chain's first branch, with <c>A</c> defined: the fix would write an expression body and delete text that is
    /// the inside of a string without <c>A</c>, so the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAReturnedStringThatHidesTheElseOfAChainAroundTheAnonymousMethod()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #if A
                                        Func<string> f = {|RH3004:delegate|} () { return @"
                                #else
                                        Func<string> f = () =>
                                "; }; //";
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a delimited comment in the parameter list whose <c>#else</c> line ends
    /// the chain's first branch, on the block-body path, with <c>A</c> defined: the diagnostic is reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAParameterListCommentThatHidesTheElseOfAChainOnTheBlockBodyPath()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #if A
                                        Action<int> a = {|RH3004:delegate|} (int x /*
                                #else
                                        var s = @"
                                        */)
                                        {
                                            Console.WriteLine(x);
                                        }; //";
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a delimited comment in the parameter list whose <c>#else</c> line ends
    /// the chain's first branch, on the expression-body path, with <c>A</c> defined: the diagnostic is reported and
    /// no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAParameterListCommentThatHidesTheElseOfAChainOnTheExpressionBodyPath()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #if A
                                        Func<int, int> a = {|RH3004:delegate|} (int x /*
                                #else
                                        var s = @"
                                        */) { return x; }; //";
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a block that only returns a raw string literal over several lines
    /// whose later line starts with <c>#</c> behind spaces, with no directive in the document: the fix would write an
    /// expression body, but the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-007")]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForASingleReturnOfAMultiLineRawStringLiteralThatHoldsALineStartingWithHash()
    {
        const string testCode = """"
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<string> c = {|RH3004:delegate|} ()
                                        {
                                            return """
                                                a
                                                #b
                                                """;
                                        };
                                    }
                                }
                                """";

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a delimited comment over several lines without a <c>#</c> line in the
    /// parameter list, on the block-body path, with no directive in the document: the diagnostic is reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForADelimitedCommentInTheParameterListThatSpansLinesOnTheBlockBodyPath()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> e = {|RH3004:delegate|} (int x /* first
                                                                                     second */)
                                        {
                                            Console.WriteLine(x);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a delimited comment over several lines without a <c>#</c> line in the parameter list, on the
    /// block-body path, in a document whose line breaks are CRLF: the diagnostic is reported and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForADelimitedCommentInTheParameterListThatSpansLinesOnTheBlockBodyPathWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<int> e = {|RH3004:delegate|} (int x /* first
                                                                                                              second */)
                                                                 {
                                                                     Console.WriteLine(x);
                                                                 };
                                                             }
                                                         }
                                                         """);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a delimited comment over several lines in the parameter list whose later line starts with <c>#</c>,
    /// on the expression-body path, with no directive in the document: the diagnostic is reported and no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAParameterListCommentWithAHashLineOnTheExpressionBodyPath()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> a = {|RH3004:delegate|} (int x /* first
                                #second */)
                                        {
                                            return x;
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding a delimited comment over several lines in a document whose line breaks are CRLF: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForADelimitedCommentThatSpansLinesWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action b = {|RH3004:delegate|} ()
                                                                 {
                                                                     Console.WriteLine(1);
                                                                     /* first
                                                                        second */
                                                                 };
                                                             }
                                                         }
                                                         """);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a block whose strings and comments each stand on one line: the fix is
    /// offered and moves the lines as the layout requirement states
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    [Requirement("RH3004-008")]
    public async Task FixRewritesABlockWhoseStringsAndCommentsEachStandOnOneLine()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name)
                                        {
                                            // Greets the user.
                                            Console.WriteLine("Hello " + name); /* done */
                                            Console.WriteLine($"{name}!");
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<string> b = (string name) =>
                                                            {
                                                                // Greets the user.
                                                                Console.WriteLine("Hello " + name); /* done */
                                                                Console.WriteLine($"{name}!");
                                                            };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a block whose strings and comments each stand on one line in a document whose line breaks are CRLF: the
    /// fix is offered and keeps the line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixRewritesABlockWhoseStringsAndCommentsEachStandOnOneLineWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<string> b = delegate (string name)
                                                                 {
                                                                     // Greets the user.
                                                                     Console.WriteLine("Hello " + name); /* done */
                                                                     Console.WriteLine($"{name}!");
                                                                 };
                                                             }
                                                         }
                                                         """);

        var expectedCode = NormalizeToCarriageReturnLineFeed("""
                                                             using System;

                                                             class C
                                                             {
                                                                 void M()
                                                                 {
                                                                     Action<string> b = (string name) =>
                                                                                        {
                                                                                            // Greets the user.
                                                                                            Console.WriteLine("Hello " + name); /* done */
                                                                                            Console.WriteLine($"{name}!");
                                                                                        };
                                                                 }
                                                             }
                                                             """);

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Verifies the specification example of a chain that starts in the parameter list and ends in the block, with
    /// <c>A</c> defined: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainFromTheParameterListIntoTheBlock()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> a = {|RH3004:delegate|} (
                                #if A
                                            int x)
                                        {
                                            Console.WriteLine(x);
                                #else
                                            int x) { Console.WriteLine(-x);
                                #endif
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a chain that starts in the parameter list and ends in the block, with no symbol defined: the diagnostic
    /// is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainFromTheParameterListIntoTheBlockWithoutTheSymbol()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> a = {|RH3004:delegate|} (
                                #if A
                                            int x)
                                        {
                                            Console.WriteLine(x);
                                #else
                                            int x) { Console.WriteLine(-x);
                                #endif
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a chain that starts in the parameter list and ends after the anonymous method, with <c>A</c> defined:
    /// the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainFromTheParameterListPastTheAnonymousMethod()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> a = {|RH3004:delegate|} (
                                #if A
                                            int x)
                                        {
                                            Console.WriteLine(x);
                                        };
                                #else
                                            int x) { Console.WriteLine(-x); };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a chain that starts in the parameter list and ends after an anonymous method whose block only returns a
    /// value, with <c>A</c> defined: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainFromTheParameterListPastTheAnonymousMethodOnTheExpressionBodyPath()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> a = {|RH3004:delegate|} (
                                #if A
                                            int x)
                                        {
                                            return x;
                                        };
                                #else
                                            int x) { return -x; };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a chain wholly inside the parameter list of an anonymous method whose
    /// block only returns a value: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainInsideTheParameterListOnTheExpressionBodyPath()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> f = {|RH3004:delegate|} (
                                #if A
                                            int x
                                #else
                                            int x
                                #endif
                                            ) { return x + 1; };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a <c>#region</c> before <c>delegate</c> whose <c>#endregion</c> stands in the block: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForARegionBeforeTheAnonymousMethodThatEndsInsideItsBlock()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a =
                                #region R
                                            {|RH3004:delegate|} ()
                                            {
                                                Console.WriteLine(1);
                                #endregion
                                                Console.WriteLine(2);
                                            };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of a chain and a <c>#region</c> that start before <c>delegate</c> and end in
    /// the block, with <c>A</c> defined: each diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForAChainAndARegionThatCrossTheDelegateKeyword()
    {
        const string testCode = """
                                #define A
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a =
                                #if A
                                            {|RH3004:delegate|} ()
                                            {
                                                Console.WriteLine(1);
                                #else
                                            () =>
                                            {
                                                Console.WriteLine(2);
                                #endif
                                                Console.WriteLine(3);
                                            };
                                        Action b =
                                #region Init
                                            {|RH3004:delegate|} ()
                                            {
                                                Console.WriteLine(1);
                                #endregion
                                            };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a <c>#region</c> in the block whose <c>#endregion</c> stands after the anonymous method: the diagnostic
    /// is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForARegionInsideTheBlockThatEndsAfterTheAnonymousMethod()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = {|RH3004:delegate|} ()
                                                   {
                                                       Console.WriteLine(1);
                                #region R
                                                       Console.WriteLine(2);
                                                   };
                                #endregion
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of <c>#region</c> and <c>#pragma</c> directives inside the block: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixWithheldForRegionAndPragmaDirectivesInsideTheBlock()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                #region Output
                                #pragma warning disable CS0168
                                            Console.WriteLine(x);
                                #pragma warning restore CS0168
                                #endregion
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method inside a region whose <c>#region</c> and <c>#endregion</c> stand wholly outside it:
    /// the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-009")]
    public async Task FixRewritesAnAnonymousMethodInsideARegionThatLiesWhollyOutsideIt()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                #region Init
                                        Action a = {|RH3004:delegate|} () { Console.WriteLine(1); };
                                #endregion
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                 #region Init
                                         Action a = () =>
                                                    {
                                                        Console.WriteLine(1);
                                                    };
                                 #endregion
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a refused comment in the block of the anonymous method withholds the fix: the fix formats the
    /// lambda it builds, which keeps the block and so the comment, and the formatter refuses it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task NoFixIsOfferedWhenTheAnonymousMethodBodyHoldsACommentTheFormatterRefuses()
    {
        const string source = """
                              using System;

                              public class Test
                              {
                                  public Func<int, int> Run()
                                  {
                                      return {|RH3004:delegate|}(int x)
                                             {
                                                 int /* c */ y = x;

                                                 return y;
                                             };
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example of a blank line that holds whitespace among the statements the fix moves:
    /// the blank line stays and is written empty
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixWritesAWhitespaceOnlyLineAmongTheMovedStatementsEmpty()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               using System;

                                               class C
                                               {
                                                   void M()
                                                   {
                                                       Action<int> b = {|RH3004:delegate|} (int x)
                                                       {
                                                           Console.WriteLine(x);
                                               ············
                                                           Console.WriteLine(x + 1);
                                                       };
                                                   }
                                               }
                                               """);

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                                             Console.WriteLine(x);

                                                             Console.WriteLine(x + 1);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a blank line that holds whitespace among the statements the fix moves is written empty in a document
    /// whose line breaks are CRLF, and the line breaks stay CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixWritesAWhitespaceOnlyLineAmongTheMovedStatementsEmptyWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers("""
                                                                                 using System;

                                                                                 class C
                                                                                 {
                                                                                     void M()
                                                                                     {
                                                                                         Action<int> b = delegate (int x)
                                                                                         {
                                                                                             Console.WriteLine(x);
                                                                                 ············
                                                                                             Console.WriteLine(x + 1);
                                                                                         };
                                                                                     }
                                                                                 }
                                                                                 """));

        var expectedCode = NormalizeToCarriageReturnLineFeed("""
                                                             using System;

                                                             class C
                                                             {
                                                                 void M()
                                                                 {
                                                                     Action<int> b = (int x) =>
                                                                                     {
                                                                                         Console.WriteLine(x);

                                                                                         Console.WriteLine(x + 1);
                                                                                     };
                                                                 }
                                                             }
                                                             """);

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Verifies a blank line that holds a tab among the statements the fix moves does not withhold the fix and is
    /// written empty
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixWritesABlankLineHoldingATabAmongTheMovedStatementsEmpty()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               using System;

                                               class C
                                               {
                                                   void M()
                                                   {
                                                       Action<int> b = {|RH3004:delegate|} (int x)
                                                       {
                                                           Console.WriteLine(x);
                                               ····→····
                                                           Console.WriteLine(x + 1);
                                                       };
                                                   }
                                               }
                                               """);

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action<int> b = (int x) =>
                                                         {
                                                             Console.WriteLine(x);

                                                             Console.WriteLine(x + 1);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a tab in front of a moving statement line: the diagnostic is reported
    /// and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixWithheldForATabInFrontOfAMovingStatementLine()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               using System;

                                               class C
                                               {
                                                   void M()
                                                   {
                                                       Action<int> b = {|RH3004:delegate|} (int x)
                                                       {
                                               →········Console.WriteLine(x);
                                                       };
                                                   }
                                               }
                                               """);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a tab in front of a moving statement line withholds the fix in a document whose line breaks are CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixWithheldForATabInFrontOfAMovingStatementLineWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers("""
                                                                                 using System;

                                                                                 class C
                                                                                 {
                                                                                     void M()
                                                                                     {
                                                                                         Action<int> b = {|RH3004:delegate|} (int x)
                                                                                         {
                                                                                 →········Console.WriteLine(x);
                                                                                         };
                                                                                     }
                                                                                 }
                                                                                 """));

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a tab in front of a later moving statement line withholds the fix, also where the first statement
    /// line is indented with spaces only
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixWithheldForATabInFrontOfALaterMovingStatementLine()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               using System;

                                               class C
                                               {
                                                   void M()
                                                   {
                                                       Action<int> b = {|RH3004:delegate|} (int x)
                                                       {
                                                           Console.WriteLine(x);
                                               ········→···Console.WriteLine(x + 1);
                                                       };
                                                   }
                                               }
                                               """);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a tab in front of a moving comment line, a line that holds only a comment that does not start in
    /// column 0, withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixWithheldForATabInFrontOfAMovingCommentLine()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               using System;

                                               class C
                                               {
                                                   void M()
                                                   {
                                                       Action<int> b = {|RH3004:delegate|} (int x)
                                                       {
                                                           Console.WriteLine(x);
                                               →···········// note
                                                           Console.WriteLine(x + 1);
                                                       };
                                                   }
                                               }
                                               """);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a tab in front of a moving statement line withholds the fix also where the shift, counting the tab as
    /// one column, is zero
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixWithheldForATabInFrontOfAMovingStatementLineWhoseShiftIsZero()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               using System;

                                               class C
                                               {
                                                   void M()
                                                   {
                                                       Action<int> b = {|RH3004:delegate|} (int x)
                                                       {
                                               →···························Console.WriteLine(x);
                                                       };
                                                   }
                                               }
                                               """);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a tab between two tokens of a moving statement line does not withhold the fix and stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixIsOfferedForATabBetweenTwoTokensOfAMovingStatementLine()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               using System;

                                               class C
                                               {
                                                   void M()
                                                   {
                                                       Action<int> b = {|RH3004:delegate|} (int x)
                                                       {
                                                           int y =→x;
                                                           Console.WriteLine(y);
                                                       };
                                                   }
                                               }
                                               """);

        var fixedCode = ExpandWhitespaceMarkers("""
                                                using System;

                                                class C
                                                {
                                                    void M()
                                                    {
                                                        Action<int> b = (int x) =>
                                                                        {
                                                                            int y =→x;
                                                                            Console.WriteLine(y);
                                                                        };
                                                    }
                                                }
                                                """);

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a tab inside a comment and a tab inside a string of moving statement lines do not withhold the fix and
    /// stay as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixIsOfferedForATabInsideACommentAndInsideAStringOfMovingStatementLines()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               using System;

                                               class C
                                               {
                                                   void M()
                                                   {
                                                       Action<int> b = {|RH3004:delegate|} (int x)
                                                       {
                                                           Console.WriteLine(x); // a→b
                                                           Console.WriteLine("c→d");
                                                       };
                                                   }
                                               }
                                               """);

        var fixedCode = ExpandWhitespaceMarkers("""
                                                using System;

                                                class C
                                                {
                                                    void M()
                                                    {
                                                        Action<int> b = (int x) =>
                                                                        {
                                                                            Console.WriteLine(x); // a→b
                                                                            Console.WriteLine("c→d");
                                                                        };
                                                    }
                                                }
                                                """);

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a tab in front of the anonymous method's first line does not withhold the fix: the tab stays, and the
    /// lambda is laid out from the column of its first token with the tab counted as one column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixIsOfferedForATabInFrontOfTheFirstLineOfTheAnonymousMethod()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               using System;

                                               class C
                                               {
                                                   void M()
                                                   {
                                               →·······Action<int> b = {|RH3004:delegate|} (int x)
                                                       {
                                                           Console.WriteLine(x);
                                                       };
                                                   }
                                               }
                                               """);

        var fixedCode = ExpandWhitespaceMarkers("""
                                                using System;

                                                class C
                                                {
                                                    void M()
                                                    {
                                                →·······Action<int> b = (int x) =>
                                                                        {
                                                                            Console.WriteLine(x);
                                                                        };
                                                    }
                                                }
                                                """);

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a tab in front of the single <see langword="return"/> statement does not withhold the fix where it
    /// writes an expression body, which moves no line of the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixIsOfferedForATabInFrontOfASingleReturnStatementOnTheExpressionBodyPath()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               using System;

                                               class C
                                               {
                                                   void M()
                                                   {
                                                       Func<int, int> b = {|RH3004:delegate|} (int x)
                                                       {
                                               →···········return x + 1;
                                                       };
                                                   }
                                               }
                                               """);

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<int, int> b = (int x) => x + 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies Fix All leaves an anonymous method with a tab in front of a moving line as written and reported, and
    /// rewrites the other anonymous method of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixAllLeavesAnAnonymousMethodWithATabInFrontOfAMovingLineReported()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               using System;

                                               class C
                                               {
                                                   void M()
                                                   {
                                                       Action<int> b = {|RH3004:delegate|} (int x)
                                                       {
                                               →···········Console.WriteLine(x);
                                                       };
                                                       Action<int> c = {|RH3004:delegate|} (int x)
                                                       {
                                                           Console.WriteLine(x);
                                                       };
                                                   }
                                               }
                                               """);

        var fixedCode = ExpandWhitespaceMarkers("""
                                                using System;

                                                class C
                                                {
                                                    void M()
                                                    {
                                                        Action<int> b = {|RH3004:delegate|} (int x)
                                                        {
                                                →···········Console.WriteLine(x);
                                                        };
                                                        Action<int> c = (int x) =>
                                                                        {
                                                                            Console.WriteLine(x);
                                                                        };
                                                    }
                                                }
                                                """);

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies a block holding an interpolated verbatim string written with <c>@$"</c> whose continuation line starts
    /// with a hole: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForAnAtDollarInterpolatedStringWhoseContinuationLineStartsWithAHole()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<string> b = {|RH3004:delegate|} (string name)
                                        {
                                            var s = @$"Hello
                                {name}";
                                            Console.WriteLine(s);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block holding a regular interpolated string whose hole spans lines: the diagnostic is reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-008")]
    public async Task FixWithheldForARegularInterpolatedStringWhoseHoleSpansLines()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            var s = $"{x
                                                + 1}";
                                            Console.WriteLine(s);
                                        };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that the block-body fix is withheld when the statements it moves hold an LF line break in a document
    /// whose line breaks are otherwise CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task FixWithheldWhenTheBlockHoldsALineBreakOtherThanTheDocumentsLineEnding()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<int> b = {|RH3004:delegate|} (int x)
                                                                 {
                                                                     Console.WriteLine(x);{LF}
                                                                     Console.WriteLine(x);
                                                                 };
                                                             }
                                                         }
                                                         """).Replace("{LF}\r\n", "\n");

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that the block-body fix is withheld in a document whose line breaks are all a lone carriage return, where
    /// the block it moves holds line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task FixWithheldWhenTheBlockHoldsLoneCarriageReturnLineBreaks()
    {
        var testCode = """
                       using System;

                       class C
                       {
                           void M()
                           {
                               Action<int> b = {|RH3004:delegate|} (int x)
                               {
                                   Console.WriteLine(x);
                                   Console.WriteLine(x);
                               };
                           }
                       }
                       """.Replace("\r\n", "\n").Replace("\n", "\r");

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that an LF line break outside the code the fix changes, in a document whose line breaks are otherwise
    /// CRLF, does not withhold the block-body fix, which writes CRLF and leaves that line break as it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task LineBreakOtherThanTheDocumentsLineEndingOutsideTheChangedCodeKeepsTheFix()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;{LF}

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action<int> b = delegate (int x)
                                                                 {
                                                                     Console.WriteLine(x);
                                                                     Console.WriteLine(x);
                                                                 };
                                                             }
                                                         }
                                                         """).Replace("{LF}\r\n", "\n");

        var expectedCode = NormalizeToCarriageReturnLineFeed("""
                                                             using System;{LF}

                                                             class C
                                                             {
                                                                 void M()
                                                                 {
                                                                     Action<int> b = (int x) =>
                                                                                     {
                                                                                         Console.WriteLine(x);
                                                                                         Console.WriteLine(x);
                                                                                     };
                                                                 }
                                                             }
                                                             """).Replace("{LF}\r\n", "\n");

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
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
                              using System;

                              public class Test
                              {
                                  public Func<int, Func<int, int>> GetCurried()
                                  {
                                      return delegate(int x)
                                      {
                                          return delegate(int y) { return x + y; };
                                      };
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies that the fix of an anonymous method whose parameter list starts with a comment lays the lambda out as the
    /// formatter does, without a blank line between the opening parenthesis and the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixLeavesNoBlankLineAboveTheFirstCommentOfTheParameterList()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> f = {|#0:delegate|}(
                                            // the value
                                            int a)
                                        {
                                            return a;
                                        };
                                        _ = f;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<int, int> f = (
                                                             // the value
                                                             int a) => a;
                                         _ = f;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3004UseLambdaSyntaxAnalyzer.DiagnosticId, "Use lambda syntax."));
    }

    #endregion // Tests
}