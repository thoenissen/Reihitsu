using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0311GotoStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzer, RH0311GotoStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0311TestData, TestData.RH0311ResultData, Diagnostics(RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0311MessageFormat));
    }
}