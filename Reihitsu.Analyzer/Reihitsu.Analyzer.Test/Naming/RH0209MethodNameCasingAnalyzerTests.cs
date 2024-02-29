using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0209MethodNameCasingAnalyzer"/> and <see cref="RH0209MethodNameCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0209MethodNameCasingAnalyzerTests : AnalyzerTestsBase<RH0209MethodNameCasingAnalyzer, RH0209MethodNameCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                           .WithMessage(AnalyzerResources.RH0209MessageFormat);

            await VerifyCodeFixAsync(TestData.RH0209_TestData,
                                     TestData.RH0209_ResultData,
                                     expectedCase);
        }
    }
}
