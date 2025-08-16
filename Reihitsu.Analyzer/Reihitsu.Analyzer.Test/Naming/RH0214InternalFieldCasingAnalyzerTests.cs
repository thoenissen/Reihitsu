using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0214InternalFieldCasingAnalyzer"/> and <see cref="RH0214InternalFieldCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0214InternalFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0214InternalFieldCasingAnalyzer, RH0214InternalFieldCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                           .WithMessage(AnalyzerResources.RH0214MessageFormat);

            await VerifyCodeFixAsync(TestData.RH0214_TestData,
                                     TestData.RH0214_ResultData,
                                     expectedCase);
        }
    }
}
