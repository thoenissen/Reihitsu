using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0218ProtectedPropertyCasingAnalyzer"/> and <see cref="RH0218ProtectedPropertyCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0218ProtectedPropertyCasingAnalyzerTests : AnalyzerTestsBase<RH0218ProtectedPropertyCasingAnalyzer, RH0218ProtectedPropertyCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await VerifyCodeFixAsync(TestData.RH0218TestData, TestData.RH0218ResultData, Diagnostics(1, AnalyzerResources.RH0218MessageFormat));
        }
    }
}
