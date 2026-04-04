using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0321YieldStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer, RH0321YieldStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0321TestData, TestData.RH0321ResultData, Diagnostics(RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0321MessageFormat));
    }
}