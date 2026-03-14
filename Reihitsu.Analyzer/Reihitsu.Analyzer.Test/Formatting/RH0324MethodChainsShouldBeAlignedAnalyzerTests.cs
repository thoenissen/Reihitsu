using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0324MethodChainsShouldBeAlignedAnalyzer"/> and <see cref="RH0324MethodChainsShouldBeAlignedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0324MethodChainsShouldBeAlignedAnalyzerTests : AnalyzerTestsBase<RH0324MethodChainsShouldBeAlignedAnalyzer, RH0324MethodChainsShouldBeAlignedCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0324TestData, TestData.RH0324ResultData, Diagnostics(RH0324MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH0324MessageFormat, 7));
    }
}
