using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
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
            var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                           .WithMessage(AnalyzerResources.RH0215MessageFormat);

            await VerifyCodeFixAsync(TestData.RH0215_TestData,
                                     TestData.RH0215_ResultData,
                                     expectedCase);
        }
    }
}
