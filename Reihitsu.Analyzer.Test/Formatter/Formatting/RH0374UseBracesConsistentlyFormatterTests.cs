using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0374UseBracesConsistentlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0374UseBracesConsistentlyFormatterTests : FormatterTestsBase<RH0374UseBracesConsistentlyAnalyzer>
{
    #region Members

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
                                 Diagnostics(RH0374UseBracesConsistentlyAnalyzer.DiagnosticId, AnalyzerResources.RH0374MessageFormat));
    }

    #endregion // Members
}