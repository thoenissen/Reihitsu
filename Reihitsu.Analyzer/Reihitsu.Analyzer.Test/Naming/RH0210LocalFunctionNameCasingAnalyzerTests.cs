using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0210LocalFunctionNameCasingAnalyzer"/> and <see cref="RH0210LocalFunctionNameCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0210LocalFunctionNameCasingAnalyzerTests : AnalyzerTestsBase<RH0210LocalFunctionNameCasingAnalyzer, RH0210LocalFunctionNameCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                           .WithMessage(AnalyzerResources.RH0210MessageFormat);

            await VerifyCodeFixAsync(TestData.RH0210_TestData,
                                     TestData.RH0210_ResultData,
                                     expectedCase);
        }
    }
}
