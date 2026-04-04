using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer"/> and <see cref="RH0305WhileStatementsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer, RH0305WhileStatementsShouldBePrecededByABlankLineCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0305TestData, TestData.RH0305ResultData, Diagnostics(RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0305MessageFormat));
    }
}