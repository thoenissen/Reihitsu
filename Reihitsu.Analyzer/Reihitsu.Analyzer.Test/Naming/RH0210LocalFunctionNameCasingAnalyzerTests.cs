using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0210LocalFunctionNameCasingAnalyzer"/> and <see cref="RH0210LocalFunctionNameCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0210LocalFunctionNameCasingAnalyzerTests : AnalyzerTestsBase<RH0210LocalFunctionNameCasingAnalyzer, RH0210LocalFunctionNameCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await VerifyCodeFixAsync(TestData.RH0210TestData, TestData.RH0210ResultData, Diagnostics(1, AnalyzerResources.RH0210MessageFormat));
        }
    }
}
