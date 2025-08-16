using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0204EnumNameCasingAnalyzer"/> and <see cref="RH0204EnumNameCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0204EnumNameCasingAnalyzerTests : AnalyzerTestsBase<RH0204EnumNameCasingAnalyzer, RH0204EnumNameCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                           .WithMessage(AnalyzerResources.RH0204MessageFormat);

            await VerifyCodeFixAsync(TestData.RH0204_TestData,
                                     TestData.RH0204_ResultData,
                                     expectedCase);
        }
    }
}
