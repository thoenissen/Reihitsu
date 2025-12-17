using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests : AnalyzerTestsBase<RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0327TestData, Diagnostics(RH0327ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH0327MessageFormat, 2));
    }
}