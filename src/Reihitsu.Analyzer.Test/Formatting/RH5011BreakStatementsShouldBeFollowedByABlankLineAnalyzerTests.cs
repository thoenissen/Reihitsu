using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer, RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported and fixed when a break statement is immediately followed by another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-001")]
    [Requirement("RH5011-003")]
    [Requirement("RH5011-004")]
    public async Task VerifyDiagnosticForBreakStatementWithoutFollowingBlankLine()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            {|#0:break|};
                                            Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5011
                                 {
                                     public void Execute()
                                     {
                                         while (true)
                                         {
                                             break;

                                             Consume();
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a break statement is followed by a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-001")]
    [Requirement("SHARED-BLANK-001")]
    public async Task VerifyNoDiagnosticForBreakStatementWithFollowingBlankLine()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            break;

                                            Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a break statement is the last statement before a closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-002")]
    public async Task VerifyNoDiagnosticForBreakStatementBeforeClosingBrace()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a diagnostic is reported when a comment line (rather than a whitespace-only blank line) directly
    /// follows the break statement, matching the formatter's whitespace-only blank-line definition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-001")]
    [Requirement("RH5011-004")]
    [Requirement("SHARED-BLANK-001")]
    public async Task VerifyDiagnosticForBreakStatementWhenCommentLineDirectlyFollowsIt()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            {|#0:break|};
                                            // Comment after break
                                            Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5011
                                 {
                                     public void Execute()
                                     {
                                         while (true)
                                         {
                                             break;

                                             // Comment after break
                                             Consume();
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when a break statement is immediately followed by an
    /// <c>#endregion</c> directive, mirroring the exemption the PrecededBy analyzer bases apply
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-002")]
    public async Task VerifyNoDiagnosticForBreakStatementFollowedByDirective()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            #region Guard
                                            break;
                                            #endregion
                                            Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a same-line following statement is split before a blank line is added after a break statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-005")]
    public async Task SameLineBreakFixSeparatesAndConverges()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            {|#0:break|}; Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5011
                                 {
                                     public void Execute()
                                     {
                                         while (true)
                                         {
                                             break;

                                             Consume();
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat));

        var crlfFixed = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode.Replace("{|#0:", string.Empty).Replace("|}", string.Empty)));

        Assert.Contains("break;\r\n\r\n            Consume();", crlfFixed);
        Assert.DoesNotContain("\n", crlfFixed.Replace("\r\n", string.Empty));
    }

    /// <summary>
    /// Verifies a same-line split preserves the break line's exact tab indentation and converges
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-005")]
    public async Task SameLineFixPreservesExactTabIndentationAndConverges()
    {
        const string testCode = "internal class RH5011\n{\n\tpublic void Execute()\n\t{\n\t\twhile (true)\n\t\t{\n\t\t\t{|#0:break|}; Consume();\n\t\t}\n\t}\n\n\tprivate void Consume()\n\t{\n\t}\n}";
        const string fixedCode = "internal class RH5011\n{\n\tpublic void Execute()\n\t{\n\t\twhile (true)\n\t\t{\n\t\t\tbreak;\n\n\t\t\tConsume();\n\t\t}\n\t}\n\n\tprivate void Consume()\n\t{\n\t}\n}";

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat));
    }

    /// <summary>
    /// Verifies separate-line insertion preserves CRLF and converges after one application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-004")]
    public async Task SeparateLineFixPreservesCarriageReturnLineFeedAndConverges()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            break;
                                            Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;
        const string markedTestCode = """
                                      internal class RH5011
                                      {
                                          public void Execute()
                                          {
                                              while (true)
                                              {
                                                  {|#0:break|};
                                                  Consume();
                                              }
                                          }

                                          private void Consume()
                                          {
                                          }
                                      }
                                      """;
        const string fixedCode = """
                                 internal class RH5011
                                 {
                                     public void Execute()
                                     {
                                         while (true)
                                         {
                                             break;

                                             Consume();
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(markedTestCode),
                     NormalizeToCarriageReturnLineFeed(fixedCode),
                     Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat));

        var onceFixed = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode));

        Assert.Contains("break;\r\n\r\n", onceFixed);
        Assert.DoesNotContain("\n", onceFixed.Replace("\r\n", string.Empty));
    }

    /// <summary>
    /// Verifies a same-line fix is withheld when the inter-statement gap contains a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-006")]
    public async Task SameLineFixIsNotOfferedAcrossComment()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            break; /* Keep. */ Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes().OfType<BreakStatementSyntax>().Single().BreakKeyword.GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies a same-line fix is withheld when the gap contains directive or skipped syntax
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SameLineFixIsNotOfferedAcrossDirectiveAndSkippedSyntax()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            break; #if FEATURE
                                            Consume();
                                            #endif
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes().OfType<BreakStatementSyntax>().Single().BreakKeyword.GetLocation(),
                                                   "FEATURE");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies a break statement is reported when a statement or the next switch section follows it without a blank
    /// line, including a following comment line, a trailing comment, a same-line statement, and an embedded break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-001")]
    [Requirement("RH5011-003")]
    public async Task VerifyBreakFollowedByStatementOrSwitchSectionIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                a++;
                                                {|RH5011:break|};
                                            case 2:
                                                break;

                                            default:
                                                break;
                                        }

                                        while (true)
                                        {
                                            if (a > 0)
                                                {|RH5011:break|};
                                            a++;
                                            {|RH5011:break|};
                                            // note
                                            a++;
                                            {|RH5011:break|}; // note
                                            a++;
                                            {|RH5011:break|}; a++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a break statement is not reported when a closing brace or a preprocessor directive follows it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-002")]
    public async Task VerifyBreakBeforeClosingBraceOrDirectiveIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                break;
                                        }

                                        while (true)
                                        {
                                            break;
                                #pragma warning disable CS0168
                                            a++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix inserts the blank line directly below the break line, keeping a trailing comment on that line
    /// and a comment on the following line directly above its code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-001")]
    [Requirement("RH5011-004")]
    public async Task VerifySeparateLineFixInsertsBlankLineBelowBreakLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                a++;
                                                {|RH5011:break|};
                                            default:
                                                break;
                                        }

                                        while (true)
                                        {
                                            {|RH5011:break|};
                                            // note
                                            a++;
                                            {|RH5011:break|}; // note
                                            a++;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         switch (a)
                                         {
                                             case 1:
                                                 a++;
                                                 break;

                                             default:
                                                 break;
                                         }

                                         while (true)
                                         {
                                             break;

                                             // note
                                             a++;
                                             break; // note

                                             a++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a break statement followed by a comment and a statement on its own line is reported and left unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-001")]
    [Requirement("RH5011-006")]
    public async Task VerifyCommentBetweenBreakAndSameLineStatementIsReportedWithoutFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (true)
                                        {
                                            {|RH5011:break|}; /* note */ a++;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies the spec example: when a comment behind the break spans the end of its line and the next token starts on a
    /// line of its own, the fix inserts the blank line below the line on which the comments end, for a statement and a
    /// switch label, also behind a line comment that ends that line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-007")]
    public async Task VerifyFixInsertsBlankLineBelowCommentThatSpansTheBreakLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                {|RH5011:break|}; /* a
                                                b */
                                            default:
                                                break;
                                        }

                                        while (true)
                                        {
                                            {|RH5011:break|}; /* a
                                            b */
                                            a++;
                                            {|RH5011:break|}; /* x */ /* a
                                            b */ // c
                                            // note
                                            a++;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         switch (a)
                                         {
                                             case 1:
                                                 break; /* a
                                                 b */

                                             default:
                                                 break;
                                         }

                                         while (true)
                                         {
                                             break; /* a
                                             b */

                                             a++;
                                             break; /* x */ /* a
                                             b */ // c

                                             // note
                                             a++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix inserts the blank line below a comment that spans the end of the break line when the next statement
    /// starts on a line of its own, in one application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-007")]
    public async Task VerifyFixInsertsBlankLineBelowCommentSpanningBreakLineBeforeStatementInOneApplication()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (true)
                                        {
                                            {|#0:break|}; /* a
                                            b */
                                            a++;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         while (true)
                                         {
                                             break; /* a
                                             b */

                                             a++;
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
                     },
                     Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat));
    }

    /// <summary>
    /// Verifies the fix inserts the blank line below a comment that spans the end of the break line, with CRLF line endings,
    /// and writes only CRLF line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-007")]
    [Requirement("SHARED-FIX-008")]
    public async Task VerifyFixInsertsBlankLineBelowCommentSpanningBreakLineWithCarriageReturnLineFeed()
    {
        const string testCode = "class C\n{\n    void M(int a)\n    {\n        while (true)\n        {\n            break; /* a\n            b */\n            a++;\n        }\n    }\n}\n";
        const string fixedCode = "class C\n{\n    void M(int a)\n    {\n        while (true)\n        {\n            break; /* a\n            b */\n\n            a++;\n        }\n    }\n}\n";

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedCode), actual);

        await Verify(actual);
    }

    /// <summary>
    /// Verifies the fix inserts the blank line below a comment that spans the end of the break line when a switch label
    /// starts on a line of its own below it, in one application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-007")]
    public async Task VerifyFixInsertsBlankLineBelowCommentSpanningBreakLineBeforeSwitchLabel()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                {|#0:break|}; /* a
                                                b */
                                            default:
                                                break;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         switch (a)
                                         {
                                             case 1:
                                                 break; /* a
                                                 b */

                                             default:
                                                 break;
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
                     },
                     Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat));
    }

    /// <summary>
    /// Verifies the fix inserts the blank line below the line on which the comments behind the break end, when a second
    /// comment spans the line end and a line comment ends that line, keeping the comment line above the next statement
    /// directly above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-007")]
    public async Task VerifyFixInsertsBlankLineBelowLineCommentThatEndsTheSpanningCommentsLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (true)
                                        {
                                            {|#0:break|}; /* x */ /* a
                                            b */ // c
                                            // note
                                            a++;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         while (true)
                                         {
                                             break; /* x */ /* a
                                             b */ // c

                                             // note
                                             a++;
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
                     },
                     Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat));
    }

    /// <summary>
    /// Verifies the spec example: when the next statement or switch label starts behind a comment that spans the end of
    /// the break line, on the comment's last line, the break is reported and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-007")]
    public async Task VerifyNoFixIsOfferedWhenCodeStartsBehindCommentSpanningBreakLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                {|RH5011:break|}; /* a
                                                b */ default:
                                                break;
                                        }

                                        while (true)
                                        {
                                            {|RH5011:break|}; /* a
                                            b */ a++;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies no fix is offered when a statement starts behind a comment that spans the end of the break line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-007")]
    public async Task VerifyNoFixIsOfferedWhenStatementStartsBehindCommentSpanningBreakLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (true)
                                        {
                                            {|RH5011:break|}; /* a
                                            b */ a++;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies no fix is offered when a switch label starts behind a comment that spans the end of the break line, also
    /// with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-007")]
    public async Task VerifyNoFixIsOfferedWhenSwitchLabelStartsBehindCommentSpanningBreakLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                {|RH5011:break|}; /* a
                                                b */ default:
                                                break;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testCode));
    }

    /// <summary>
    /// Verifies a multi-line documentation-style comment behind the break that spans the end of its line is handled as the
    /// harness parses it: reported, with the blank line inserted below the comment's last line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-007")]
    public async Task VerifyFixInsertsBlankLineBelowDelimitedDocumentationCommentSpanningBreakLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (true)
                                        {
                                            {|#0:break|}; /** a
                                            b */
                                            a++;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         while (true)
                                         {
                                             break; /** a
                                             b */

                                             a++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat));
    }

    /// <summary>
    /// Verifies a multi-line comment that starts on the line below the break leaves the break line ending outside a
    /// comment, so the blank line goes directly below the break line, above the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-004")]
    public async Task VerifyFixInsertsBlankLineBelowBreakLineAboveMultiLineCommentOnNextLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (true)
                                        {
                                            {|#0:break|};
                                            /* a
                                            b */
                                            a++;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         while (true)
                                         {
                                             break;

                                             /* a
                                             b */
                                             a++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat));
    }

    /// <summary>
    /// Verifies Fix All inserts the blank line below a comment that spans the end of a break line and below a plain break
    /// line in one document, and leaves a break whose fix is withheld reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-004")]
    [Requirement("RH5011-007")]
    public async Task VerifyFixAllHandlesSpanningCommentNextToPlainBreakAndWithheldBreak()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (true)
                                        {
                                            {|RH5011:break|}; /* a
                                            b */
                                            a++;
                                        }
                                        while (true)
                                        {
                                            {|RH5011:break|};
                                            a++;
                                        }
                                        while (true)
                                        {
                                            {|RH5011:break|}; /* a
                                            b */ a++;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         while (true)
                                         {
                                             break; /* a
                                             b */

                                             a++;
                                         }
                                         while (true)
                                         {
                                             break;

                                             a++;
                                         }
                                         while (true)
                                         {
                                             {|RH5011:break|}; /* a
                                             b */ a++;
                                         }
                                     }
                                 }
                                 """;

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
    /// Verifies a break whose comment spans the end of its line is reported when a preprocessor directive follows the
    /// comment, because the first trivia behind the semicolon is the comment, and the fix inserts the blank line below the
    /// line on which the comment ends
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5011-007")]
    public async Task VerifyFixInsertsBlankLineBelowCommentSpanningBreakLineBeforeDirective()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        while (true)
                                        {
                                            {|#0:break|}; /* a
                                            b */
                                #pragma warning disable CS0168
                                            a++;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         while (true)
                                         {
                                             break; /* a
                                             b */

                                 #pragma warning disable CS0168
                                             a++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5011
                                {
                                    public void Execute()
                                    {
                                        while (true)
                                        {
                                            {|#0:break|};
                                            Consume();
                                        }
                                        while (true)
                                        {
                                            {|#1:break|};
                                            Consume();
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class RH5011
                                 {
                                     public void Execute()
                                     {
                                         while (true)
                                         {
                                             break;

                                             Consume();
                                         }
                                         while (true)
                                         {
                                             break;

                                             Consume();
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        // Two adjacent while loops, each with a break statement missing its following blank line, so the second
        // fix's insertion point is unaffected by the first occurrence's own blank-line insertion
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5011MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}