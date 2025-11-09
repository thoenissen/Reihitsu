using System.Threading.Tasks;
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
            await Verify(TestData.RH0216TestData, TestData.RH0216ResultData, Diagnostics(RH0216ConstFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0216MessageFormat));
        }
    }
}
