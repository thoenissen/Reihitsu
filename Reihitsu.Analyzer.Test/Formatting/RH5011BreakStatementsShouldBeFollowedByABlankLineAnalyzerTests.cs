using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzerTests : AnalyzerTestsBase<RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer, RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported and fixed when a break statement is immediately followed by another statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// (issue #415)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    public async Task Issue469SameLineBreakFixSeparatesAndConverges()
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

        var onceFixed = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode));

        Assert.Contains("break;\r\n\r\n", onceFixed);
        Assert.DoesNotContain("\n", onceFixed.Replace("\r\n", string.Empty));
    }

    /// <summary>
    /// Verifies a same-line fix is withheld when the inter-statement gap contains a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}