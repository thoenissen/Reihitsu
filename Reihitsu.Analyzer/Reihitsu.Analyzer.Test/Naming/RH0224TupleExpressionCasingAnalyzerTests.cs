using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0224TupleElementCasingAnalyzer"/> and <see cref="RH0224TupleElementCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0224TupleElementCasingAnalyzerTests : AnalyzerTestsBase<RH0224TupleElementCasingAnalyzer, RH0224TupleElementCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await Verify(TestData.RH0224TestData, TestData.RH0224ResultData, Diagnostics(RH0224TupleElementCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0224MessageFormat));
        }
    }
}
