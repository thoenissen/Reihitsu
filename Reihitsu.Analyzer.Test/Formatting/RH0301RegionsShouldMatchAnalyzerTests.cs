using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0301RegionsShouldMatchAnalyzer"/> and <see cref="RH0301RegionsShouldMatchCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0301RegionsShouldMatchAnalyzerTests : AnalyzerTestsBase<RH0301RegionsShouldMatchAnalyzer, RH0301RegionsShouldMatchCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0301TestData, TestData.RH0301ResultData, Diagnostics(RH0301RegionsShouldMatchAnalyzer.DiagnosticId, AnalyzerResources.RH0301MessageFormat, 4));
    }
}