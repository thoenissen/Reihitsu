using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Documentation.Resources;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH0401InheritdocShouldBeUsedAnalyzer"/> and <see cref="RH0401InheritdocShouldBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0401InheritdocShouldBeUsedAnalyzerTests : AnalyzerTestsBase<RH0401InheritdocShouldBeUsedAnalyzer, RH0401InheritdocShouldBeUsedCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await VerifyCodeFixAsync(TestData.RH0401TestData, TestData.RH0401ResultData, Diagnostics(4, AnalyzerResources.RH0401MessageFormat));
    }
}