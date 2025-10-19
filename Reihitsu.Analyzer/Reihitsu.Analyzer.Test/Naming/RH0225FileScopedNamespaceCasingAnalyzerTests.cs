using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0225FileScopedNamespaceCasingAnalyzer"/>
/// </summary>
[TestClass]
public class RH0225FileScopedNamespaceCasingAnalyzerTests : AnalyzerTestsBase<RH0225FileScopedNamespaceCasingAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await VerifyCodeFixAsync(TestData.RH0225TestData, Diagnostics(3, AnalyzerResources.RH0225MessageFormat));
    }
}