using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0312TestData, Diagnostics(RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0312MessageFormat));
    }
}