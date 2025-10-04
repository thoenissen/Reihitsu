using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Performance;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Performance.Resources;

namespace Reihitsu.Analyzer.Test.Performance;

/// <summary>
/// Test methods for <see cref="RH0501TypesUsedAsKeysMustImplementEqualityMembersAnalyzerTests"/>
/// </summary>
[TestClass]
public class RH0501TypesUsedAsKeysMustImplementEqualityMembersAnalyzerTests : AnalyzerTestsBase<RH0501TypesUsedAsKeysMustImplementEqualityMembersAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        var expectedCases = Enumerable.Range(0, 10)
                                      .Select(i => Diagnostic().WithLocation(i, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                                               .WithMessage(AnalyzerResources.RH0501MessageFormat))
                                      .ToArray();

        await VerifyCodeFixAsync(TestData.RH0501TestData, expectedCases);
    }
}