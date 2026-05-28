using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5201MethodChainsShouldBeAlignedAnalyzer"/>
/// </summary>
[TestClass]
public class RH5201MethodChainsShouldBeAlignedFormatterTests : FormatterTestsBase<RH5201MethodChainsShouldBeAlignedAnalyzer>
{
    #region Tests

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
                                 Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    #endregion // Tests
}