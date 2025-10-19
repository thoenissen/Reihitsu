using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0223DeconstructionVariableCasingAnalyzer"/> and <see cref="RH0223DeconstructionVariableCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0223DeconstructionVariableCasingAnalyzerTests : AnalyzerTestsBase<RH0223DeconstructionVariableCasingAnalyzer, RH0223DeconstructionVariableCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await VerifyCodeFixAsync(TestData.RH0223TestData, TestData.RH0223ResultData, Diagnostics(1, AnalyzerResources.RH0223MessageFormat));
        }
    }
}
