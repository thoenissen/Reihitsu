using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5407UseBracesConsistentlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH5407UseBracesConsistentlyFormatterTests : FormatterTestsBase<RH5407UseBracesConsistentlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter applies braces consistently across chained statements
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
                                     if (true)
                                     {
                                         return;
                                     }
                                     else
                                         {|#0:return;|}
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
                                         else
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5407UseBracesConsistentlyAnalyzer.DiagnosticId, AnalyzerResources.RH5407MessageFormat));
    }

    #endregion // Tests
}