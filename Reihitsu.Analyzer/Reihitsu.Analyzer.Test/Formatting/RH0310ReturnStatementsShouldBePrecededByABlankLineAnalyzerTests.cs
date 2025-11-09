using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0310TestData, Diagnostics(RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0310MessageFormat));
    }
}