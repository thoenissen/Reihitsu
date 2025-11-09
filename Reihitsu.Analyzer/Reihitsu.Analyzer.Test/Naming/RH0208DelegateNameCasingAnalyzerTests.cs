using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0208DelegateNameCasingAnalyzer"/> and <see cref="RH0208DelegateNameCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0208DelegateNameCasingAnalyzerTests : AnalyzerTestsBase<RH0208DelegateNameCasingAnalyzer, RH0208DelegateNameCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await Verify(TestData.RH0208TestData, TestData.RH0208ResultData, Diagnostics(RH0208DelegateNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0208MessageFormat));
        }
    }
}
