using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH5002IfStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5002IfStatementsShouldBePrecededByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer, RH5002IfStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when an if statement directly follows another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5002-001")]
    [Requirement("RH5002-002")]
    [Requirement("SHARED-STMT-004")]
    [Requirement("SHARED-STMT-001")]
    public async Task VerifyDiagnosticForIfStatementWithoutPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:if|} (value > 0)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5002
                                 {
                                     public void Execute()
                                     {
                                         var value = 1;

                                         if (value > 0)
                                         {
                                             value++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5002MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an if statement already has a preceding blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5002-001")]
    public async Task VerifyNoDiagnosticForIfStatementWithPrecedingBlankLine()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute()
                                    {
                                        var value = 1;

                                        if (value > 0)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an if statement is the first statement in a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5002-001")]
    public async Task VerifyNoDiagnosticForIfStatementAtStartOfBlock()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute()
                                    {
                                        if (true)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a diagnostic is reported when a comment line (rather than a whitespace-only blank line) directly precedes the statement, matching the formatter's whitespace-only blank-line definition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5002-001")]
    [Requirement("SHARED-STMT-004")]
    [Requirement("SHARED-STMT-001")]
    public async Task VerifyDiagnosticForIfStatementWhenCommentLineDirectlyPrecedesIt()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute(bool isReady)
                                    {
                                        var value = 1;
                                        // Comment before if
                                        {|#0:if|} (isReady)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5002
                                 {
                                     public void Execute(bool isReady)
                                     {
                                         var value = 1;

                                         // Comment before if
                                         if (isReady)
                                         {
                                             value++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5002MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when an if statement is the unbraced embedded body of a while statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-002")]
    public async Task VerifyNoDiagnosticForIfStatementAsEmbeddedWhileBody()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public int Execute(bool outer, bool inner)
                                    {
                                        while (outer)
                                            if (inner)
                                                return 1;

                                        return 0;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for an else-if statement, since its parent is an else clause rather than a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-002")]
    public async Task VerifyNoDiagnosticForElseIfStatement()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute(bool first, bool second)
                                    {
                                        if (first)
                                        {
                                            DoFirst();
                                        }
                                        else if (second)
                                        {
                                            DoSecond();
                                        }
                                    }

                                    private static void DoFirst()
                                    {
                                    }

                                    private static void DoSecond()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostic is reported when a preprocessor directive is the last trivia directly above the if statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-003")]
    public async Task VerifyNoDiagnosticForIfStatementDirectlyBelowDirective()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute(int value)
                                    {
                                        value++;
                                #if DEBUG
                                        value--;
                                #endif
                                        if (value > 6)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix moves an if statement that shares its line with the previous statement onto its own line after a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5002-001")]
    [Requirement("SHARED-STMT-004")]
    [Requirement("SHARED-STMT-001")]
    public async Task VerifyIfStatementOnSameLineAsPreviousStatementIsMovedToItsOwnLine()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute(int value)
                                    {
                                        value++; {|#0:if|} (value > 5) { }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5002
                                 {
                                     public void Execute(int value)
                                     {
                                         value++;

                                         if (value > 5) { }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5002MessageFormat));
    }

    /// <summary>
    /// Verifies one application of the fix corrects the line in front of the reported <c>if</c>, so the rule does not
    /// report it again
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-001")]
    [Requirement("SHARED-FIX-006")]
    public async Task OneApplicationCorrectsTheLineInFrontOfTheReportedIf()
    {
        const string testCode = "class C\n{\n    void M(int a)\n    {\n        a++;\n        {|RH5002:if|} (a > 1)\n        {\n        }\n    }\n}\n";
        const string fixedCode = "class C\n{\n    void M(int a)\n    {\n        a++;\n\n        if (a > 1)\n        {\n        }\n    }\n}\n";

        await Verify(testCode, fixedCode, test => test.NumberOfIncrementalIterations = 1);
    }

    /// <summary>
    /// Verifies one application of the fix corrects the line in front of the reported <c>if</c> under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-001")]
    public async Task OneApplicationCorrectsTheLineInFrontOfTheReportedIfUnderCarriageReturnLineFeed()
    {
        const string testCode = "class C\r\n{\r\n    void M(int a)\r\n    {\r\n        a++;\r\n        {|RH5002:if|} (a > 1)\r\n        {\r\n        }\r\n    }\r\n}\r\n";
        const string fixedCode = "class C\r\n{\r\n    void M(int a)\r\n    {\r\n        a++;\r\n\r\n        if (a > 1)\r\n        {\r\n        }\r\n    }\r\n}\r\n";

        await Verify(testCode, fixedCode, test => test.NumberOfIncrementalIterations = 1);
    }

    /// <summary>
    /// Verifies the spec example of comments that end on the statement's line: a block comment that starts the statement's
    /// line and a comment that starts an earlier line each get exactly one blank line above the line they start on, and a
    /// comment that starts behind the statement before it makes the fix move the statement below it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsOneBlankLineForSpecExampleOfCommentsEndingOnStatementLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        /* block */ {|RH5002:if|} (a > 1) { }

                                        a++;
                                        /* a
                                        b */ {|RH5002:if|} (a > 2) { }

                                        a++; /* a
                                        b */ {|RH5002:if|} (a > 3) { }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++;

                                         /* block */ if (a > 1) { }

                                         a++;

                                         /* a
                                         b */ if (a > 2) { }

                                         a++; /* a
                                         b */

                                         if (a > 3) { }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 3;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix inserts exactly one blank line above a block comment that starts the reported
    /// statement's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsOneBlankLineAboveBlockCommentStartingStatementLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        /* block */ {|RH5002:if|} (a > 7) { }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++;

                                         /* block */ if (a > 7) { }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix inserts exactly one blank line above a block comment that starts the reported
    /// statement's line under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsOneBlankLineAboveBlockCommentStartingStatementLineWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        /* block */ {|RH5002:if|} (a > 7) { }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++;

                                         /* block */ if (a > 7) { }
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode),
                     NormalizeToCarriageReturnLineFeed(fixedCode),
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix inserts exactly one blank line above a multi-line comment that starts its own
    /// line and ends on the reported statement's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsOneBlankLineAboveMultiLineCommentEndingOnStatementLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        /* a
                                        b */ {|RH5002:if|} (a > 7) { }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++;

                                         /* a
                                         b */ if (a > 7) { }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix inserts exactly one blank line above a comment line that stands above a block
    /// comment starting the reported statement's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsOneBlankLineAboveCommentLineAboveBlockCommentStartingStatementLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        /* above */
                                        /* block */ {|RH5002:if|} (a > 7) { }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++;

                                         /* above */
                                         /* block */ if (a > 7) { }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix inserts exactly one blank line above two block comments that start the reported
    /// statement's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsOneBlankLineAboveTwoBlockCommentsStartingStatementLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        /* x */ /* y */ {|RH5002:if|} (a > 7) { }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++;

                                         /* x */ /* y */ if (a > 7) { }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix inserts exactly one blank line above a block comment that starts the reported
    /// statement's line when a line comment ends the previous statement's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsOneBlankLineAboveBlockCommentWhenLineCommentEndsPreviousLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++; // x
                                        /* b */ {|RH5002:if|} (a > 7) { }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++; // x

                                         /* b */ if (a > 7) { }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix inserts exactly one blank line above a delimited documentation comment that
    /// starts the reported statement's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsOneBlankLineAboveDelimitedDocumentationCommentStartingStatementLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        /** d */ {|RH5002:if|} (a > 7) { }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++;

                                         /** d */ if (a > 7) { }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix moves a statement out from behind a multi-line comment that starts behind the
    /// statement before it and holds every line break between them, with one blank line and no whitespace at the end of
    /// the comment's last line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixMovesStatementBehindCommentStartingBehindPreviousStatement()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++; /* a
                                        b */ {|RH5002:if|} (a > 7) { }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++; /* a
                                         b */

                                         if (a > 7) { }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix moves a statement out from behind a multi-line comment that starts behind the
    /// statement before it under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixMovesStatementBehindCommentStartingBehindPreviousStatementWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++; /* a
                                        b */ {|RH5002:if|} (a > 7) { }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++; /* a
                                         b */

                                         if (a > 7) { }
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode),
                     NormalizeToCarriageReturnLineFeed(fixedCode),
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix inserts exactly one blank line above a comment line that a line comment ends
    /// before the reported statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixInsertsOneBlankLineAboveBlockCommentLineEndedByLineComment()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        /* b */ // c
                                        {|RH5002:if|} (a > 7) { }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++;

                                         /* b */ // c
                                         if (a > 7) { }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies one application of the fix moves a statement that shares its line with the statement before it and a block
    /// comment between them to a new line, leaving the comment behind the statement before it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyFixMovesStatementBehindSingleLineCommentSharingPreviousStatementLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++; /* c */ {|RH5002:if|} (a > 7) { }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++; /* c */

                                         if (a > 7) { }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies that inside an object initializer, an if statement that follows a statement behind a case label spanning
    /// two lines takes the column of break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyMovedIfStatementTakesSectionColumnWhenLabelSpansMultipleLinesInsideObjectInitializer()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = value =>
                                        {
                                            switch (value)
                                            {
                                                case 1 when
                                                    Check(value): Method(1); {|RH5002:if|} (value > 2) { }
                                                    break;
                                            }
                                        }
                                    };

                                    private static bool Check(int value)
                                    {
                                        return true;
                                    }

                                    private static void Method(int value)
                                    {
                                    }
                                }
                                internal sealed class Config
                                {
                                    public System.Action<int> Handler { get; set; }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5002
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = value =>
                                         {
                                             switch (value)
                                             {
                                                 case 1 when
                                                     Check(value): Method(1);

                                                     if (value > 2) { }
                                                     break;
                                             }
                                         }
                                     };

                                     private static bool Check(int value)
                                     {
                                         return true;
                                     }

                                     private static void Method(int value)
                                     {
                                     }
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action<int> Handler { get; set; }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies the spec example: the empty line directly below a documentation comment is blank, so the if statement
    /// below it is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    public async Task VerifyNoDiagnosticForSpecExampleEmptyLineBelowDocumentationComment()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (a > 0)
                                        {
                                            a--;

                                            break;
                                            /// doc

                                            if (a > 1)
                                            {
                                            }
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the line directly below a documentation comment that holds only spaces is blank, so the if statement below
    /// it is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    public async Task VerifyNoDiagnosticWhenLineBelowDocumentationCommentHoldsSpaces()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (a > 0)
                                        {
                                            a--;

                                            break;
                                            /// doc
                                            ····
                                            if (a > 1)
                                            {
                                            }
                                        }
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(testCode));
    }

    /// <summary>
    /// Verifies the line directly below a documentation comment that holds only spaces is blank also when documentation
    /// comments are not parsed, so the if statement below it is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    public async Task VerifyNoDiagnosticWhenLineBelowDocumentationCommentHoldsSpacesWithDocumentationModeNone()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (a > 0)
                                        {
                                            a--;

                                            break;
                                            /// doc
                                            ····
                                            if (a > 1)
                                            {
                                            }
                                        }
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(testCode), test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the line directly below a documentation comment that holds only spaces is blank under CRLF line endings,
    /// so the if statement below it is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    public async Task VerifyNoDiagnosticWhenLineBelowDocumentationCommentHoldsSpacesWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (a > 0)
                                        {
                                            a--;

                                            break;
                                            /// doc
                                            ····
                                            if (a > 1)
                                            {
                                            }
                                        }
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(NormalizeToCarriageReturnLineFeed(testCode)));
    }

    /// <summary>
    /// Verifies the empty line directly below a documentation comment is blank when a comment line stands between it and
    /// the if statement, so the if statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    public async Task VerifyNoDiagnosticWhenBlankLineBelowDocumentationCommentPrecedesCommentLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        /// doc

                                        // note
                                        if (a > 1)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the spec example: the empty line directly below an endif directive is blank, so the if statement below the
    /// comment line under it is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    public async Task VerifyNoDiagnosticForSpecExampleEmptyLineBelowEndIfDirective()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                #if DEBUG
                                        a--;
                                #endif

                                        // note
                                        if (a > 1)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the line directly below an endif directive that holds only spaces is blank, so the if statement below the
    /// comment line under it is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    public async Task VerifyNoDiagnosticWhenLineBelowEndIfDirectiveHoldsSpaces()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                #if DEBUG
                                        a--;
                                #endif
                                                ····
                                        // note
                                        if (a > 1)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(testCode));
    }

    /// <summary>
    /// Verifies the line directly below an endif directive that holds only spaces is blank under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    public async Task VerifyNoDiagnosticWhenLineBelowEndIfDirectiveHoldsSpacesWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                #if DEBUG
                                        a--;
                                #endif
                                                ····
                                        // note
                                        if (a > 1)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(NormalizeToCarriageReturnLineFeed(testCode)));
    }

    /// <summary>
    /// Verifies the empty line directly below a pragma directive is blank, so the if statement below the comment line
    /// under it is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    public async Task VerifyNoDiagnosticWhenEmptyLineFollowsPragmaDirective()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                #pragma warning disable CS0168

                                        // note
                                        if (a > 1)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an if statement directly below a documentation comment, with no blank line, stays reported and the fix
    /// puts the blank line above the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    [Requirement("SHARED-STMT-004")]
    public async Task VerifyDiagnosticWhenDocumentationCommentDirectlyPrecedesIfStatement()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a++;
                                        /// doc
                                        {|RH5002:if|} (a > 1)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a++;

                                         /// doc
                                         if (a > 1)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5002
                                {
                                    public void Execute()
                                    {
                                        var value = 1;
                                        {|#0:if|} (value > 0)
                                        {
                                            value++;
                                        }
                                        {|#1:if|} (value > 1)
                                        {
                                            value++;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5002
                                 {
                                     public void Execute()
                                     {
                                         var value = 1;

                                         if (value > 0)
                                         {
                                             value++;
                                         }

                                         if (value > 1)
                                         {
                                             value++;
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent if statements, each missing its preceding blank line, so the second fix's insertion point
        // sits directly against the first fix's closing brace
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5002MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}