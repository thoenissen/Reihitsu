using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0326TestData, Diagnostics(RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0326MessageFormat, 2));
    }
}