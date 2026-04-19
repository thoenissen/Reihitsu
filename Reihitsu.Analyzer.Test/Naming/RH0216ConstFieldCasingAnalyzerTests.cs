using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0216ConstFieldCasingAnalyzer"/> and <see cref="RH0216ConstFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0216ConstFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0216ConstFieldCasingAnalyzer, RH0216ConstFieldCasingCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class TestClass
                                    {
                                        /// <summary>
                                        /// Test field
                                        /// </summary>
                                        private const int {|#0:testField|} = 42;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     /// <summary>
                                     /// Test class
                                     /// </summary>
                                     public class TestClass
                                     {
                                         /// <summary>
                                         /// Test field
                                         /// </summary>
                                         private const int TestField = 42;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0216ConstFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0216MessageFormat));
    }
}