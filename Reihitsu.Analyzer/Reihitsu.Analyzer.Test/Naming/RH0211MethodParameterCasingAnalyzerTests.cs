using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0211MethodParameterCasingAnalyzer"/> and <see cref="RH0211MethodParameterCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0211MethodParameterCasingAnalyzerTests : AnalyzerTestsBase<RH0211MethodParameterCasingAnalyzer, RH0211MethodParameterCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await Verify(TestData.RH0211TestData, TestData.RH0211ResultData, Diagnostics(RH0211MethodParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0211MessageFormat));
        }
    }
}
