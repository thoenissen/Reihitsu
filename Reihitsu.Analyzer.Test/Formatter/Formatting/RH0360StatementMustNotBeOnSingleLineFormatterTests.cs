using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0360StatementMustNotBeOnSingleLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0360StatementMustNotBeOnSingleLineFormatterTests : FormatterTestsBase<RH0360StatementMustNotBeOnSingleLineAnalyzer>
{
    #region Members

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
                                 Diagnostics(RH0360StatementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH0360MessageFormat));
    }

    #endregion // Members
}