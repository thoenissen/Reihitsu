using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH0302ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer, RH0302ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0302TestData, TestData.RH0302ResultData, Diagnostics(RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0302MessageFormat, 3));
    }
}