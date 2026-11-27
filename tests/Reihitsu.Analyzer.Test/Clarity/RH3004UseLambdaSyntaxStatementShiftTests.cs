using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3004UseLambdaSyntaxCodeFixProvider"/> on the shift of the statement lines the fix moves, the comment lines among them, and the whitespace in front of them
/// </summary>
[TestClass]
public class RH3004UseLambdaSyntaxStatementShiftTests : BatchCodeFixTestsBase<RH3004UseLambdaSyntaxAnalyzer, RH3004UseLambdaSyntaxCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies a comment line in front of the block's first statement in another column does not decide the number
    /// of columns: the statement stands one level inside the braces, and the comment line moves by the same number
    /// of columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMeasuresTheShiftOnTheFirstStatementAndNotOnACommentLineInFrontOfIt()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                    // Writes the value
                                            Console.WriteLine(x);
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
                                                     // Writes the value
                                                             Console.WriteLine(x);
                                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the lines of a block without a statement move so that the block's first line stands one level inside
    /// the braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3004-006")]
    public async Task FixMeasuresTheShiftOfABlockWithoutAStatementOnItsFirstLine()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = {|RH3004:delegate|} (int x)
                                        {
                                            // Nothing to do
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
                                                             // Nothing to do
                                                         };
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

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Func<int, Func<int, int>> GetCurried()
                                    {
                                        return {|#0:delegate|}(int x)
                                        {
                                            return {|#1:delegate|}(int y) { return x + y; };
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Func<int, Func<int, int>> GetCurried()
                                     {
                                         return (int x) => (int y) => x + y;
                                     }
                                 }
                                 """;

        // Verifying anonymous methods nested in expressions are reported and fixed
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3004UseLambdaSyntaxAnalyzer.DiagnosticId, MessageFormat, 2),
                                  config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}