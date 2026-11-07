using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Documentation;

/// <summary>
/// Formatter validation tests for <see cref="RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer"/>
/// </summary>
[TestClass]
public class RH8305SummaryElementMustSpanAtLeastThreeLinesFormatterTests : FormatterTestsBase<RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter expands single-line summary elements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 /// <summary>Summary text</summary>
                                 void Method()
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /// <summary>
                                     /// Summary text
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, 3, 9, 3, 40, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies that comments the formatter leaves as written are neither reported nor changed by the formatter, under
    /// LF and CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-010")]
    [Requirement("RH8305-002")]
    public async Task VerifyFormatterLeavesCommentsKeptAsWrittenUnchanged()
    {
        const string input = """
                             class C
                             {
                                 /// <summary>S
                                 int A;

                                 /// <summary>S</returns>
                                 int B;

                                 /// <summary>Gets the value.</summary>
                                 /// <remarks>Text
                                 /// more
                                 int D;

                                 int E; /// <summary>end</summary>
                             }
                             """;

        await VerifyFormatter(input);
    }

    #endregion // Tests
}