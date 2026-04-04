using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0317CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer, RH0317CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0317TestData, TestData.RH0317ResultData, Diagnostics(RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0317MessageFormat));
    }
}