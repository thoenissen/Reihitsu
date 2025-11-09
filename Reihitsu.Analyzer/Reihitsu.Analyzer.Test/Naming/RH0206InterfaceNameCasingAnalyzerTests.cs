using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0206InterfaceNameCasingAnalyzer"/> and <see cref="RH0206InterfaceNameCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0206InterfaceNameCasingAnalyzerTests : AnalyzerTestsBase<RH0206InterfaceNameCasingAnalyzer, RH0206InterfaceNameCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await Verify(TestData.RH0206TestData, TestData.RH0206ResultData, Diagnostics(RH0206InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0206MessageFormat));
        }
    }
}
