using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Design.Resources;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer"/> and <see cref="RH0101PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer, RH0101PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0101TestData, TestData.RH0101ResultData, Diagnostics(RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0101MessageFormat));
    }
}