using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0324MethodChainsShouldBeAlignedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0324MethodChainsShouldBeAlignedFormatterTests : FormatterTestsBase<RH0324MethodChainsShouldBeAlignedAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter wraps and aligns method chains consistently
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             using System.Linq;
                             
                             internal class Example
                             {
                                 internal int[] Filter(int[] values)
                                 {
                                     return values.Where(obj => obj > 0){|#0:.|}Select(obj => obj)
                                                  .ToArray();
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Linq;
                                 
                                 internal class Example
                                 {
                                     internal int[] Filter(int[] values)
                                     {
                                         return values.Where(obj => obj > 0)
                                                      .Select(obj => obj)
                                                      .ToArray();
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0324MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH0324MessageFormat));
    }

    #endregion // Members
}