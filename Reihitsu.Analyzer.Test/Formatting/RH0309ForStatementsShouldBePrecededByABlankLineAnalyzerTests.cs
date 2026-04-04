using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0309ForStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0309ForStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer, RH0309ForStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0309TestData, TestData.RH0309ResultData, Diagnostics(RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0309MessageFormat));
    }
}