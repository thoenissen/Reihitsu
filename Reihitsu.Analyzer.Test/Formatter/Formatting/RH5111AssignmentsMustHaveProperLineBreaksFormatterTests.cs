using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5111AssignmentsMustHaveProperLineBreaksAnalyzer"/>
/// </summary>
[TestClass]
public class RH5111AssignmentsMustHaveProperLineBreaksFormatterTests : FormatterTestsBase<RH5111AssignmentsMustHaveProperLineBreaksAnalyzer>
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifies that the formatter keeps assignments on a single line around the equals sign
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     var value
                                         = "test";
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var value = "test";
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, 5, 13, 6, 21, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter keeps property initializers on one line around the equals sign
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesPropertyInitializerViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal string Value { get; set; }
                                     = "test";
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal string Value { get; set; } = "test";
                                 }
                                 """;

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               ExpectedDiagnostic(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, 3, 5, 4, 18, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter and analyzer agree on a parameter default value split after the equals sign
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesParameterDefaultValueSplitAfterEquals()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method({|#0:int value =
                                     5|})
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method(int value = 5)
                                     {
                                     }
                                 }
                                 """;

        foreach (var endOfLine in new[] { "\n", "\r\n" })
        {
            var normalizedInput = input.Replace("{|#0:", string.Empty).Replace("|}", string.Empty).Replace("\r\n", "\n").Replace("\n", endOfLine);
            var normalizedFixedData = fixedData.Replace("\r\n", "\n").Replace("\n", endOfLine);
            var syntaxTree = CSharpSyntaxTree.ParseText(normalizedInput, cancellationToken: TestContext.CancellationToken);
            var formatted = FormattingPipeline.Execute(syntaxTree.GetRoot(TestContext.CancellationToken), new FormattingContext(endOfLine), TestContext.CancellationToken).ToFullString();

            Assert.AreEqual(normalizedFixedData, formatted, "Formatter output should join the parameter default value.");
        }

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter and analyzer agree on an enum-member value split after the equals sign
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesEnumMemberValueSplitAfterEquals()
    {
        const string input = """
                             internal enum Values
                             {
                                 {|#0:First =
                                     1|}
                             }
                             """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First = 1
                                 }
                                 """;

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, AnalyzerResources.RH5111MessageFormat));
    }

    #endregion // Tests
}