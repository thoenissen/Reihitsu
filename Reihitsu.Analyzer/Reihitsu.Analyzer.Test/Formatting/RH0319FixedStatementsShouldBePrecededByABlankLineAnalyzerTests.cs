using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0319FixedStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer, RH0319FixedStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0319TestData, TestData.RH0319ResultData, Diagnostics(RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0319MessageFormat));
    }
}