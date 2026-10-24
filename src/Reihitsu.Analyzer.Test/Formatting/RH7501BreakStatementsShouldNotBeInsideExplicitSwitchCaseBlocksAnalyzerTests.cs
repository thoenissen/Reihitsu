using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer"/> and <see cref="RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzerTests : BatchCodeFixTestsBase<RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer, RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported when a switch case block ends with a break statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-001")]
    [Requirement("RH7501-003")]
    [Requirement("RH7501-004")]
    public async Task VerifyDiagnosticWhenBreakStatementIsInsideExplicitSwitchCaseBlock()
    {
        const string testCode = """
                                internal class RH7501
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    Consume();
                                                    {|#0:break|};
                                                }
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH7501
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 {
                                                     Consume();
                                                 }
                                                 break;
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId, AnalyzerResources.RH7501MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix keeps comments attached to the moved break statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-004")]
    public async Task VerifyCodeFixPreservesCommentsWhenMovingBreakStatement()
    {
        const string testCode = """
                                internal class RH7501
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    Consume();
                                                    // Exit this case
                                                    {|#0:break|}; // trailing comment
                                                }
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH7501
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 {
                                                     Consume();
                                                 }
                                                 // Exit this case
                                                 break; // trailing comment
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId, AnalyzerResources.RH7501MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix does not reformat unrelated switch section layout
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-004")]
    public async Task VerifyCodeFixDoesNotReformatNonCompliantSwitchSectionLayout()
    {
        const string testCode = """
                                internal class RH7501
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                        case 1:
                                        {
                                            Consume();
                                            {|#0:break|};
                                        }
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH7501
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                         case 1:
                                         {
                                             Consume();
                                         }
                                         break;
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId, AnalyzerResources.RH7501MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when the break statement already follows the explicit switch case block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-002")]
    public async Task VerifyNoDiagnosticWhenBreakStatementFollowsExplicitSwitchCaseBlock()
    {
        const string testCode = """
                                internal class RH7501
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    Consume();
                                                }
                                                break;
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
    /// Verifies no diagnostics are reported for break statements inside nested loop blocks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-001")]
    public async Task VerifyNoDiagnosticWhenBreakStatementBelongsToNestedLoop()
    {
        const string testCode = """
                                internal class RH7501
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    while (true) 
                                                    {
                                                        break;
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no code fix is offered when the break statement is not the last statement in the explicit switch case block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoCodeFixWhenBreakStatementIsNotLastStatementInExplicitSwitchCaseBlock()
    {
        const string testCode = """
                                internal class RH7501
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    break;
                                                    Consume();
                                                }
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<BreakStatementSyntax>()
                                                               .Single()
                                                               .BreakKeyword
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies no diagnostic is reported when the break statement is not the last statement in the explicit switch case block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-001")]
    public async Task VerifyNoDiagnosticWhenBreakStatementIsNotLastStatementInExplicitSwitchCaseBlock()
    {
        const string testCode = """
                                internal class RH7501
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    break;
                                                    Consume();
                                                }
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
    /// Verifies no diagnostic is reported when the explicit switch case block is not the last statement in the switch section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-001")]
    public async Task VerifyNoDiagnosticWhenBlockIsNotLastStatementInSwitchSection()
    {
        const string testCode = """
                                internal class RH7501
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    Consume();
                                                    break;
                                                }
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
    /// Verifies that a break that ends a block which is the last statement of its section is reported on its keyword,
    /// whatever statements precede the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-001")]
    [Requirement("RH7501-003")]
    public async Task VerifyDiagnosticsForBreakEndingSectionBlockWhateverStatementsPrecedeIt()
    {
        const string testData = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    {|RH7501:break|};
                                                }

                                            case 2:
                                                A();
                                                {
                                                    A();
                                                    {|RH7501:break|};
                                                }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a break that is not the block's last statement, a block that is not the section's last statement, a
    /// block nested in the section's block, and a break of a loop inside the block are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-001")]
    public async Task VerifyNoDiagnosticsForBreakThatDoesNotEndTheSectionBlock()
    {
        const string testData = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    break;
                                                    A();
                                                }

                                            case 2:
                                                {
                                                    A();
                                                    break;
                                                }
                                                A();

                                            case 3:
                                                {
                                                    {
                                                        A();
                                                        break;
                                                    }
                                                }

                                            case 4:
                                                {
                                                    while (true)
                                                    {
                                                        break;
                                                    }
                                                }
                                                break;
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a return or goto case statement that ends a section's block and a break that already follows the block
    /// are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-002")]
    public async Task VerifyNoDiagnosticsForOtherJumpStatementsEndingSectionBlock()
    {
        const string testData = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();

                                                    return 1;
                                                }

                                            case 2:
                                                {
                                                    A();

                                                    goto case 1;
                                                }

                                            case 3:
                                                {
                                                    A();
                                                }
                                                break;
                                        }

                                        return 0;
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a throw, continue, goto, or goto default statement that ends a section's block is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-002")]
    public async Task VerifyNoDiagnosticsForThrowContinueAndGotoDefaultEndingSectionBlock()
    {
        const string testData = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        while (true)
                                        {
                                            switch (x)
                                            {
                                                case 1:
                                                    {
                                                        A();
                                                        throw new System.InvalidOperationException();
                                                    }

                                                case 2:
                                                    {
                                                        A();
                                                        continue;
                                                    }

                                                case 3:
                                                    {
                                                        A();
                                                        goto default;
                                                    }

                                                case 4:
                                                    {
                                                        A();
                                                        goto End;
                                                    }

                                                default:
                                                    {
                                                        A();
                                                        return;
                                                    }
                                            }
                                        }

                                        End:
                                        A();
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a yield break statement that ends a section's block in an iterator is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-002")]
    public async Task VerifyNoDiagnosticForYieldBreakEndingSectionBlock()
    {
        const string testData = """
                                using System.Collections.Generic;

                                class C
                                {
                                    IEnumerable<int> M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    yield return 1;
                                                    yield break;
                                                }
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix moves the break behind the block, indented like the closing brace, together with the comment
    /// on the line in front of it and the comment behind it, and keeps a layout the formatter would not produce
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-004")]
    public async Task VerifyCodeFixMovesBreakWithItsCommentsBehindBlock()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    // leave the section
                                                    {|RH7501:break|}; // done
                                                }

                                            case 2:
                                            {
                                                A();
                                                {|RH7501:break|};
                                            }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(int x)
                                     {
                                         switch (x)
                                         {
                                             case 1:
                                                 {
                                                     A();
                                                 }
                                                 // leave the section
                                                 break; // done

                                             case 2:
                                             {
                                                 A();
                                             }
                                             break;
                                         }
                                     }

                                     void A()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix moves a comment in front of the break on its line together with the break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-004")]
    public async Task VerifyCodeFixMovesCommentInFrontOfBreakOnItsLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    /* leave */ {|RH7501:break|};
                                                }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(int x)
                                     {
                                         switch (x)
                                         {
                                             case 1:
                                                 {
                                                     A();
                                                 }
                                                 /* leave */ break;
                                         }
                                     }

                                     void A()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix keeps a comment behind the block's closing brace on the brace's line and places the break on
    /// the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-004")]
    public async Task VerifyCodeFixKeepsCommentBehindClosingBrace()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            default:
                                                {
                                                    A();
                                                    {|RH7501:break|};
                                                } // end of default
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(int x)
                                     {
                                         switch (x)
                                         {
                                             default:
                                                 {
                                                     A();
                                                 } // end of default
                                                 break;
                                         }
                                     }

                                     void A()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix indents the break like the block's closing brace when the opening brace shares the line of the
    /// case label
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-004")]
    public async Task VerifyCodeFixIndentsBreakLikeClosingBraceWhenOpeningBraceIsOnLabelLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1: {
                                                    A();
                                                    {|RH7501:break|};
                                                }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(int x)
                                     {
                                         switch (x)
                                         {
                                             case 1: {
                                                     A();
                                                 }
                                                 break;
                                         }
                                     }

                                     void A()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix keeps a comment on its own line between the break and the closing brace inside the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-004")]
    public async Task VerifyCodeFixKeepsCommentBetweenBreakAndClosingBraceInsideBlock()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    {|RH7501:break|};
                                                    // after the break
                                                }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(int x)
                                     {
                                         switch (x)
                                         {
                                             case 1:
                                                 {
                                                     A();
                                                     // after the break
                                                 }
                                                 break;
                                         }
                                     }

                                     void A()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix keeps carriage return line feed line endings when it moves the break behind the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-004")]
    public async Task VerifyCodeFixKeepsCarriageReturnLineFeedLineEndings()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    break;
                                                }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        void M(int x)
                                        {
                                            switch (x)
                                            {
                                                case 1:
                                                    {
                                                        A();
                                                    }
                                                    break;
                                            }
                                        }

                                        void A()
                                        {
                                        }
                                    }
                                    """;

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(expectedCode), await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode)));
    }

    /// <summary>
    /// Verifies that the fix moves the break of a section block that holds a nested switch whose own section block ends
    /// with a break, and converges for both
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7501-004")]
    public async Task VerifyFixAllMovesBreaksOfNestedSwitchSections()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    switch (x)
                                                    {
                                                        case 2:
                                                            {
                                                                A();
                                                                {|RH7501:break|};
                                                            }
                                                    }
                                                    {|RH7501:break|};
                                                }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(int x)
                                     {
                                         switch (x)
                                         {
                                             case 1:
                                                 {
                                                     switch (x)
                                                     {
                                                         case 2:
                                                             {
                                                                 A();
                                                             }
                                                             break;
                                                     }
                                                 }
                                                 break;
                                         }
                                     }

                                     void A()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Characterizes the fix when code that #if excludes lies in front of the break: the fix is offered and moves the
    /// directives and the excluded code behind the block together with the break. This test pins today's behavior
    /// without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixMovesExcludedCodeInFrontOfBreakBehindBlock()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    var y = 1;
                                #if DEBUG
                                                    A(y);
                                #endif
                                                    break;
                                                }
                                        }
                                    }

                                    void A(int y)
                                    {
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        void M(int x)
                                        {
                                            switch (x)
                                            {
                                                case 1:
                                                    {
                                                        var y = 1;
                                                    }
                                                    #if DEBUG
                                                        A(y);
                                    #endif
                                                        break;
                                            }
                                        }

                                        void A(int y)
                                        {
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes the fix when a region encloses the break inside the block: the fix is offered, moves the region
    /// directive behind the block with the break, and leaves the end of the region inside the block. This test pins
    /// today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixSplitsRegionAroundBreak()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    #region Exit
                                                    break;
                                                    #endregion
                                                }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        void M(int x)
                                        {
                                            switch (x)
                                            {
                                                case 1:
                                                    {
                                                        A();
                                                        #endregion
                                                    }
                                                    #region Exit
                                                        break;
                                            }
                                        }

                                        void A()
                                        {
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes the fix when conditional code follows the break inside the block: the fix is offered, moves the break
    /// behind the block, and leaves the conditional code inside it. This test pins today's behavior without stating it as
    /// intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixLeavesConditionalCodeBehindBreakInsideBlock()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    break;
                                #if DEBUG
                                                    A();
                                #endif
                                                }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        void M(int x)
                                        {
                                            switch (x)
                                            {
                                                case 1:
                                                    {
                                                        A();
                                    #if DEBUG
                                                        A();
                                    #endif
                                                    }
                                                    break;
                                            }
                                        }

                                        void A()
                                        {
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes the fix when a pragma pair encloses the break inside the block: the fix is offered, moves the
    /// disabling pragma behind the block with the break, and leaves the restoring pragma inside the block. This test pins
    /// today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixSeparatesPragmaPairAroundBreak()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                #pragma warning disable CS0162
                                                    break;
                                #pragma warning restore CS0162
                                                }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        void M(int x)
                                        {
                                            switch (x)
                                            {
                                                case 1:
                                                    {
                                                        A();
                                    #pragma warning restore CS0162
                                                    }
                                                    #pragma warning disable CS0162
                                                        break;
                                            }
                                        }

                                        void A()
                                        {
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes the fix when the whole section block shares the line of its case label: the fix is offered and
    /// duplicates text of the line in front of the brace. This test pins today's behavior without stating it as
    /// intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixOnSectionBlockOnOneLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1: { A(); break; }
                                            default: { A(); break; }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        void M(int x)
                                        {
                                            switch (x)
                                            {
                                                case 1: { A(); }
                                                case 1: { A(); break; break;             default: { A(); break; }
                                            }
                                        }

                                        void A()
                                        {
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes the fix when the break shares its line with the block's closing brace: the fix is offered and writes a
    /// second break the user did not write. This test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixWhenBreakSharesLineWithClosingBrace()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();
                                                    break; }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        void M(int x)
                                        {
                                            switch (x)
                                            {
                                                case 1:
                                                    {
                                                        A();
                                    }
                                                        break; break;         }
                                        }

                                        void A()
                                        {
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes the fix when the break shares its line with the statement in front of it: the fix is offered and joins
    /// the block's closing brace onto that statement's line. This test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixWhenBreakSharesLineWithStatementInFrontOfIt()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A(); break;
                                                }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        void M(int x)
                                        {
                                            switch (x)
                                            {
                                                case 1:
                                                    {
                                                        A();                 }
                                                    break;
                                            }
                                        }

                                        void A()
                                        {
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes the fix when the break is the only statement of the section block: the fix is offered and leaves an
    /// empty block in front of the break. This test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixLeavesEmptyBlockWhenBreakIsOnlyStatement()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    break;
                                                }
                                        }
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        void M(int x)
                                        {
                                            switch (x)
                                            {
                                                case 1:
                                                    {
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes the fix when a blank line separates the break from the statement in front of it: the blank line moves
    /// with the break, so it lies between the closing brace and the break. This test pins today's behavior without stating
    /// it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixMovesBlankLineInFrontOfBreakWithIt()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    A();

                                                    break;
                                                }
                                        }
                                    }

                                    void A()
                                    {
                                    }
                                }
                                """;

        const string expectedCode = """
                                    class C
                                    {
                                        void M(int x)
                                        {
                                            switch (x)
                                            {
                                                case 1:
                                                    {
                                                        A();
                                                    }

                                                    break;
                                            }
                                        }

                                        void A()
                                        {
                                        }
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH7501
                                {
                                    public void Execute(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    Consume();
                                                    {|#0:break|};
                                                }
                                            case 2:
                                                {
                                                    Consume();
                                                    {|#1:break|};
                                                }
                                        }
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class RH7501
                                 {
                                     public void Execute(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 {
                                                     Consume();
                                                 }
                                                 break;
                                             case 2:
                                                 {
                                                     Consume();
                                                 }
                                                 break;
                                         }
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        // Two switch sections on adjacent lines each carry their own explicit block ending with break; the
        // fixes only move their own block's break statement, so the batch fixer converges in one pass
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId, AnalyzerResources.RH7501MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}