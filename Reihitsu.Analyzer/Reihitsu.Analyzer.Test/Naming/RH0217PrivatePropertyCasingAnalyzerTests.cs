﻿using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0217PrivatePropertyCasingAnalyzer"/> and <see cref="RH0217PrivatePropertyCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0217PrivatePropertyCasingAnalyzerTests : AnalyzerTestsBase<RH0217PrivatePropertyCasingAnalyzer, RH0217PrivatePropertyCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                           .WithMessage(AnalyzerResources.RH0217MessageFormat);

            await VerifyCodeFixAsync(TestData.RH0217_TestData,
                                     TestData.RH0217_ResultData,
                                     expectedCase);
        }
    }
}