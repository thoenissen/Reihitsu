using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0218ProtectedPropertyCasingAnalyzer"/> and <see cref="RH0218ProtectedPropertyCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0218ProtectedPropertyCasingAnalyzerTests : AnalyzerTestsBase<RH0218ProtectedPropertyCasingAnalyzer, RH0218ProtectedPropertyCasingCodeFixProvider>
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
                                        /// Test property
                                        /// </summary>
                                        protected int {|#0:property|} { get; set; }
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
                                         /// Test property
                                         /// </summary>
                                         protected int Property { get; set; }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0218ProtectedPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0218MessageFormat));
    }
}