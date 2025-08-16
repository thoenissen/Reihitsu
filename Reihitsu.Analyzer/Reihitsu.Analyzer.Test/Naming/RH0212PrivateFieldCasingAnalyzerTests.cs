using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0212PrivateFieldCasingAnalyzer"/> and <see cref="RH0212PrivateFieldCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0212PrivateFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0212PrivateFieldCasingAnalyzer, RH0212PrivateFieldCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                           .WithMessage(AnalyzerResources.RH0212MessageFormat);

            await VerifyCodeFixAsync(TestData.RH0212_TestData,
                                     TestData.RH0212_ResultData,
                                     expectedCase);
        }
    }
}
