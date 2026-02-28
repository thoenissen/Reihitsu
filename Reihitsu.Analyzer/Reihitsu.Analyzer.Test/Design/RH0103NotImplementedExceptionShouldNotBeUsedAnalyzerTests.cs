using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Design.Resources;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0103NotImplementedExceptionShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0103NotImplementedExceptionShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0103NotImplementedExceptionShouldNotBeUsedAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0103TestData, Diagnostics(RH0103NotImplementedExceptionShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0103MessageFormat, 3));
    }
}
