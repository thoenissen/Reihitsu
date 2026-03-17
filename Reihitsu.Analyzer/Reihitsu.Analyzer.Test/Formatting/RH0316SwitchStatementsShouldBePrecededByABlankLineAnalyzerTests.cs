using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0316SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer, RH0316SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0316TestData, TestData.RH0316ResultData, Diagnostics(RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0316MessageFormat));
    }
}