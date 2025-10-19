using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0215PublicFieldCasingAnalyzer"/> and <see cref="RH0215PublicFieldCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0215PublicFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0215PublicFieldCasingAnalyzer, RH0215PublicFieldCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await VerifyCodeFixAsync(TestData.RH0215TestData, TestData.RH0215ResultData, Diagnostics(1, AnalyzerResources.RH0215MessageFormat));
        }
    }
}
