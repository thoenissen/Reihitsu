using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0202ClassNameCasingAnalyzer"/> and <see cref="RH0202ClassNameCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0202ClassNameCasingAnalyzerTests : AnalyzerTestsBase<RH0202ClassNameCasingAnalyzer, RH0202ClassNameCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await VerifyCodeFixAsync(TestData.RH0202TestData, TestData.RH0202ResultData, Diagnostics(1, AnalyzerResources.RH0202MessageFormat));
        }
    }
}
