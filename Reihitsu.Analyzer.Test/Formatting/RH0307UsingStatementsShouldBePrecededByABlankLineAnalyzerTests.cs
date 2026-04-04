using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0307UsingStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer, RH0307UsingStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0307TestData, TestData.RH0307ResultData, Diagnostics(RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0307MessageFormat));
    }
}