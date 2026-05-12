using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0394BlankLineAfterClosingBraceAnalyzer"/>
/// </summary>
[TestClass]
public class RH0394BlankLineAfterClosingBraceFormatterTests : FormatterTestsBase<RH0394BlankLineAfterClosingBraceAnalyzer>
{
    #region Members

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
                                 ExpectedDiagnostic(RH0394BlankLineAfterClosingBraceAnalyzer.DiagnosticId, 8, 9, 8, 10, AnalyzerResources.RH0394MessageFormat));
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

    #endregion // Members
}