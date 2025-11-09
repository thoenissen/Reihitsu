using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0309ForStatementsShouldBePrecededByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0309TestData, Diagnostics(RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0309MessageFormat));
    }
}