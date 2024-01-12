using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Documentation.Resources;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH0401InheritdocShouldBeUsedAnalyzer"/> and <see cref="RH0401InheritdocShouldBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0401InheritdocShouldBeUsedAnalyzerTests : AnalyzerTestsBase<RH0401InheritdocShouldBeUsedAnalyzer, RH0401InheritdocShouldBeUsedCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        var expected1 = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                   .WithMessage(AnalyzerResources.RH0401MessageFormat);
        var expected2 = Diagnostic().WithLocation(1, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                   .WithMessage(AnalyzerResources.RH0401MessageFormat);
        var expected3 = Diagnostic().WithLocation(2, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                   .WithMessage(AnalyzerResources.RH0401MessageFormat);
        var expected4 = Diagnostic().WithLocation(3, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                   .WithMessage(AnalyzerResources.RH0401MessageFormat);

        await VerifyCodeFixAsync(TestData.RH0401_TestData,
                                 TestData.RH0401_ResultData,
                                 expected1,
                                 expected2,
                                 expected3,
                                 expected4);
    }
}