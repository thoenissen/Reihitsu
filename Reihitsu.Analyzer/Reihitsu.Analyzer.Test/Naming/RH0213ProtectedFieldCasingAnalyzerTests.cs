using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0213ProtectedFieldCasingAnalyzer"/> and <see cref="RH0213ProtectedFieldCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0213ProtectedFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0213ProtectedFieldCasingAnalyzer, RH0213ProtectedFieldCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await Verify(TestData.RH0213TestData, TestData.RH0213ResultData, Diagnostics(RH0213ProtectedFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0213MessageFormat));
        }
    }
}
