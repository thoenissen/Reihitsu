using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Formatting.Resources;

using Verifier = Reihitsu.Analyzer.Test.Verifiers.CSharpCodeFixVerifier<Reihitsu.Analyzer.Rules.Formatting.RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer, Reihitsu.Analyzer.Rules.Formatting.RH0302ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider>;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH0302ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        var expectedCase0 = Verifier.Diagnostic()
                                   .WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                   .WithMessage(AnalyzerResources.RH0302MessageFormat);
        var expectedCase1 = Verifier.Diagnostic()
                                   .WithLocation(1, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                   .WithMessage(AnalyzerResources.RH0302MessageFormat);
        var expectedCase2 = Verifier.Diagnostic()
                                    .WithLocation(2, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                    .WithMessage(AnalyzerResources.RH0302MessageFormat);

        await Verifier.VerifyCodeFixAsync(TestData.RH0302_TestData,
                                          TestData.RH0302_ResultData,
                                          expectedCase0,
                                          expectedCase1,
                                          expectedCase2);
    }
}