using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0212PrivateFieldCasingAnalyzer"/> and <see cref="RH0212PrivateFieldCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0212PrivateFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0212PrivateFieldCasingAnalyzer, RH0212PrivateFieldCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await Verify(TestData.RH0212TestData, TestData.RH0212ResultData, Diagnostics(RH0212PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0212MessageFormat));
        }
    }
}
