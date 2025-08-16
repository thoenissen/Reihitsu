using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
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
            var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                           .WithMessage(AnalyzerResources.RH0222MessageFormat);

            await VerifyCodeFixAsync(TestData.RH0222_TestData,
                                     TestData.RH0222_ResultData,
                                     expectedCase);
        }
    }
}
