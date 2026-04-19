using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0223DeconstructionVariableCasingAnalyzer"/> and <see cref="RH0223DeconstructionVariableCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0223DeconstructionVariableCasingAnalyzerTests : AnalyzerTestsBase<RH0223DeconstructionVariableCasingAnalyzer, RH0223DeconstructionVariableCasingCodeFixProvider>
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
                                        /// Test method
                                        /// </summary>
                                        public void TestMethod()
                                        {
                                            var ({|#0:FirstVariable|}, secondVariable) = (1, 2);
                                        }
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
                                         /// Test method
                                         /// </summary>
                                         public void TestMethod()
                                         {
                                             var (firstVariable, secondVariable) = (1, 2);
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0223DeconstructionVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0223MessageFormat));
    }
}