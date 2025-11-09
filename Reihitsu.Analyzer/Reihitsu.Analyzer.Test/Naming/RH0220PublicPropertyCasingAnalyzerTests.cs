using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0220PublicPropertyCasingAnalyzer"/> and <see cref="RH0220PublicPropertyCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0220PublicPropertyCasingAnalyzerTests : AnalyzerTestsBase<RH0220PublicPropertyCasingAnalyzer, RH0220PublicPropertyCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await Verify(TestData.RH0220TestData, TestData.RH0220ResultData, Diagnostics(RH0220PublicPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0220MessageFormat));
        }
    }
}
