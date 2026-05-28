using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5403StatementMustNotBeOnSingleLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5403StatementMustNotBeOnSingleLineFormatterTests : FormatterTestsBase<RH5403StatementMustNotBeOnSingleLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter expands single-line statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method()
                                 {
                                     if (true) {|#0:{|} return; }
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5403StatementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5403MessageFormat));
    }

    #endregion // Tests
}