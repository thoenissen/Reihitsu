using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Formatting.Resources;

using Verifier = Reihitsu.Analyzer.Test.Verifiers.CSharpCodeFixVerifier<Reihitsu.Analyzer.Rules.Formatting.RH0301RegionsShouldMatchAnalyzer, Reihitsu.Analyzer.Rules.Formatting.RH0301RegionsShouldMatchCodeFixProvider>;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0301RegionsShouldMatchAnalyzer"/> and <see cref="RH0301RegionsShouldMatchCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0301RegionsShouldMatchAnalyzerTests
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        var expectedCase1 = Verifier.Diagnostic()
                                    .WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                    .WithMessage(AnalyzerResources.RH0301MessageFormat);

        var expectedCase2 = Verifier.Diagnostic()
                                    .WithLocation(1, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                    .WithMessage(AnalyzerResources.RH0301MessageFormat);

        var expectedCase3 = Verifier.Diagnostic()
                                    .WithLocation(2, options: DiagnosticLocationOptions.InterpretAsMarkupKey)

                                    .WithMessage(AnalyzerResources.RH0301MessageFormat);
        var expectedCase4 = Verifier.Diagnostic()
                                    .WithLocation(3, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                    .WithMessage(AnalyzerResources.RH0301MessageFormat);

        await Verifier.VerifyCodeFixAsync(TestData.RH0301_TestData,
                                          TestData.RH0301_ResultData,
                                          expectedCase1,
                                          expectedCase2,
                                          expectedCase3,
                                          expectedCase4);
    }
}