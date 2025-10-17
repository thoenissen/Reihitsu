using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Formatting.Resources;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        var expectedCase0 = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                        .WithMessage(AnalyzerResources.RH0326MessageFormat);
        var expectedCase1 = Diagnostic().WithLocation(1, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                        .WithMessage(AnalyzerResources.RH0326MessageFormat);

        await VerifyCodeFixAsync(TestData.RH0326_TestData, expectedCase0, expectedCase1);
    }
}