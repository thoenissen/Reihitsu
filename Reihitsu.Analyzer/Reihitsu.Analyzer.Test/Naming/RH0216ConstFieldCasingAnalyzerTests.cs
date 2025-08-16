using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0216ConstFieldCasingAnalyzer"/> and <see cref="RH0216ConstFieldCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0216ConstFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0216ConstFieldCasingAnalyzer, RH0216ConstFieldCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                           .WithMessage(AnalyzerResources.RH0216MessageFormat);

            await VerifyCodeFixAsync(TestData.RH0216_TestData,
                                     TestData.RH0216_ResultData,
                                     expectedCase);
        }
    }
}
