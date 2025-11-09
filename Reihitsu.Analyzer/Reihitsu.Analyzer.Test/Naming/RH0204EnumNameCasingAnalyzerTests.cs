using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0204EnumNameCasingAnalyzer"/> and <see cref="RH0204EnumNameCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0204EnumNameCasingAnalyzerTests : AnalyzerTestsBase<RH0204EnumNameCasingAnalyzer, RH0204EnumNameCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await Verify(TestData.RH0204TestData, TestData.RH0204ResultData, Diagnostics(RH0204EnumNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0204MessageFormat));
        }
    }
}
