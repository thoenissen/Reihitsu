using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
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
            var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                           .WithMessage(AnalyzerResources.RH0202MessageFormat);

            await VerifyCodeFixAsync(TestData.RH0202_TestData,
                                     TestData.RH0202_ResultData,
                                     expectedCase);
        }
    }
}
