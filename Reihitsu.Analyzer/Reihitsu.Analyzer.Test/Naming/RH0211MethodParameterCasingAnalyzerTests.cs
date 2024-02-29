﻿using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0211MethodParameterCasingAnalyzer"/> and <see cref="RH0211MethodParameterCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0211MethodParameterCasingAnalyzerTests : AnalyzerTestsBase<RH0211MethodParameterCasingAnalyzer, RH0211MethodParameterCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                           .WithMessage(AnalyzerResources.RH0211MessageFormat);

            await VerifyCodeFixAsync(TestData.RH0211_TestData,
                                     TestData.RH0211_ResultData,
                                     expectedCase);
        }
    }
}
