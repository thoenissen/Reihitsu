using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0214InternalFieldCasingAnalyzer"/> and <see cref="RH0214InternalFieldCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0214InternalFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0214InternalFieldCasingAnalyzer, RH0214InternalFieldCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await Verify(TestData.RH0214TestData, TestData.RH0214ResultData, Diagnostics(RH0214InternalFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0214MessageFormat));
        }
    }
}
