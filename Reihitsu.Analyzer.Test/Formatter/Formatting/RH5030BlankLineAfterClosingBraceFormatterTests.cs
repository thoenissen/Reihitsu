using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5030BlankLineAfterClosingBraceAnalyzer"/>
/// </summary>
[TestClass]
public class RH5030BlankLineAfterClosingBraceFormatterTests : FormatterTestsBase<RH5030BlankLineAfterClosingBraceAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter inserts a blank line after a closing brace when the next statement follows immediately
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterInsertsBlankLineAfterClosingBrace()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(bool flag)
                                 {
                                     if (flag)
                                     {
                                         Consume();
                                     }
                                     Consume();
                                 }

                                 private void Consume()
                                 {
                                 }
                             }
                             """;

        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method(bool flag)
                                     {
                                         if (flag)
                                         {
                                             Consume();
                                         }

                                         Consume();
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5030BlankLineAfterClosingBraceAnalyzer.DiagnosticId, 8, 9, 8, 10, AnalyzerResources.RH5030MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter inserts a blank line after an <c>#if</c> directive that immediately follows
    /// a closing brace, positioning it below the directive rather than skipping it or inserting it above the
    /// directive inside the conditional region. RH5030 has no directive exemption, so the formatter must stay
    /// in parity with <see cref="Reihitsu.Analyzer.CodeFixes.Rules.Layout.RH5030BlankLineAfterClosingBraceCodeFixProvider"/>
    /// here rather than exempt the statement entirely (issue #415)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterInsertsBlankLineAfterDirectiveFollowingClosingBrace()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(bool flag)
                                 {
                                     if (flag)
                                     {
                                         Consume();
                                     }
                             #if true
                                     Consume();
                             #endif
                                 }

                                 private void Consume()
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                internal class Example
                                {
                                    internal void Method(bool flag)
                                    {
                                        if (flag)
                                        {
                                            Consume();
                                        }
                                #if true

                                        Consume();
                                #endif
                                    }

                                    private void Consume()
                                    {
                                    }
                                }
                                """;

        await VerifyFormatterOutput(input, expected);
    }

    /// <summary>
    /// Verifies that the formatter leaves code untouched when a blank line is already present after a closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterLeavesBlankLineAlreadyPresentUntouched()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(bool flag)
                                 {
                                     if (flag)
                                     {
                                         Consume();
                                     }

                                     Consume();
                                 }

                                 private void Consume()
                                 {
                                 }
                             }
                             """;

        await VerifyFormatterLeavesCodeUnchanged(input);
    }

    /// <summary>
    /// Verifies that the formatter leaves code untouched when the closing brace is the last statement in the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterLeavesCodeWithClosingBraceAsLastStatementUntouched()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(bool flag)
                                 {
                                     if (flag)
                                     {
                                         Consume();
                                     }
                                 }

                                 private void Consume()
                                 {
                                 }
                             }
                             """;

        await VerifyFormatterLeavesCodeUnchanged(input);
    }

    /// <summary>
    /// Verifies that the formatter leaves else clauses following a closing brace untouched
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterLeavesElseClauseUntouched()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(bool flag)
                                 {
                                     if (flag)
                                     {
                                         Consume();
                                     }
                                     else
                                     {
                                         Consume();
                                     }
                                 }

                                 private void Consume()
                                 {
                                 }
                             }
                             """;

        await VerifyFormatterLeavesCodeUnchanged(input);
    }

    /// <summary>
    /// Verifies that the formatter leaves do-while statements untouched
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterLeavesDoWhileUntouched()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     do
                                     {
                                         Consume();
                                     }
                                     while (GetValue());
                                 }

                                 private bool GetValue()
                                 {
                                     return false;
                                 }

                                 private void Consume()
                                 {
                                 }
                             }
                             """;

        await VerifyFormatterLeavesCodeUnchanged(input);
    }

    /// <summary>
    /// Verifies that the formatter does not insert a blank line between the main block and break in a switch case
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterDoesNotInsertBlankLineBeforeBreakInSwitchCase()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(int value)
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

        await VerifyFormatterLeavesCodeUnchanged(input);
    }

    /// <summary>
    /// Verifies that the formatter does not insert a blank line before a break statement that follows a closing
    /// brace when both are inside a single block that is itself the braced body of a switch section (issue #440)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterDoesNotInsertBlankLineBeforeBreakInsideBracedSwitchSection()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             {
                                                 if (GetValue())
                                                 {
                                                     Consume();
                                                 }
                                                 break;
                                             }
                                     }
                                 }

                                 private bool GetValue()
                                 {
                                     return false;
                                 }

                                 private void Consume()
                                 {
                                 }
                             }
                             """;

        await VerifyFormatterLeavesCodeUnchanged(input);
    }

    /// <summary>
    /// Verifies that the formatter inserts a blank line after a closing brace even when a comment separates it
    /// from the next statement, since a single line break on each side of a comment is not a blank line (issue #440)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterInsertsBlankLineWhenCommentSeparatesClosingBraceFromNextStatement()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(bool flag)
                                 {
                                     if (flag)
                                     {
                                         Consume();
                                     }
                                     // Comment after block
                                     Consume();
                                 }

                                 private void Consume()
                                 {
                                 }
                             }
                             """;

        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method(bool flag)
                                     {
                                         if (flag)
                                         {
                                             Consume();
                                         }

                                         // Comment after block
                                         Consume();
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5030BlankLineAfterClosingBraceAnalyzer.DiagnosticId, 8, 9, 8, 10, AnalyzerResources.RH5030MessageFormat));
    }

    /// <summary>
    /// Verifies the formatter leaves already compliant code unchanged
    /// </summary>
    /// <param name="source">Source code</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyFormatterLeavesCodeUnchanged(string source)
    {
        await Verify(source);

        await VerifyFormatterOutput(source, source);

        await Verify(source);
    }

    /// <summary>
    /// Runs the formatter pipeline over the specified source and asserts the produced output
    /// </summary>
    /// <param name="source">Source code to format</param>
    /// <param name="expected">The expected formatted output</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyFormatterOutput(string source, string expected)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var context = new FormattingContext(Environment.NewLine);
        var formatted = FormattingPipeline.Execute(await tree.GetRootAsync(), context, CancellationToken.None).ToFullString();

        Assert.AreEqual(expected, formatted, "Formatter output did not match the expected result.");
    }

    #endregion // Tests
}