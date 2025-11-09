using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0308TestData, Diagnostics(RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0308MessageFormat));
    }
}