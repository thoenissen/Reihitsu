using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzerTests : AnalyzerTestsBase<RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                       .WithMessage(AnalyzerResources.RH0313MessageFormat);

        await VerifyCodeFixAsync(TestData.RH0313_TestData, expectedCase);
    }
}