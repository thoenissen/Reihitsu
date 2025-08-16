﻿using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0203StructNameCasingAnalyzer"/> and <see cref="RH0203StructNameCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0203StructNameCasingAnalyzerTests : AnalyzerTestsBase<RH0203StructNameCasingAnalyzer, RH0203StructNameCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            var expectedCase = Diagnostic().WithLocation(0, options: DiagnosticLocationOptions.InterpretAsMarkupKey)
                                           .WithMessage(AnalyzerResources.RH0203MessageFormat);

            await VerifyCodeFixAsync(TestData.RH0203_TestData,
                                     TestData.RH0203_ResultData,
                                     expectedCase);
        }
    }
}
