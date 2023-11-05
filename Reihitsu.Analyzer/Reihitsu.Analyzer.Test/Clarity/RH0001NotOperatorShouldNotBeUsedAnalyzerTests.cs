using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Clarity;
using Reihitsu.Analyzer.Test.Clarity.Resources;

using Verifier = Reihitsu.Analyzer.Test.Verifiers.CSharpCodeFixVerifier<Reihitsu.Analyzer.Clarity.RH0001NotOperatorShouldNotBeUsedAnalyzer, Reihitsu.Analyzer.Clarity.RH0001NotOperatorShouldNotBeUsedCodeFixProvider>;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0001NotOperatorShouldNotBeUsedAnalyzer"/> and <see cref="RH0001NotOperatorShouldNotBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0001NotOperatorShouldNotBeUsedAnalyzerTests
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        var expectedLiteral = Verifier.Diagnostic()
                                      .WithLocation(0, options: Microsoft.CodeAnalysis.Testing.DiagnosticLocationOptions.InterpretAsMarkupKey)
                                      .WithMessage(AnalyzerResources.RH0001MessageFormat);

        var expectedField = Verifier.Diagnostic()
                                    .WithLocation(1)
                                    .WithMessage(AnalyzerResources.RH0001MessageFormat);

        var expectedProperty = Verifier.Diagnostic()
                                       .WithLocation(2)
                                       .WithMessage(AnalyzerResources.RH0001MessageFormat);

        var expectedMethod = Verifier.Diagnostic()
                                     .WithLocation(3)
                                     .WithMessage(AnalyzerResources.RH0001MessageFormat);

        await Verifier.VerifyCodeFixAsync(TestData.RH0001_TestData,
                                          TestData.RH0001_ResultData,
                                          expectedLiteral,
                                          expectedField,
                                          expectedProperty,
                                          expectedMethod);
    }
}