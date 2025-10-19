using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0219InternalPropertyCasingAnalyzer"/> and <see cref="RH0219InternalPropertyCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0219InternalPropertyCasingAnalyzerTests : AnalyzerTestsBase<RH0219InternalPropertyCasingAnalyzer, RH0219InternalPropertyCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await VerifyCodeFixAsync(TestData.RH0219TestData, TestData.RH0219ResultData, Diagnostics(1, AnalyzerResources.RH0219MessageFormat));
        }
    }
}
