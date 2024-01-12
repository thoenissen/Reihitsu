using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Design.Resources;

using Verifier = Reihitsu.Analyzer.Test.Verifiers.CSharpCodeFixVerifier<Reihitsu.Analyzer.Rules.Design.RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer, Reihitsu.Analyzer.Rules.Design.RH0101PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider>;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer"/> and <see cref="RH0101PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        var expected = Verifier.Diagnostic()
                               .WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                               .WithMessage(AnalyzerResources.RH0101MessageFormat);

        await Verifier.VerifyCodeFixAsync(TestData.RH0101_TestData,
                                          TestData.RH0101_ResultData,
                                          expected);
    }
}