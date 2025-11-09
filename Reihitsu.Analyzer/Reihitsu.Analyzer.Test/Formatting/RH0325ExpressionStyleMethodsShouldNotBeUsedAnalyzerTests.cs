using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0325ExpressionStyleMethodsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0325ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0325ExpressionStyleMethodsShouldNotBeUsedAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0325TestData, Diagnostics(RH0325ExpressionStyleMethodsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0325MessageFormat));
    }
}