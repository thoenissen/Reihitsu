using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0361ElementMustNotBeOnSingleLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0361ElementMustNotBeOnSingleLineFormatterTests : FormatterTestsBase<RH0361ElementMustNotBeOnSingleLineAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter expands single-line elements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class {|#0:Example|} { }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0361ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH0361MessageFormat));
    }

    #endregion // Members
}