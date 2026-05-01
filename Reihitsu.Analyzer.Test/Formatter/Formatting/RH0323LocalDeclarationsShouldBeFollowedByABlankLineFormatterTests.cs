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
/// Formatter validation tests for <see cref="RH0323LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0323LocalDeclarationsShouldBeFollowedByABlankLineFormatterTests : FormatterTestsBase<RH0323LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter inserts a blank line between a local declaration with initializer and a following expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterInsertsBlankLineAfterLocalDeclarationBeforeExpressionStatement()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     var value = GetValue();
                                     Consume(value);
                                 }

                                 private string GetValue()
                                 {
                                     return string.Empty;
                                 }

                                 private void Consume(string value)
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var value = GetValue();

                                         Consume(value);
                                     }

                                     private string GetValue()
                                     {
                                         return string.Empty;
                                     }

                                     private void Consume(string value)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0323LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, 6, 9, 6, 16, AnalyzerResources.RH0323MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter inserts a blank line between a local declaration without initializer and a following expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterInsertsBlankLineAfterLocalDeclarationWithoutInitializerBeforeExpressionStatement()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     string text;
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
                                     internal void Method()
                                     {
                                         string text;

                                         Consume();
                                     }

                                     private void Consume()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0323LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, 6, 9, 6, 16, AnalyzerResources.RH0323MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter leaves an expression statement before a declaration untouched
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterLeavesExpressionStatementBeforeLocalDeclarationUntouched()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     Consume();
                                     var next = GetValue();
                                 }

                                 private string GetValue()
                                 {
                                     return string.Empty;
                                 }

                                 private void Consume()
                                 {
                                 }
                             }
                             """;

        await VerifyFormatterLeavesCodeUnchanged(input);
    }

    /// <summary>
    /// Verifies that the formatter leaves mixed declaration blocks and consecutive expression statements untouched
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterDoesNotSplitMixedDeclarationBlocksOrConsecutiveExpressionStatements()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     string text;
                                     var value = GetValue();

                                     Consume(value);
                                     Consume(value);
                                 }

                                 private string GetValue()
                                 {
                                     return string.Empty;
                                 }

                                 private void Consume(string value)
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