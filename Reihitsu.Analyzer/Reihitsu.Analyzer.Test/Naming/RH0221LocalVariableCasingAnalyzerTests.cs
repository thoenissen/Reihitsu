using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0221LocalVariableCasingAnalyzer"/> and <see cref="RH0221LocalVariableCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0221LocalVariableCasingAnalyzerTests : AnalyzerTestsBase<RH0221LocalVariableCasingAnalyzer, RH0221LocalVariableCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await VerifyCodeFixAsync(TestData.RH0221TestData, TestData.RH0221ResultData, Diagnostics(1, AnalyzerResources.RH0221MessageFormat));
        }
    }
}
