using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0222TupleElementCasingAnalyzer"/> and <see cref="RH0222TupleElementCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0222TupleElementCasingAnalyzerTests : AnalyzerTestsBase<RH0222TupleElementCasingAnalyzer, RH0222TupleElementCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await Verify(TestData.RH0222TestData, TestData.RH0222ResultData, Diagnostics(RH0222TupleElementCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0222MessageFormat));
        }
    }
}
