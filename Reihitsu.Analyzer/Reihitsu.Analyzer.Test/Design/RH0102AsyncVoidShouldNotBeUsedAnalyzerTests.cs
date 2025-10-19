using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Design.Resources;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0102AsyncVoidShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0102AsyncVoidShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0102AsyncVoidShouldNotBeUsedAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await VerifyCodeFixAsync(TestData.RH0102TestData, Diagnostics(4, AnalyzerResources.RH0102MessageFormat));
    }
}