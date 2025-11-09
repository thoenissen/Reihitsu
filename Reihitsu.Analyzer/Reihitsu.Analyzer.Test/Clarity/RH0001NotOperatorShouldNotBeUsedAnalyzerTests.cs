using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Clarity.Resources;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0001NotOperatorShouldNotBeUsedAnalyzer"/> and <see cref="RH0001NotOperatorShouldNotBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0001NotOperatorShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0001NotOperatorShouldNotBeUsedAnalyzer, RH0001NotOperatorShouldNotBeUsedCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0001TestData, TestData.RH0001ResultData, Diagnostics(RH0001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0001MessageFormat, 4));
    }
}