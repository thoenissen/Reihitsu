using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0225FileScopedNamespaceCasingAnalyzer"/>
/// </summary>
[TestClass]
public class RH0225FileScopedNamespaceCasingAnalyzerTests : AnalyzerTestsBase<RH0225FileScopedNamespaceCasingAnalyzer>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        var expectedCase0 = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                        .WithMessage(AnalyzerResources.RH0225MessageFormat);
        var expectedCase1 = Diagnostic().WithLocation(1, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                        .WithMessage(AnalyzerResources.RH0225MessageFormat);
        var expectedCase2 = Diagnostic().WithLocation(2, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                        .WithMessage(AnalyzerResources.RH0225MessageFormat);

        await VerifyCodeFixAsync(TestData.RH0225_TestData, expectedCase0, expectedCase1, expectedCase2);
    }
}