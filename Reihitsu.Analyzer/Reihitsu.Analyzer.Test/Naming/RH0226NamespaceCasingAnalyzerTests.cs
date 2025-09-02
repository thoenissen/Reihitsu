using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0226NamespaceCasingAnalyzerTests"/>
/// </summary>
[TestClass]
public class RH0226NamespaceCasingAnalyzerTests : AnalyzerTestsBase<RH0226NamespaceCasingAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        var expectedCase0 = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                        .WithMessage(AnalyzerResources.RH0226MessageFormat);
        var expectedCase1 = Diagnostic().WithLocation(1, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                        .WithMessage(AnalyzerResources.RH0226MessageFormat);
        var expectedCase2 = Diagnostic().WithLocation(2, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                        .WithMessage(AnalyzerResources.RH0226MessageFormat);

        await VerifyCodeFixAsync(TestData.RH0226_TestData, expectedCase0, expectedCase1, expectedCase2);
    }
}