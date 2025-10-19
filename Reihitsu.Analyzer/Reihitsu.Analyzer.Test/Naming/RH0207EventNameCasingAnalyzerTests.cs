﻿using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming
{
    /// <summary>
    /// Test methods for <see cref="RH0207EventNameCasingAnalyzer"/> and <see cref="RH0207EventNameCasingCodeFixProvider"/>
    /// </summary>
    [TestClass]
    public class RH0207EventNameCasingAnalyzerTests : AnalyzerTestsBase<RH0207EventNameCasingAnalyzer, RH0207EventNameCasingCodeFixProvider>
    {
        /// <summary>
        /// Verifying diagnostics
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [TestMethod]
        public async Task VerifyDiagnostics()
        {
            await VerifyCodeFixAsync(TestData.RH0207TestData, TestData.RH0207ResultData, Diagnostics(1, AnalyzerResources.RH0207MessageFormat));
        }
    }
}
