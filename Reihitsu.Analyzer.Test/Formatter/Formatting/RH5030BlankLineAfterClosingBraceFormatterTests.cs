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
    /// Verifies the formatter leaves already compliant code unchanged
    /// </summary>
    /// <param name="source">Source code</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyFormatterLeavesCodeUnchanged(string source)
    {
        await Verify(source);

        var tree = CSharpSyntaxTree.ParseText(source);
        var context = new FormattingContext(Environment.NewLine);
        var formatted = FormattingPipeline.Execute(await tree.GetRootAsync(), context, CancellationToken.None).ToFullString();

        Assert.AreEqual(source, formatted, "Formatter output should keep compliant code unchanged.");

        await Verify(formatted);
    }

    #endregion // Tests
}