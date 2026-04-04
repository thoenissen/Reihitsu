using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0306DoStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0306DoStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer, RH0306DoStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0306TestData, TestData.RH0306ResultData, Diagnostics(RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0306MessageFormat));
    }
}