using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0205EnumMemberCasingAnalyzer"/> and <see cref="RH0205EnumMemberCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0205EnumMemberCasingAnalyzerTests : AnalyzerTestsBase<RH0205EnumMemberCasingAnalyzer, RH0205EnumMemberCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                           .WithMessage(AnalyzerResources.RH0205MessageFormat);

            await VerifyCodeFixAsync(TestData.RH0205_TestData,
                                     TestData.RH0205_ResultData,
                                     expectedCase);
        }
    }
}
