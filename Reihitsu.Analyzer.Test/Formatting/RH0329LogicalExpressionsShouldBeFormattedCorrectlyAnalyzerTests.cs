using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH0329LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer, RH0329LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        await Verify(TestData.RH0329TestData, TestData.RH0329ResultData, Diagnostics(RH0329LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0329MessageFormat, 4));
    }
}