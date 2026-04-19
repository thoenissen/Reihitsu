using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0219InternalPropertyCasingAnalyzer"/> and <see cref="RH0219InternalPropertyCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0219InternalPropertyCasingAnalyzerTests : AnalyzerTestsBase<RH0219InternalPropertyCasingAnalyzer, RH0219InternalPropertyCasingCodeFixProvider>
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
                                        internal int {|#0:property|} { get; set; }
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
                                         internal int Property { get; set; }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0219InternalPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0219MessageFormat));
    }
}