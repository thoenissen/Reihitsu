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
/// Formatter validation tests for <see cref="RH0393LocalDeclarationsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0393LocalDeclarationsShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH0393LocalDeclarationsShouldBePrecededByABlankLineAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter inserts a blank line before a local declaration after an expression statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterInsertsBlankLineBeforeLocalDeclaration()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     Consume();
                                     var value = GetValue();
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
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Consume();

                                         var value = GetValue();
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

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0393LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 6, 9, 6, 12, AnalyzerResources.RH0393MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter leaves a local declaration at the beginning of a block untouched
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterLeavesLocalDeclarationAtStartOfBlockUntouched()
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

        await VerifyFormatterLeavesCodeUnchanged(input);
    }

    /// <summary>
    /// Verifies that the formatter leaves consecutive local declarations untouched
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterLeavesConsecutiveLocalDeclarationsUntouched()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     Consume();

                                     var first = GetValue();
                                     var second = GetValue();

                                     Consume(first + second);
                                 }

                                 private string GetValue()
                                 {
                                     return string.Empty;
                                 }

                                 private void Consume()
                                 {
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